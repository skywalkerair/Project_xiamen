using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using matrix_test;
using System.Runtime.InteropServices;

namespace SimpleImageDisplaySample
{
    public partial class calib_C : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        double fc1, fc2, cc1, cc2, R11, R12, R13, R21, R22, R23, T1, T2, T3,s;
        int point_x, point_y;
      
        double[,] c = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] c_ = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] world_cor = new double[3, 1] { { 0 }, { 0 }, { 1 } };
        public calib_C()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string s1 = textBox1.Text;
                fc1 = double.Parse(s1);
                string s2 = textBox2.Text;
                fc2 = double.Parse(s2);
                string s3 = textBox3.Text;
                cc1 = double.Parse(s3);
                string s4 = textBox4.Text;
                cc2 = double.Parse(s4);
                string s5 = textBox5.Text;
                R11 = double.Parse(s5);
                string s6 = textBox6.Text;
                R12 = double.Parse(s6);
                string s7 = textBox7.Text;
                R13 = double.Parse(s7);
                string s8 = textBox8.Text;
                R21 = double.Parse(s8);
                string s9 = textBox9.Text;
                R22 = double.Parse(s9);
                string s10 = textBox10.Text;
                R23 = double.Parse(s10);
                string s11 = textBox11.Text;
                T1 = double.Parse(s11);
                string s12 = textBox12.Text;
                T2 = double.Parse(s12);
                string s13 = textBox13.Text;
                T3 = double.Parse(s13);
                string s14 = textBox14.Text;
                s = double.Parse(s14);
                string s15 = textBox15.Text;
                point_x = int.Parse(s15);
                string s16 = textBox16.Text;
                point_y = int.Parse(s16);
            
                double[,] a = new double[3, 3] { { fc1, 0, cc1 }, { 0, fc2, cc2 }, { 0, 0, 1 } };
                double[,] b = new double[3, 3] { { R11, R21, T1 }, { R12, R22, T2 }, { R13, R23, T3 } };
                double[,] image_pix = new double[3, 1] { { point_x }, { point_y }, { 1 } };
                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);          
                textBox17.Text = ((world_cor[0, 0]/s)*1000).ToString();
                textBox18.Text = ((world_cor[1, 0]/s)*1000).ToString();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WritePrivateProfileString("标定", "fc1", textBox1.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "fc2", textBox2.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "cc1", textBox3.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "cc2", textBox4.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "R11", textBox5.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "R12", textBox6.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "R13", textBox7.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "R21", textBox8.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "R22", textBox9.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "R23", textBox10.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "T1", textBox11.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "T2", textBox12.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "T3", textBox13.Text, Application.StartupPath + "/calib.ini");
            WritePrivateProfileString("标定", "s", textBox14.Text, Application.StartupPath + "/calib.ini");
            Form1.fc1 = fc1;
            Form1.fc2 = fc2;
            Form1.cc1 = cc1;
            Form1.cc2 = cc2;
            Form1.R11 = R11;
            Form1.R12 = R12;
            Form1.R13 = R13;
            Form1.R21 = R21;
            Form1.R22 = R22;
            Form1.R23 = R23;
            Form1.T1 = T1;
            Form1.T2 = T2;
            Form1.T3 = T3;
            Form1.s = s;
        }

    }
}
