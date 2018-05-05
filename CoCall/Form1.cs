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

            return a;
        }

        private void trySendWaveFileUnderXModem()
        {
            try
            {
                byte []data = new byte [128];
                byte packetnum = 0;
                int len;
                int offset = 0;
                FileStream wav = System.IO.File.OpenRead(textBox1.Text);

                long total = wav.Length;


                while (0 != (len = wav.Read(data, 0, 128)))
                {
                    offset += len;
                    byte[] buff = makeXmodeFrame(packetnum++, data, len);
                    MessageBox.Show(buff.ToString());
                    progressBar1.Value = (int)((double)offset * (double)100 / (double)total);
                }
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
    }
}
