using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
//using System.Threading;
using System.Runtime.InteropServices; // �����[DllImport("kernel32")]
using System.Drawing.Imaging;
using System.Diagnostics;

using matrix_test;

//����JAI������
using Jai_FactoryDotNET;

//����flycapture������
//using System.Diagnostics;   
using FlyCapture2Managed;
using FlyCapture2Managed.Gui;

//�ָ��ַ���
using System.Text.RegularExpressions;



namespace SimpleImageDisplaySample
{
    public partial class Form1 : Form,ILog,IDisposable
    {
        #region ��ʼ��ȫ�ֱ���
        //ͼ����������������--������
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /**********�����ʼ��***********/
        #region 0.����ģ��
        public static double fc1, fc2, cc1, cc2, R11, R12, R13, R21, R22, R23, T1, T2, T3, s;
        //ͼ�����������������ʼ��
      
        //����һ��Flag�����־��κ�Բ��
        int Flag = 0;
        int Flag_t = 1;
  
        int AreaCircle;

        int point_X, point_Y;
        int point_X_circle, point_Y_circle;

        double world_X, world_Y;
        //TODO
        double world_X_circle, world_Y_circle; 

        double[,] c = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] c_ = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] world_cor = new double[3, 1] { { 0 }, { 0 }, { 1 } };
        #endregion
        #region 1.FLY����ĳ�ʼ��
        private FlyCapture2Managed.Gui.CameraControlDialog m_camCtlDlg;
        private ManagedCameraBase m_camera = null;
        private ManagedImage m_rawImage;
        private ManagedImage m_processedImage;
        private bool m_grabImages;
        private AutoResetEvent m_grabThreadExited;
        private BackgroundWorker m_grabThread;
        #endregion
        #region 2.JAI�����ͼ����
        //ͼ����--Բ
        public CircleF circle;
        public CircleF[] circles;
        //ͼ����--����
        public MCvBox2D box1;
        public List<MCvBox2D> boxList;
        
        //JAI���
        CFactory myFactory = new CFactory();
         
        // Opened camera obejct
        CCamera myCamera1;
        //CCamera myCamera2;
        //Jai_FactoryWrapper.EFactoryError error;
        

        /***********Modbus--Tcp*********/
        private ModBusWrapper Wrapper = null;
        #endregion 
        #endregion

        public Form1 ()
        {         
            InitializeComponent(); //���ڵĳ�ʼ����������һЩ��
            #region 0.Modbus��ʼ��
            /*************JAI__init***************/
            //Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;
            // Open the factory with the default Registry database
            //error = myFactory.Open("");
            // Search for cameras and update all controls
            //SearchButton_Click(null, null);

            /***********Modbus--TCP**************/
            this.Wrapper = ModBusWrapper.CreateInstance(Protocol.TCPIP);
            this.Wrapper.Logger = this;
            #endregion
            #region 1.Fly���__init

            m_rawImage = new ManagedImage();
            m_processedImage = new ManagedImage();
            m_camCtlDlg = new CameraControlDialog();
            m_grabThreadExited = new AutoResetEvent(false);  //����ֹ

            Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

            // Open the factory with the default Registry database
            error = myFactory.Open("");

            // Search for cameras and update all controls
            SearchButton_Click(null, null);
            #endregion
            #region 2.ע��--Fly���__hide()
        //    Hide();
        //    CameraSelectionDialog camSlnDlg = new CameraSelectionDialog();
        //    bool retVal = camSlnDlg.ShowModal();
        //    if  (retVal)
        //    {
        //       try
        //        {
        //            ManagedPGRGuid[] selectedGuids = camSlnDlg.GetSelectedCameraGuids();
        //            ManagedPGRGuid guidToUse = selectedGuids[0];

        //            ManagedBusManager busMgr = new ManagedBusManager();
        //            InterfaceType ifType = busMgr.GetInterfaceTypeFromGuid(guidToUse);

        //            if (ifType == InterfaceType.GigE)
        //            {
        //                m_camera = new ManagedGigECamera();
        //            }
        //            else
        //            {
        //                m_camera = new ManagedCamera();
        //            }

        //            // Connect to the first selected GUID
        //            m_camera.Connect(guidToUse);

        //            m_camCtlDlg.Connect(m_camera);    //

        //            CameraInfo camInfo = m_camera.GetCameraInfo();
        //            //UpdateFormCaption(camInfo);

        //            // Set embedded timestamp to on
        //            EmbeddedImageInfo embeddedInfo = m_camera.GetEmbeddedImageInfo();
        //            embeddedInfo.timestamp.onOff = true;
        //            m_camera.SetEmbeddedImageInfo(embeddedInfo);

        //            m_camera.StartCapture();

        //            m_grabImages = true;

        //            StartGrabLoop();
        //        }
        //        catch (FC2Exception ex)
        //        {
        //            Debug.WriteLine("Failed to load form successfully: " + ex.Message);
        //            Environment.ExitCode = -1;
        //            Application.Exit();
        //            return;
        //        }

        //        toolStripButtonStart.Enabled = false;
        //        toolStripButtonStop.Enabled = true;
        //    }
        //    else
        //    {
        //        Environment.ExitCode = -1;
        //        Application.Exit();
        //        return;
        //    }

        //    Show();
        #endregion       
        }

        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
            #region B���__�����3����ʾ
            pictureBox3.Image = m_processedImage.bitmap;
            pictureBox3.Invalidate();
            #endregion
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            #region Fly���__hide()
            //Hide();
            CameraSelectionDialog camSlnDlg = new CameraSelectionDialog();
            bool retVal = camSlnDlg.ShowModal();
            if (retVal)
            {
                try
                {
                    ManagedPGRGuid[] selectedGuids = camSlnDlg.GetSelectedCameraGuids();
                    ManagedPGRGuid guidToUse = selectedGuids[0];

                    ManagedBusManager busMgr = new ManagedBusManager();
                    InterfaceType ifType = busMgr.GetInterfaceTypeFromGuid(guidToUse);

                    if (ifType == InterfaceType.GigE)
                    {
                        m_camera = new ManagedGigECamera();
                    }
                    else
                    {
                        m_camera = new ManagedCamera();
                    }

                    // Connect to the first selected GUID
                    m_camera.Connect(guidToUse);

                    m_camCtlDlg.Connect(m_camera);    //

                    CameraInfo camInfo = m_camera.GetCameraInfo();
                    //UpdateFormCaption(camInfo);

                    // Set embedded timestamp to on
                    EmbeddedImageInfo embeddedInfo = m_camera.GetEmbeddedImageInfo();
                    embeddedInfo.timestamp.onOff = true;
                    m_camera.SetEmbeddedImageInfo(embeddedInfo);

                    m_camera.StartCapture();

                    m_grabImages = true;

                    StartGrabLoop();
              }
                catch (FC2Exception ex)
                {
                    Debug.WriteLine("Failed to load form successfully: " + ex.Message);
                    Environment.ExitCode = -1;
                    Application.Exit();
                    return;
                }

                toolStripButtonStart.Enabled = false;
                toolStripButtonStop.Enabled = true;
        }
            else
            {
                Environment.ExitCode = -1;
                Application.Exit();
                return;
            }

            //Show();
            #endregion
            #region ��������ĳ�ʼ��
            //���ر궨����
            StringBuilder str = new StringBuilder(100);
            GetPrivateProfileString("�궨", "fc1", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                fc1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "fc2", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                fc2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "cc1", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                cc1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "cc2", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                cc2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "R11", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R11 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "R12", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R12 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "R13", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R13 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "R21", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R21 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "R22", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R22 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "R23", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R23 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "T1", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                T1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "T2", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                T2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "T3", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                T3 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("�궨", "s", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                s = Convert.ToDouble(str.ToString());
            #endregion
        }
           
        //Form_load����
            // Search for any new cameras using Filter Driver
            //myFactory.UpdateCameraList(Jai_FactoryDotNET.CFactory.EDriverType.FilterDriver);
            //foreach (CCamera i in myFactory.CameraList)
            //{
            //    switch (i.UserName)
            //    {
            //        case "Cam1":
            //            myCamera1 = i;
            //            break;
            //        case "Cam2":
            //            myCamera2 = i;
            //            break;
            //        //case "Cam3":
            //        //    myCamera3 = i;
            //        //    break;
            //    }
            //    //if (myCamera1 != null)
            //    //{
            //    //    error = myCamera1.Open();
            //    //    if (error != Jai_FactoryWrapper.EFactoryError.Success)
            //    //    {
            //    //        MessageBox.Show(error.ToString(), "һ�����");
            //    //    }               
            //    //    myCamera1.StretchLiveVideo = true;
            //    //    myCamera1.SkipImageDisplayWhenBusy = true;
            //    //    myCamera1.GetNode("TriggerMode").Value = "Off";

            //    //}
            //    //if (myCamera2 != null)
            //    //{
            //    //    error = myCamera2.Open();
            //    //    if (error != Jai_FactoryWrapper.EFactoryError.Success)
            //    //    {
            //    //        MessageBox.Show(error.ToString(), "�������");
            //    //    }
            //    //    myCamera2.StretchLiveVideo = true;
            //    //    myCamera2.SkipImageDisplayWhenBusy = true;
            //    //    myCamera2.GetNode("TriggerMode").Value = "Off";

            //    //}
           
         /**************FLY-B���*******************/
        //Form1_FormClosing 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region FLY����ر�
            try
            {
                //�޸ġ�1.close()
                //toolStripButtonStop_Click_1(sender, e);
                StopButton_Click( sender,e);
                m_camera.Disconnect();
            }
            catch (FC2Exception)
            {
                // Nothing to do here
            }
            catch (NullReferenceException)
            {
                // Nothing to do here
            }
            #endregion
        }
        
       //FLY-StartGrabLoop
        private void StartGrabLoop()
        {
        #region FLY�����ʼ����
            m_grabThread = new BackgroundWorker();
            m_grabThread.ProgressChanged += new ProgressChangedEventHandler(UpdateUI);
            m_grabThread.DoWork += new DoWorkEventHandler(GrabLoop);
            m_grabThread.WorkerReportsProgress = true;
            m_grabThread.RunWorkerAsync();
        #endregion
        }
        
        //FLY---GrabLoop
        private void GrabLoop(object sender, DoWorkEventArgs e)
        {
        #region FLY�����������
            BackgroundWorker worker = sender as BackgroundWorker;
            while (m_grabImages)
            {
                try
                {
                    m_camera.RetrieveBuffer(m_rawImage);
                }
                catch (FC2Exception ex)
                {
                    Debug.WriteLine("Error: " + ex.Message);
                    continue;
                }

                lock (this)
                {
                    m_rawImage.Convert(FlyCapture2Managed.PixelFormat.PixelFormatBgr, m_processedImage);
                }
                
                worker.ReportProgress(0);    //��������  
            }
            m_grabThreadExited.Set();
        #endregion
        }
        
        //FLY-begin
        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
        #region FLY�������
            m_camera.StartCapture();

            m_grabImages = true;

            StartGrabLoop();

            toolStripButtonStart.Enabled = false;
            toolStripButtonStop.Enabled = true;
        #endregion
        }

        //FLY-B_stop
        private void toolStripButtonStop_Click_1(object sender, EventArgs e)
        {
            #region FLY����ر�
            m_grabImages = false;

            try
            {
                m_camera.StopCapture();
            }
            catch (FC2Exception ex)
            {
                Debug.WriteLine("Failed to stop camera: " + ex.Message);
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("Camera is null");
            }

            toolStripButtonStart.Enabled = true;
            toolStripButtonStop.Enabled = false;
            #endregion
        }
       
        //FLY--control
        private void toolStripButtonCameraControl_Click(object sender, EventArgs e)
        {
        #region Fly-������ؿ�������
            if (m_camCtlDlg.IsVisible())
            {
                m_camCtlDlg.Hide();
                toolStripButtonCameraControl.Checked = false;
            }
            else
            {
                m_camCtlDlg.Show();
                toolStripButtonCameraControl.Checked = true;
            }
        #endregion
        }
   
        //��������ĶԻ���   
        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            #region �궨�����
            calib cab = new calib();
            cab.Show();
            #endregion
        }

        /**********EgiE�������***********/
        #region searching
        private void SearchButton_Click(object sender, EventArgs e)
        {
            Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

            // enable Force IP
            myFactory.EnableAutoForceIP = true;
            // Search for any new cameras using Filter Driver
            myFactory.UpdateCameraList(Jai_FactoryDotNET.CFactory.EDriverType.FilterDriver);

            if (myFactory.CameraList.Count > 0)
            {
                for (int i = 0; i < myFactory.CameraList.Count; i++)
                {
                    string sList = myFactory.CameraList[i].ModelName;
                    camListBox.Items.Add(sList);

                    error = myFactory.CameraList[i].Open();
                }

                StartButton.Enabled = true;
                StopButton.Enabled = true;

                // Open the camera
                myCamera1 = myFactory.CameraList[0];
               // myCamera2 = myFactory.CameraList[1];
            }
            else
            {
                MessageBox.Show("No Cameras Found!");
            }
        }
        #endregion
        //EgiE�����ʼֹͣ    
        private void StartButton_Click(object sender, EventArgs e)
        {
            //#region �����ʼ��ֹͣ����
            if (myFactory.CameraList[0] != null)
            {
                myFactory.CameraList[0].StartImageAcquisition(true, 5, pictureBox1.Handle);
            }
            if (myFactory.CameraList[1] != null)
            {
                myFactory.CameraList[1].StartImageAcquisition(true, 5, pictureBox2.Handle);
            }

            StartButton.Enabled = false;
            StopButton.Enabled = true;
            SearchButton.Enabled = true;
           // #endregion
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            #region Stop_0.JAI���ֹͣ����
            for (int i = 0; i < myFactory.CameraList.Count; i++)
            {
                myFactory.CameraList[i].StopImageAcquisition();
            }

            StartButton.Enabled = true;
            StopButton.Enabled = false;
            SearchButton.Enabled = true;
            #endregion
 
        }        
        /*******����ͼ�������**********/
        //��һ�´���ͼ����ʾ
        //public void button_circle_Click_1(object sender, EventArgs e)
        //{
        //    #region ����ͼ����
        //    Console.WriteLine("1");
        //    myFactory.CameraList[0].SaveNextFrame(".\\saveimg" + ".bmp");
        //    ImageProcess();
        //    #endregion
        //}
        /********ֱ���߳̿���ͼ����*******/
        private void timer2_Tick_1(object sender, EventArgs e)
        {
            #region 5000�������һ��ͼ����
            Console.WriteLine("image processing!!!");
            myFactory.CameraList[0].SaveNextFrame(".\\saveimg" + ".bmp");
            ImageProcess();

            Console.WriteLine("===Flag_t====" + Flag_t);
            string str1 = this.tbxSendText.Text.Trim().ToString();

            //x = 34.3333;
            //point_X = (Int32)(circles[0].Center.X);
            //point_Y = (Int32)(circles[0].Center.Y);
            if (Flag_t == 0)
            {
                Int32 x = (Int32)(world_X);
                Int32 y = (Int32)(world_Y);
                Console.WriteLine("����modbus_x:" + x);
                Console.WriteLine("����modbus_y:" + y);
                Int32 m = (Int32)(0.011);

                byte[] a = BitConverter.GetBytes(x);
                a = LittleEncodingFloat(a);
                byte[] b = BitConverter.GetBytes(y);
                b = LittleEncodingFloat(b);
                byte[] c = BitConverter.GetBytes(m);
                c = LittleEncodingFloat(c);
                byte[] z = new byte[a.Length + b.Length + c.Length];
                a.CopyTo(z, 0);
                b.CopyTo(z, a.Length);
                c.CopyTo(z, a.Length + b.Length);

                // System.Console.WriteLine( x);
                //System.Console.WriteLine( y);
                this.Wrapper.Send(z);

            }
            else if (Flag_t == 1)
            {
                Int32 x = (Int32)(world_X_circle);
                Int32 y = (Int32)(world_Y_circle);
                Console.WriteLine("����modbus_x:" + x);
                Console.WriteLine("����modbus_y:" + y);
                Int32 m = (Int32)(0.011);

                byte[] a = BitConverter.GetBytes(x);
                a = LittleEncodingFloat(a);
                byte[] b = BitConverter.GetBytes(y);
                b = LittleEncodingFloat(b);
                byte[] c = BitConverter.GetBytes(m);
                c = LittleEncodingFloat(c);
                byte[] z = new byte[a.Length + b.Length + c.Length];
                a.CopyTo(z, 0);
                b.CopyTo(z, a.Length);
                c.CopyTo(z, a.Length + b.Length);

                // System.Console.WriteLine( x);
                //System.Console.WriteLine( y);
                this.Wrapper.Send(z);
            }
            else
            {
                Flag_t = 1;
            }

            #endregion
        }
        
        //ͼ�������ģ��
        #region ͼ����ģ��
        private void ImageProcess()
        {
            //*canny*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(".\\saveimg" + ".bmp");
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();
            double cannyThreshold =200.0;
            double circleAccumulatorThreshold = 55;
            #region Find circles
            /*���Բ��*/
            circles = grayImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                2.0, //Resolution of the accumulator used to detect centers of the circles
                grayImage.Width, //min distance 
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            //CircleF[] circles = grayImage.HoughCircles(new Gray(250), new Gray(74.471), 1.0, grayImage.Width,0, 0)[0];//�ڶ�������
            /*��ԭͼ�ϻ�Բ*/
            // Image<Bgr, Byte> imageLines = new Image<Bgr, Byte>(".\\saveimg" + ".bmp"); 
            // foreach (CircleF circle in circles)
            // {
            //    imageLines.Draw(circle, new Bgr(Color.Red), 2);
            /*���Բ��Բ��*/
            //Console.WriteLine(circle.Center);
            // }
            #endregion
            #region Canny and edge detection

            double cannyThresholdLinking = 100.0;
            Image<Gray, Byte> cannyEdges = grayImage.Canny(cannyThreshold, cannyThresholdLinking);
            LineSegment2D[] lines = cannyEdges.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 90.0, //Angle resolution measured in radians.
                20, //threshold
                30, //min Line width
                10 //gap between lines
                )[0]; //Get the lines from the first channel
            #endregion
            #region Find triangles and rectangles

            List<Triangle2DF> triangleList = new List<Triangle2DF>();
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle

            List<MCvBox2D> pentagon = new List<MCvBox2D>();
            // PointF[] GetVertices();

            using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                for (
                   Contour<Point> contours = cannyEdges.FindContours(
                      Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                      Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                      storage);
                   contours != null;
                   contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.03, storage);//ע�������The desired approximation accuracyΪ0.04

                    if (currentContour.Area >400) //only consider contours with area greater than 4300
                    {
                        if (currentContour.Total == 3) //The contour has 3 vertices, it is a triangle
                        {
                            #region   triangle_detected
                            Point[] pts = currentContour.ToArray();
                            triangleList.Add(new Triangle2DF(
                               pts[0],
                               pts[1],
                               pts[2]
                               ));
                            #endregion
                        }

                        else if (currentContour.Total == 4) //The contour has 4 vertices.
                        {
                            #region determine if all the angles in the contour are within [80, 100] degree
                            bool isRectangle = true;
                            Point[] pts = currentContour.ToArray();
                            LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                            for (int i = 0; i < edges.Length; i++)
                            {
                                double angle = Math.Abs(
                                   edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                if (angle < 80 || angle > 100)
                                {
                                    isRectangle = false;
                                    break;
                                }
                            }
                            #endregion
                            if (isRectangle) boxList.Add(currentContour.GetMinAreaRect());
                        }

                        else if (currentContour.Total == 5) //The contour has 5 vertices.
                        {
                            #region determine if all the angles in the contour are within [65, 80] degree  5555
                            bool isPentagon = true;
                            Point[] pts2 = currentContour.ToArray();
                            // PointF[] p = p;
                            LineSegment2D[] edges = PointCollection.PolyLine(pts2, true);

                            for (int i = 0; i < edges.Length; i++)
                            {
                                double angle = Math.Abs(
                                   edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                if (angle < 65 || angle > 80)
                                {
                                    isPentagon = false;
                                    break;
                                }
                            }
                            #endregion
                            if (isPentagon) pentagon.Add(currentContour.GetMinAreaRect());

                        }

                    }
                }
            #endregion
            #region draw triangles and rectangles and pentagon
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(".\\saveimg" + ".bmp");
            foreach (Triangle2DF triangle in triangleList)
            {
                triangleRectangleImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
                Console.WriteLine("Center Of Triangle :" + triangle.Centeroid);
            }
            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.DarkOrange), 2);
                //Console.WriteLine("Center Of rectangle1 :" + boxList[0].center);
               // Console.WriteLine("Center Of rectangle2 :"+ boxList);
                //ͼ����x,y������λ��
                point_X = (Int32)(boxList[0].center.X);
                point_Y = (Int32)(boxList[0].center.Y);
                //Flag = 0;
                //AreaRect = (Int32)(boxList[0].center.X * boxList[0].center.Y);
                //Console.WriteLine("Area Of rectangle :" + (box1.center.X*box1.center.Y));
            }

            foreach (MCvBox2D pen in pentagon)
            {
                triangleRectangleImage.Draw(pen, new Bgr(Color.DarkOrange), 2);
                //Console.WriteLine("Center Of rectangle_penta 1:" + pen.center);
               // Console.WriteLine("Center Of rectangle_penta 2:" + pentagon[0].MinAreaRect());
            }

            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circles[0].Area);
                if (AreaCircle >= 4000 && AreaCircle <= 4600)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
                    /*���Բ��Բ��*/
                    //Console.WriteLine("Center Of Circle:" + circle.Center);
                    point_X_circle = (Int32)(circles[0].Center.X);
                    point_Y_circle = (Int32)(circles[0].Center.Y);
                }

                //Flag = 1;

                //Console.WriteLine("circle_area:" + AreaCircle);
              
                //Console.WriteLine("circle-x:" + circles[0].Center.X);
                //Console.WriteLine("circle-y:" + circles[0].Center.Y); //circle[0]����circle[1]��ָ�ҵ���Բ�еĵ�һ���͵ڶ���

            }
            #endregion
            //��ʾ���
            pictureBox_circle.Image = triangleRectangleImage.ToBitmap();
            //point_X = (Int32)(box1.center.X);   //Բ��x�����Բ��y���� SimpleImageDisplaySample.Form1.box1����ͻ	C:\Users\Administrator\Desktop\zsx__PC\20180108_PC_xiamen\5ADD_calib(20180108)\ADD_calib(20180108)_changing\ADD_Polygon��20171029��\imshow_add_modbus��20171009��\SimpleImageDisplaySample\Form1.cs	716	31	SimpleImageDisplaySample
            //point_Y = (Int32)(box1.center.Y);
            //point_Y = (Int32)(circles[0].Center.Y);
            //Console.WriteLine("SimpleImageDisplaySample.Form1.box1.center.X:" + box1.center.X);
            double[,] a = new double[3, 3] { { fc1, 0, cc1 }, { 0, fc2, cc2 }, { 0, 0, 1 } };
            double[,] b = new double[3, 3] { { R11, R21, T1 }, { R12, R22, T2 }, { R13, R23, T3 } };
             
            //Area
            if (Flag == 0)
            { 
                //rectangle_location
                double[,] image_pix = new double[3, 1] { { point_X }, { point_Y }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);
                //Matrix.MatrixMultiply(c_, image_pix, ref world_cor);
                world_X = (world_cor[0, 0] / s) * 1000;
                world_Y = (world_cor[1, 0] / s) * 1000;
                //point_X = (Int32)(circles[0].Center.X);   //Բ��x�����Բ��y����
                //point_Y = (Int32)(circles[0].Center.Y);
                //int number_x = (int)(world_X);
                //int number_y = (int)(world_Y);
                //Console.WriteLine("point_X:" + point_X);
                //Console.WriteLine("point_Y:" + point_Y);
                //Console.WriteLine("world_initial_X:" + world_X);
                //Console.WriteLine("world_initial_Y:" + world_Y);
                Flag_t = Flag_t - 1;
                Flag = 1;
                //Console.WriteLine("world__X:" + number_x);
            }
            else if (Flag == 1)
            {
                //circle_location
                double[,] image_pix = new double[3, 1] { { point_X_circle }, { point_Y_circle }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);
                //Matrix.MatrixMultiply(c_, image_pix, ref world_cor);
                world_X_circle = (world_cor[0, 0] / s) * 1000;
                world_Y_circle = (world_cor[1, 0] / s) * 1000;
                //point_X = (Int32)(circles[0].Center.X);   //Բ��x�����Բ��y����
                //point_Y = (Int32)(circles[0].Center.Y);
                //int number_x = (int)(world_X);
                //int number_y = (int)(world_Y);
                //Console.WriteLine("point_X:" + point_X_circle);
                //Console.WriteLine("point_Y:" + point_Y_circle);
               // Console.WriteLine("world_initial_X:" + world_X);
                //Console.WriteLine("world_initial_Y:" + world_Y);
                Flag_t = Flag_t + 1;
                Flag = 0;
                //Console.WriteLine("world__X:" + number_x);
            }
            else
            {
                Flag = 0;
            }
            //Console.WriteLine("world__Y:" + number_y);
        }
        #endregion



        /**********Modbus����***********/
        #region Modbus �Ĵ��͵����ݸ�ʽ����
        //private void btnSend_Click_1(object sender, EventArgs e)
        //{
           
        
        //    string str1 = this.tbxSendText.Text.Trim().ToString();

        //    //x = 34.3333;
        //    //point_X = (Int32)(circles[0].Center.X);
        //    //point_Y = (Int32)(circles[0].Center.Y);

        //    Int32 x = (Int32)(world_X);

        //    Int32 y = (Int32)(world_Y);
        //    Console.WriteLine("����modbus_x:" + x);
        //    Console.WriteLine("����modbus_y:" + y);
        //   // Int32 y = (Int32)circles[0].Center.Y * 1000;
        //   // System.Console.WriteLine(y);

        //    //Int32 y = (Int32)circles[0].Center.Y;
        //    //System.Console.WriteLine(y);


        //    Int32 m = (Int32)( - 19.487);
            
        //    byte[] a = BitConverter.GetBytes(x);
        //    a = LittleEncodingFloat(a);
        //    byte[] b = BitConverter.GetBytes(y);
        //    b = LittleEncodingFloat(b);
        //    byte[] c = BitConverter.GetBytes(m);
        //    c = LittleEncodingFloat(c);
        //    byte[] z = new byte[a.Length + b.Length + c.Length];
        //    a.CopyTo(z, 0);
        //    b.CopyTo(z, a.Length);
        //    c.CopyTo(z, a.Length+b.Length);

        //   // System.Console.WriteLine( x);
        //    //System.Console.WriteLine( y);
        //    this.Wrapper.Send(z);
        //    //this.Wrapper.Send(BitConverter.GetBytes(y));
            
        //}
        #endregion
        #region С�˷�װ
        public static void ReverseBytes(byte[] bytes, int start, int len)
        {
            int end = start + len - 1;
            byte tmp;
            int i = 0;
            for (int index = start; index < start + len / 2; index++, i++)
            {
                tmp = bytes[end - i];
                bytes[end - i] = bytes[index];
                bytes[index] = tmp;
            }
        }
        public static byte[] LittleEncodingFloat(byte[] bytes)
        {
            ReverseBytes(bytes, 0, 2);
            ReverseBytes(bytes, 2, 2);
            // ReverseBytes(bytes, 4, 2);
            return bytes;
        }
        #endregion
        #region ILog ��Ա
        public void Write(string log)
        {
            this.tbxHistory.AppendText(log + Environment.NewLine);
            this.tbxHistory.Select(this.tbxHistory.TextLength, 0);
            this.tbxHistory.ScrollToCaret();
        }
        #endregion
        #region �ͷ�Modbus��Դ
        private void TestModBus_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Wrapper.Dispose();
        }
        #endregion
        /******����Ŵ���С*********/
        #region zoom�Ŵ���С
        private void ZoomInbutton_Click(object sender, EventArgs e)
        {
            //Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;
            if (myFactory.CameraList[0] != null)
                myFactory.CameraList[0].ZoomIn();
               

        }

        private void ZoomResetbutton_Click(object sender, EventArgs e)
        {
            if (myFactory.CameraList[0] != null)
                myFactory.CameraList[0].ZoomReset();
        }

        private void ZoomOutbutton_Click(object sender, EventArgs e)
        {
            if (myFactory.CameraList[0] != null)
                myFactory.CameraList[0].ZoomOut();
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    #region 500�������һ�Σ�������ֵͨ��Modbus��ֵ������е��
        //    timer1.Enabled = true;
        //    string str1 = this.tbxSendText.Text.Trim().ToString();
            
        //    //x = 34.3333;
        //    //point_X = (Int32)(circles[0].Center.X);
        //    //point_Y = (Int32)(circles[0].Center.Y);
        //    if (Flag_t == 2)
        //    {
        //        Int32 x = (Int32)(world_X);
        //        Int32 y = (Int32)(world_Y);
        //        Console.WriteLine("����modbus_x:" + x);
        //        Console.WriteLine("����modbus_y:" + y);
        //        Int32 m = (Int32)(0.011);

        //        byte[] a = BitConverter.GetBytes(x);
        //        a = LittleEncodingFloat(a);
        //        byte[] b = BitConverter.GetBytes(y);
        //        b = LittleEncodingFloat(b);
        //        byte[] c = BitConverter.GetBytes(m);
        //        c = LittleEncodingFloat(c);
        //        byte[] z = new byte[a.Length + b.Length + c.Length];
        //        a.CopyTo(z, 0);
        //        b.CopyTo(z, a.Length);
        //        c.CopyTo(z, a.Length + b.Length);

        //        // System.Console.WriteLine( x);
        //        //System.Console.WriteLine( y);
        //        this.Wrapper.Send(z);
            
        //    }
        //    else if (Flag_t == 3)
        //    {
        //        Int32 x = (Int32)(world_X_circle);
        //        Int32 y = (Int32)(world_Y_circle);
        //        Console.WriteLine("����modbus_x:" + x);
        //        Console.WriteLine("����modbus_y:" + y);
        //        Int32 m = (Int32)(0.011);

        //        byte[] a = BitConverter.GetBytes(x);
        //        a = LittleEncodingFloat(a);
        //        byte[] b = BitConverter.GetBytes(y);
        //        b = LittleEncodingFloat(b);
        //        byte[] c = BitConverter.GetBytes(m);
        //        c = LittleEncodingFloat(c);
        //        byte[] z = new byte[a.Length + b.Length + c.Length];
        //        a.CopyTo(z, 0);
        //        b.CopyTo(z, a.Length);
        //        c.CopyTo(z, a.Length + b.Length);

        //        // System.Console.WriteLine( x);
        //        //System.Console.WriteLine( y);
        //        this.Wrapper.Send(z);
        //    }
        //    else {

        //        Flag = 0;
        //    }
          
          
        //    // Int32 y = (Int32)circles[0].Center.Y * 1000;
        //    // System.Console.WriteLine(y);

        //    //Int32 y = (Int32)circles[0].Center.Y;
        //    //System.Console.WriteLine(y);

           
        //    #endregion
        //}       
    }  
}
     


/****����ע��*****/
#region ����ͼ������ת������
/* ͼ������ת��������
         * Image img = Properties.Resources.Form3_PIC_00;  //ֻ����system.drawing.image�ܶ��룬Mat��emgu��image������
            Bitmap bmpImage = new Bitmap(img); //���ǹؼ���������վ������
            Emgu.CV.Image<Bgr, Byte> currentFrame = new Emgu.CV.Image<Bgr, Byte>(bmpImage);  //ֻ����ôת
       
            Mat invert = new Mat();
             CvInvoke.BitwiseAnd(currentFrame, currentFrame, invert);  //���ǹ����ϵķ�������ͨ�á�û�����ṩ��������ֱ��ת���ġ�
         */
#endregion
/****����ע��*****/