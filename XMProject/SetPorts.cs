using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

//Serial System namespace
using System.IO.Ports;


namespace SimpleImageDisplaySample
{
    public partial class SetPorts : Form
    {
        public SetPorts()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            MainForm.strPortName = ComboNumber.Text;
            MainForm.strBaudRate = ComboBaudRate.Text;
            MainForm.strDataBits = ComboDataBits.Text;
            MainForm.strStopBits = ComboStopBit.Text;
            DialogResult = DialogResult.OK;
        }

        private void btnConcel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SetPorts_Load(object sender, EventArgs e)
        {
            //SerialPorts
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                ComboNumber.Items.Add(port);
            }
            ComboNumber.SelectedIndex = 0;

            //BaudRate
            ComboBaudRate.Items.Add("110");
            ComboBaudRate.Items.Add("300");
            ComboBaudRate.Items.Add("1200");
            ComboBaudRate.Items.Add("2400");
            ComboBaudRate.Items.Add("4800");
            ComboBaudRate.Items.Add("9600");
            ComboBaudRate.Items.Add("19200");
            ComboBaudRate.Items.Add("38400");
            ComboBaudRate.Items.Add("57600");
            ComboBaudRate.Items.Add("115200");
            ComboBaudRate.Items.Add("230400");
            ComboBaudRate.Items.Add("460800");
            ComboBaudRate.Items.Add("921600");
            ComboBaudRate.SelectedIndex = 9;

            //DataBits
            ComboDataBits.Items.Add("5");
            ComboDataBits.Items.Add("6");
            ComboDataBits.Items.Add("7");
            ComboDataBits.Items.Add("8");
            ComboDataBits.SelectedIndex = 3;

            //StopBit
            //这里的通讯的停止位机械爪是给的“1”
            ComboStopBit.Items.Add("1");
            ComboStopBit.SelectedIndex = 0;

            //ParityBit
            ComboParityBit.Items.Add("无");
        }

       
    }
}
