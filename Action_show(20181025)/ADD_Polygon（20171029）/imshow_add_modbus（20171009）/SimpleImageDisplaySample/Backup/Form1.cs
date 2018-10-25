using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Jai_FactoryDotNET;

namespace SimpleImageDisplaySample
{
   public partial class Form1 : Form
   {
      // Main factory object
      CFactory myFactory = new CFactory();

      // Opened camera obejct
      CCamera myCamera;

      // GenICam nodes
      CNode myWidthNode;
      CNode myHeightNode;
      CNode myGainNode;

      public Form1()
      {
         InitializeComponent();

         Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

         // Open the factory with the default Registry database
         error = myFactory.Open("");

         // Search for cameras and update all controls
         SearchButton_Click(null, null);
      }

      private void WidthNumericUpDown_ValueChanged(object sender, EventArgs e)
      {
         if (myWidthNode != null)
            myWidthNode.Value = int.Parse(WidthNumericUpDown.Value.ToString());
      }

      private void HeightNumericUpDown_ValueChanged(object sender, EventArgs e)
      {
         if (myHeightNode != null)
            myHeightNode.Value = int.Parse(HeightNumericUpDown.Value.ToString());
      }

      private void GainTrackBar_Scroll(object sender, EventArgs e)
      {
         if (myGainNode != null)
            myGainNode.Value = int.Parse(GainTrackBar.Value.ToString());

         GainLabel.Text = myGainNode.Value.ToString();
      }

      private void StartButton_Click(object sender, EventArgs e)
      {
         if (myCamera != null)
            myCamera.StartImageAcquisition(true, 5);
      }

      private void StopButton_Click(object sender, EventArgs e)
      {
         if (myCamera != null)
            myCamera.StopImageAcquisition();
      }

      private void SearchButton_Click(object sender, EventArgs e)
      {
         // Search for any new cameras using Filter Driver
         myFactory.UpdateCameraList(Jai_FactoryDotNET.CFactory.EDriverType.FilterDriver);

         if (myFactory.CameraList.Count > 0)
         {
            // Open the camera
            myCamera = myFactory.CameraList[0];

            CameraIDTextBox.Text = myCamera.CameraID;

            myCamera.Open();

            StartButton.Enabled = true;
            StopButton.Enabled = true;
            int currentValue = 0;

            // Get the Width GenICam Node
            myWidthNode = myCamera.GetNode("Width");
            if (myWidthNode != null)
            {
               currentValue = int.Parse(myWidthNode.Value.ToString());

               // Update range for the Numeric Up/Down control
               // Convert from integer to Decimal type
               WidthNumericUpDown.Maximum = decimal.Parse(myWidthNode.Max.ToString());
               WidthNumericUpDown.Minimum = decimal.Parse(myWidthNode.Min.ToString());
               WidthNumericUpDown.Value = decimal.Parse(currentValue.ToString());

               WidthNumericUpDown.Enabled = true;
            }
            else
               WidthNumericUpDown.Enabled = false;

            // Get the Height GenICam Node
            myHeightNode = myCamera.GetNode("Height");
            if (myHeightNode != null)
            {
               currentValue = int.Parse(myHeightNode.Value.ToString());

               // Update range for the Numeric Up/Down control
               // Convert from integer to Decimal type
               HeightNumericUpDown.Maximum = decimal.Parse(myHeightNode.Max.ToString());
               HeightNumericUpDown.Minimum = decimal.Parse(myHeightNode.Min.ToString());
               HeightNumericUpDown.Value = decimal.Parse(currentValue.ToString());

               HeightNumericUpDown.Enabled = true;
            }
            else
               HeightNumericUpDown.Enabled = false;

            // Get the GainRaw GenICam Node
            myGainNode = myCamera.GetNode("GainRaw");
            if (myGainNode != null)
            {
               currentValue = int.Parse(myGainNode.Value.ToString());

               // Update range for the TrackBar Controls
               GainTrackBar.Maximum = int.Parse(myGainNode.Max.ToString());
               GainTrackBar.Minimum = int.Parse(myGainNode.Min.ToString());
               GainTrackBar.Value = currentValue;
               GainLabel.Text = myGainNode.Value.ToString();

               GainLabel.Enabled = true;
               GainTrackBar.Enabled = true;
            }
            else
            {
               GainLabel.Enabled = false;
               GainTrackBar.Enabled = false;
            }
         }
         else
         {
            StartButton.Enabled = true;
            StopButton.Enabled = true;
            WidthNumericUpDown.Enabled = false;
            HeightNumericUpDown.Enabled = true;
            GainLabel.Enabled = false;
            GainTrackBar.Enabled = false;

            MessageBox.Show("No Cameras Found!");
         }
      }

      private void Form1_FormClosing(object sender, FormClosingEventArgs e)
      {
         if (myCamera != null)
            myCamera.Close();
      }
   }
}