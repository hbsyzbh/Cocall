using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CoCall
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool checkFileIsValied(String filepath)
        {
            if (filepath == "")
            {
                MessageBox.Show("Please Select WAV file");
                return false;
            }

            if (!File.Exists(filepath))
            {
                MessageBox.Show("Can not read file:" + filepath);
                return false;
            }

            try
            {
                FileStream wav = System.IO.File.OpenRead(filepath);

                if (wav.Length >= 8 * 1000 * 1000 / 8)
                {
                    MessageBox.Show("File too Big!");
                    return false;
                }

                byte[] wavHead = new byte[64];
                wav.Read(wavHead, 0, 40);
                Byte a = (byte)'R';
                if ((wavHead[0] == (byte)'R' )&&
                    (wavHead[1] == (byte)'I') &&
                    (wavHead[2] == (byte)'F') &&
                    (wavHead[3] == (byte)'F') &&
                    (wavHead[8] == (byte)'W') &&
                    (wavHead[9] == (byte)'A') &&
                    (wavHead[10] == (byte)'V') &&
                    (wavHead[11] == (byte)'E') &&
                    (wavHead[12] == (byte)'f') &&
                    (wavHead[13] == (byte)'m') &&
                    (wavHead[14] == (byte)'t')
                ) {
                    ulong sample = wavHead[24];
                    sample += ((ulong)(wavHead[25]) << 8);
                    sample += ((ulong)(wavHead[26]) << 16);
                    sample += ((ulong)(wavHead[27]) << 24);
                    
                    ulong BitsPerSample = wavHead[34];
                    BitsPerSample += ((ulong)(wavHead[35]) << 8);

                    if (sample != 11025)
                    {
                        ;
                        //MessageBox.Show("SampleRate is not 11.025K!");
                        //return false;
                    }

                    if ((BitsPerSample != 8) && (BitsPerSample != 16))
                    {
                        MessageBox.Show("Only support 8bits and 16bits now!");
                        return false;
                    }


                } else {
                    MessageBox.Show("Not a recognized wav file!");
                    return false;
                }



            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                return false;
            }

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "WAV(*.wav)|*.wav|All(*.*)|*.*";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                textBox1.Text = dlg.FileName;

                if ( ! checkFileIsValied(textBox1.Text))
                {
                    textBox1.Text = "";    
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("iexplore.exe", "http://www.cocall.ca");
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private ushort CRC16_XMODEM(byte []puchMsg, uint usDataLen)  
        {  
            ushort wCRCin = 0x0000;  
            ushort wCPoly = 0x1021;
            ushort wChar = 0;
            int pos = 0;

            while ( pos < usDataLen)     
            {
                wChar = puchMsg[pos++];
                wCRCin ^= (ushort)(wChar << 8);  
                for(int i = 0;i < 8;i++)  
                {  
                    if((wCRCin & 0x8000) != 0 )
                    wCRCin = (ushort)((wCRCin << 1) ^ wCPoly);  
                    else
                        wCRCin = (ushort)(wCRCin << 1);  
                }
            }  
            return (wCRCin) ;  
        }  

        private byte[] makeXmodeFrame(byte framenumber, byte []data, int len)
        {
/*
 * 
SOH              0x01          //Xmodem数据头
STX              0x02           //1K-Xmodem数据头
EOT              0x04           //发送结束
ACK             0x06           //认可响应
NAK             0x15           //不认可响应
CAN             0x18           //撤销传送
CTRLZ         0x1A          //填充数据包  
                                                       Xmodem包格式
       Byte1                      Byte2                  Byte3                 Byte4~131            Byte132~133

    Start Of Header          Packet Number          ~(Packet Number)          Packet Data            16-Bit CR
 */

            byte[] a = new byte[133];

            a[0] = 0x01;
            a[1] = framenumber;
            a[2] = (byte)~framenumber;

            for(int i = 0; i<128; i++)
            {
                if (i < len)
                    a[3 + i] = data[i];
                else
                    a[3 + i] = 0x1A;
            }

            byte []tmp = new byte[128];
            for(int i =0; i < 128; i++)
                tmp[i] = a[3+i];

            ushort crcresule = CRC16_XMODEM(tmp, 128);

            a[131] = (byte)(crcresule / 256);
            a[132] = (byte)(crcresule % 256);

            return a;
        }

        private bool tryHandShakeOK()
        {
            for (int i = 0; i < 3; i++)
            {
                serialPort1.ReadTimeout = 1000;
                serialPort1.DiscardInBuffer();
                serialPort1.Write("C");
                try
                {
                    int rec = serialPort1.ReadByte();
                    if (rec == 0x06)
                        return true;
                }
                catch (Exception err)
                {
                    ;
                }
            }

            return false;
        }

        private bool askEndOk()
        {
            byte [] buff = new byte[1];

            buff[0] = 0x04;
            serialPort1.DiscardInBuffer();
            serialPort1.Write(buff, 0, 1);
            try
            {
                int rec = serialPort1.ReadByte();
                if (rec == 0x04)
                    return true;
            }
            catch (Exception err)
            {
                ;
            }

            return false;
        }

        private void trySendWaveFileUnderXModem(bool bDebug = false)
        {
            try
            {
                byte []data = new byte [128];
                byte packetnum = 1;
                int len;
                int offset = 0;
                FileStream wav = System.IO.File.OpenRead(textBox1.Text);
                wav.Seek(0, SeekOrigin.Begin);
                long total = wav.Length;

                if (!bDebug)
                {
                    if (!tryHandShakeOK())
                    {
                        throw new Exception("COMMU error");
                        //return;
                    }
                }

                while (0 != (len = wav.Read(data, 0, 128)))
                {
                    byte[] buff = makeXmodeFrame(packetnum++, data, len);
                    //MessageBox.Show(buff.ToString());

                    for (; ; )
                    {
                        int rec = 0x06;

                        if (!bDebug)
                        {
                            serialPort1.Write(buff, 0, buff.Length);
                            rec = serialPort1.ReadByte();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(BitConverter.ToString(buff));
                        }

                        if (rec == 0x06)  //ack
                        {
                            offset += len;
                            progressBar1.Value = (int)((double)offset * (double)100 / (double)total);
                            break;
                        }
                        else if (rec == 0x15)//nak
                        {
                            continue;
                        }
                        else
                        {
                            throw new Exception("commu error");
                            //return;
                        }
                    }

                    if (!bDebug)
                    {
                        serialPort1.DiscardInBuffer(); //容错，抗干扰。
                    }
                }

                if (!bDebug)
                {
                    if (!askEndOk())
                    {
                        throw new Exception("File transfer OK, but saving seems not done.");
                    }
                    else
                    {
                        MessageBox.Show("Transfer done!");
                    }
                }
                wav.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "")
            {
                MessageBox.Show("No COM port selected!!!");
                return;
            }

            if (! checkFileIsValied(textBox1.Text))
            {
                return;
            }

            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.Open();
                trySendWaveFileUnderXModem();
                serialPort1.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            refreshcomport();
        }

        private void refreshcomport()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            comboBox1.Items.Clear();
            foreach (string str in ports)
            {
                comboBox1.Items.Add(str);
            }
        }


        private void comboBox1_Click(object sender, EventArgs e)
        {
            refreshcomport();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trySendWaveFileUnderXModem(true);
            Form2 a = new Form2(textBox1.Text);
            a.ShowDialog();
        }
    }
}
