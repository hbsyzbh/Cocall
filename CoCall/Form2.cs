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
    public partial class Form2 : Form
    {
        String filepath;
        byte[] wavdata = null;
        int BitsPerSample = 0;
        public Form2(String p_filepath = "")
        {
            filepath = p_filepath;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            long orig = this.Height / 2;
            try
            {
                FileStream wav = File.OpenRead(filepath);
                wav.Seek(34, SeekOrigin.Begin);
                BitsPerSample = wav.ReadByte();
                wav.Seek(40, SeekOrigin.Begin);
                int size = wav.ReadByte();
                size += wav.ReadByte() << 8;
                size += wav.ReadByte() << 16;
                size += wav.ReadByte() << 24;
                wavdata = new byte[size];

                wav.Read(wavdata, 0, size);

                wav.Close();
            }
            catch (Exception err)
            {
                ;    
            }
        }

        private int getX(int x)
        {
            return (x * this.Width / wavdata.Length);
        }

        private int getY(byte y)
        {
            return ((256 - y) * this.Height / 256 );
        }


        private int get16X(int x)
        {
            return (x * this.Width / (wavdata.Length / 2));
        }

        private int get16Y(int y)
        {
            return ((65536/2 - y) * this.Height / 65536);
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            if (wavdata == null) return;

            Graphics g = e.Graphics;

            if (BitsPerSample == 8)
            {
                for (int i = 1; i < wavdata.Length; i++)
                    g.DrawLine(Pens.Black, getX(i - 1), getY(wavdata[i - 1]), getX(i), getY(wavdata[i]));
            }
            else if (BitsPerSample == 16)
            {
                for (int i = 2; i < wavdata.Length; i += 2)
                {
                    int x0, x1, y0, y1;

                    x0 = (i / 2) - 1; x1 = i / 2;
                    y0 = (short)(wavdata[i - 2] + wavdata[i - 1] * 256);
                    y1 = (short)(wavdata[i] + wavdata[i+1] * 256);
                    g.DrawLine(Pens.Black, get16X(x0), get16Y(y0), get16X(x1), get16Y(y1));
                }
            }
        }
    }
}
