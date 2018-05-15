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

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            if (wavdata == null) return;

            Graphics g = e.Graphics;

            for(int i = 1; i < wavdata.Length; i++)
                g.DrawLine(Pens.Black, getX(i-1), getY(wavdata[i-1]), getX(i), getY(wavdata[i]));
        }
    }
}
