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

        private void button3_Click(object sender, EventArgs e)
        {

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
