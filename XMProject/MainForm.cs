using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

//导入 串口通讯控制机械爪
using System.IO.Ports;
using System.IO;
using System.Threading;

//导入 Emgu.CV
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;

//导入 System.Threading;
using System.Runtime.InteropServices; // 后面的[DllImport("kernel32")]
using System.Drawing.Imaging;
using System.Diagnostics;

//导入 自己的Matrix矩阵模块
using matrix_test;

//导入 JAI
using Jai_FactoryDotNET;
//导入 flycapture
using FlyCapture2Managed;
using FlyCapture2Managed.Gui;

//分割字符串
using System.Text.RegularExpressions;


namespace SimpleImageDisplaySample
{
    public partial class MainForm : Form, ILog, IDisposable
    {

        #region 声明全局变量
        #region /***Modbus 的返回值***/

        //static 定义的变量，可以直接从类这边点出来
        public static int g_ModbusFlagResult = 0;

        #endregion

        #region /***RS232初始化变量***/
        /*****  机械爪的初始化  ******/
        //读取夹爪的状态:可得到当前开口的大小，夹持力当前值和阈值；
        byte[] g_SReadStatus = new byte[] { 0xEB, 0x90, 0x01, 0x01, 0x14, 0x16 };
        //对应的应答{0xEE,0x16,0x01,0x07,0x14,0xA0,0x0F,0x02,0x08,0x34,0x08,0x11}
        //设置夹爪的抓取数据
        byte[] g_GrabArray = new byte[] { 0xEB, 0x90, 0x01, 0x04, 0x10, 0x00, 0x00, 0x00, 0x00 };
        //0xEB,0x90,0x01,xxx,0x10,0x32(速度50),0x64(力控阈值100g),0x00,0xAB




        #endregion

        //C相机的BMP文件可以看到全图
        #region /***三个相机捕捉到的图像路径***/
        string ImagePath_A = ".\\saveimgA.jpg";
        string ImagePath_B = ".\\saveimgB.jpg";
        string ImagePath_C = ".\\saveimgC.bmp";


        #endregion

        #region /***区分长方形、正方形和圆形的世界坐标点在A相机和C相机***/
        double world_X = 0;
        double world_Y = 0;
        double world_X_c = 0;
        double world_Y_c = 0;

        double world_X_circle = 0;
        double world_Y_circle = 0;
        double world_X_circle_c = 0;
        double world_Y_circle_c = 0;

        double point_X_C_chang;
        double point_Y_C_chang;

        double point_X_C_zheng;
        double point_Y_C_zheng;


        //将选择好需要传给Modbus的值g_AWorldX&&g_AWorldY
        double g_AWRectX = 0;
        double g_AWRectY = 0;

        double g_AWCircleX = 0;
        double g_AWCircleY = 0;


        double public_X_C = 0;
        double public_Y_C = 0;
        #endregion

        #region /***设置区分长方形，正方形和圆形的标志位***/
        //TODO:Flag_B标志位表示B相机是否检测到了物体的形状，
        //为1则表示检测到了长方形，
        //2则表示检测到了正方形，
        //3则表示检测到了圆形
        int Flag_B = 1;

        int AreaCircle;

        #endregion

        #region /***ini文件变量声明***/
        //系统文件用来读取ini文件
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion

        #region /***A&C相机定标变量声明***/
        //A相机--图像坐标与世界坐标初始化
        public static double fc1, fc2, cc1, cc2, R11, R12, R13, R21, R22, R23, T1, T2, T3, s;

        //B相机--图像坐标与世界坐标初始化
        public static double fc1_c, fc2_c, cc1_c, cc2_c, R11_c, R12_c, R13_c, R21_c, R22_c, R23_c, T1_c, T2_c, T3_c, s_c;

        double[,] c = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] c_ = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] world_cor = new double[3, 1] { { 0 }, { 0 }, { 1 } };

        #endregion

        #region /***Fly相机变量声明***/
        //相机的初始化程序
        private FlyCapture2Managed.Gui.CameraControlDialog m_camCtlDlg;
        private ManagedCameraBase m_camera = null;
        private ManagedImage m_rawImage;
        private ManagedImage m_raw_B_Image;
        private ManagedImage m_processedImage;
        private ManagedImage m_processed_B_Image;
        private bool m_grabImages;
        private AutoResetEvent m_grabThreadExited;
        private BackgroundWorker m_grabThread;
        #endregion

        #region /***JAI相机和图像处理变量声明***/
        //JAI相机
        CFactory myFactory = new CFactory();
        //图像处理--圆
        public CircleF circle;
        public CircleF[] circles;
        //图像处理--矩形
        public MCvBox2D box1;
        public List<MCvBox2D> boxList;
        // Opened camera obejct
        //CCamera myCamera1;
        //CCamera myCamera2;
        //Jai_FactoryWrapper.EFactoryError error;
        #endregion

        #region /***Modbus实例化***/
        private ModBusWrapper Wrapper = null;
        #endregion
        #endregion

        public MainForm()
        {
            InitializeComponent(); //窗口的初始化：比如拖一些框

            #region /***初始化主窗口:三个相机和Modbus初始化***/
            /*************Fly__init***************/
            #region /*Fly-C相机初始化*/
            m_rawImage = new ManagedImage();
            m_raw_B_Image = new ManagedImage();
            m_processedImage = new ManagedImage();
            m_processed_B_Image = new ManagedImage();
            m_camCtlDlg = new CameraControlDialog();
            m_grabThreadExited = new AutoResetEvent(false);  //非终止
            #endregion

            /*************JAI__init***************/
            #region /*JAI-A&B相机初始化*/
            Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

            // Open the factory with the default Registry database
            error = myFactory.Open("");
            // Search for cameras and update all controls
            SearchButton_Click(null, null);
            #endregion

            /***********Modbus--TCP**************/
            #region /*Modbus初始化*/
            this.Wrapper = ModBusWrapper.CreateInstance(Protocol.TCPIP);
            this.Wrapper.Logger = this;
            #endregion
            #endregion
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer2.Enabled = false;

            #region /***Fly-C相机__hide()***/
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

                    m_camCtlDlg.Connect(m_camera);

                    CameraInfo camInfo = m_camera.GetCameraInfo();
                    //UpdateFormCaption(camInfo);

                    // Set embedded timestamp to on
                    EmbeddedImageInfo embeddedInfo = m_camera.GetEmbeddedImageInfo();
                    embeddedInfo.timestamp.onOff = true;
                    m_camera.SetEmbeddedImageInfo(embeddedInfo);

                }
                catch (FC2Exception ex)
                {
                    Debug.WriteLine("Failed to load form successfully: " + ex.Message);
                    Environment.ExitCode = -1;
                    Application.Exit();
                    return;
                }

            }
            else
            {
                Environment.ExitCode = -1;
                Application.Exit();
                return;
            }
            Show();
            #endregion
            #region /***A相机_定标参数的初始化***/
            //加载标定参数
            StringBuilder str = new StringBuilder(100);
            //calibA_A.ini:表示的是A相机定标之后所保存的定标参数
            GetPrivateProfileString("标定", "fc1", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                fc1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "fc2", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                fc2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "cc1", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                cc1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "cc2", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                cc2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R11", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                R11 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R12", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                R12 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R13", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                R13 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R21", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                R21 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R22", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                R22 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R23", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                R23 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "T1", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                T1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "T2", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                T2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "T3", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                T3 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "s", "", str, 100, Application.StartupPath + "/calibA_A.ini");
            if (str.ToString() != "")
                s = Convert.ToDouble(str.ToString());
            #endregion
            #region /***C相机_定标参数的初始化***/
            //加载标定参数
            StringBuilder str_c = new StringBuilder(100);
            GetPrivateProfileString("标定", "fc1", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                fc1_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "fc2", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                fc2_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "cc1", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                cc1_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "cc2", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                cc2_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R11", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                R11_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R12", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                R12_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R13", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                R13_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R21", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                R21_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R22", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                R22_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R23", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                R23_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "T1", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                T1_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "T2", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                T2_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "T3", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                T3_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "s", "", str_c, 100, Application.StartupPath + "/calib_C_C.ini");
            if (str_c.ToString() != "")
                s_c = Convert.ToDouble(str_c.ToString());
            #endregion
            #region /***串口通讯的初始化，变成灰色***/
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            #endregion
        }

        #region /***相机A和相机C的标定程序***/
        private void btnCalib_A_Click(object sender, EventArgs e)
        {
            //TODO:定时器里面写Modbus
            //   timer2.Enabled = true;
            //标定框出来
            Calib_A cab = new Calib_A();
            cab.Show();
        }

        private void btnCalib_C_Click(object sender, EventArgs e)
        {
            Calib_C cab_C = new Calib_C();
            cab_C.Show();
        }
        #endregion


        #region /***FlyCapture-C相机刷新图像***/
        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
            pictureBox_C.Image = m_processedImage.bitmap;

            pictureBox_C.Invalidate();
        }

        private void StartGrabLoop()
        {
            m_grabThread = new BackgroundWorker();
            m_grabThread.ProgressChanged += new ProgressChangedEventHandler(UpdateUI);
            m_grabThread.DoWork += new DoWorkEventHandler(GrabLoop);
            m_grabThread.WorkerReportsProgress = true;
            m_grabThread.RunWorkerAsync();
        }
        private void GrabLoop(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (m_grabImages)
            {
                try
                {
                    // Retrieve an image
                    m_camera.RetrieveBuffer(m_rawImage);

                    //保存相机C的图像信息
                    m_rawImage.Save(ImagePath_C);

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
                worker.ReportProgress(0);    //很重要：不然显示不出来相机C-进度流程  
            }
            m_grabThreadExited.Set();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                StopButton_Click(sender, e);
                m_camera.Disconnect();
            }
            catch (FC2Exception)
            {
                // Nothing to do here
                Console.WriteLine("主窗口的关闭界面出现问题！");
            }
            catch (NullReferenceException)
            {
                // Nothing to do here
                Console.WriteLine("主窗口出现其他错误！");
            }
        }
        #endregion


        /*****************Main*****************/
        /************AI-A&&B相机**************/
        #region JAI-A&&B相机,开始，结束和搜索
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
            }
            else
            {
                MessageBox.Show("No Cameras Found!");
            }
        }

        //将三个相机的开始程序集合在一起
        private void StartButton_Click(object sender, EventArgs e)
        {
            /* ***
             * Fly_C相机的开始程序
             * ***/
            #region Fly_C
            m_camera.StartCapture();
            m_grabImages = true;
            StartGrabLoop();
            #endregion

            /* ***
             * JAIA&B相机的开始程序
             * ***/
            #region JAI A&B
            if (myFactory.CameraList[0] != null)
            {
                myFactory.CameraList[0].StartImageAcquisition(true, 5, pictureBox_A.Handle);
                myFactory.CameraList[0].SaveNextFrame(ImagePath_A);

            }
            if (myFactory.CameraList[1] != null)
            {

                myFactory.CameraList[1].StartImageAcquisition(true, 5, pictureBox_B.Handle);
                myFactory.CameraList[1].SaveNextFrame(ImagePath_B);
            }

            StartButton.Enabled = false;
            StopButton.Enabled = true;
            SearchButton.Enabled = true;
            #endregion


            // myFactory.CameraList[0].SaveNextFrame(ImagePath_A);

            //myFactory.CameraList[1].SaveNextFrame(ImagePath_B);

            //ImageProcess_A(ImagePath_A);
            //SendDataToModBus(g_AWorldX, g_AWorldX);

            // ImageProcess_B(ImagePath_B);
            //ImageProcess_C(ImagePath_C);

        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            /* ***
             * Fly_C相机的关闭程序
             * ***/
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

            /* ***
             * JAI-A&B 相机的关闭程序
             * ***/
            for (int i = 0; i < myFactory.CameraList.Count; i++)
            {
                myFactory.CameraList[i].StopImageAcquisition();
            }

            StartButton.Enabled = true;
            StopButton.Enabled = false;
            SearchButton.Enabled = true;
        }
        #endregion

        /* ***
         * 关于图像处理程序
         * ***/
        #region 相机A的处理过程,返回14表示A相机传值给modbus成功
        private int ImageProcess_A(string ImagePath)
        {
            //矩形和圆形的中心点坐标
            double ARectangleX = 0;
            double ARectangleY = 0;

            double ACircleX = 0;
            double ACircleY = 0;

            /*canny算子处理图像*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(ImagePath);
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();

            Image<Gray, Byte> MedianImage = grayImage.SmoothMedian(13);

            double cannyThreshold = 200.0;
            double circleAccumulatorThreshold = 50.0;

            #region Find circles
            /*检测圆形*/
            circles = MedianImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                1.5, //Resolution of the accumulator used to detect centers of the circles
                MedianImage.Width, //min distance 
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion

            #region Canny and edge detection
            double cannyThresholdLinking = 50.0;
            Image<Gray, Byte> cannyEdges = MedianImage.Canny(cannyThreshold, cannyThresholdLinking);

            #endregion

            #region search rectangles
            //存放矩形的形状
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
            // PointF[] GetVertices();
            using (MemStorage storage1 = new MemStorage()) //allocate storage for contour approximation
                for (
                   Contour<Point> contours = cannyEdges.FindContours(
                      Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                      Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                      storage1);
                   contours != null;
                   contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.03, storage1);//注意这里的The desired approximation accuracy为0.04

                    if (currentContour.Area > 1000) //only consider contours with area greater than 4300
                    {
                        if (currentContour.Total == 4)  //The contour has 4 vertices.
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
                    }
                }
            #endregion

            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            //draw the rectangles           
            Console.WriteLine("A相机中的矩形的个数：" + boxList.Count());

            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.DarkOrange), 2);
            }

            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circle.Area);
                if (AreaCircle <= 8000 && AreaCircle >= 3000)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
                }
                else { continue; }
            }
            #endregion

            /*显示结果，在A相机的图像中显示出来*/
            pictureBox_A_processed.Image = triangleRectangleImage.ToBitmap();
            //pictureBox_A_processed.Image = cannyEdges.ToBitmap();

            double[,] a = new double[3, 3] { { fc1, 0, cc1 }, { 0, fc2, cc2 }, { 0, 0, 1 } };
            double[,] b = new double[3, 3] { { R11, R21, T1 }, { R12, R22, T2 }, { R13, R23, T3 } };


            if (boxList.Count() == 0)
            {
                Console.WriteLine("Rectangle is no found!");
            }
            else
            {
                //图像中x,y的坐标位置
                ARectangleX = (double)(boxList[0].center.X);
                ARectangleY = (double)(boxList[0].center.Y);
                Console.WriteLine("矩形的重心：" + boxList[0].center);
                Console.WriteLine("ARectangleX:" + boxList[0].center.X);
                Console.WriteLine("ARectangleY:" + boxList[0].center.Y);


                //rectangle_location
                double[,] image_pix = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X = (world_cor[0, 0] / s) * 1000;
                world_Y = (world_cor[1, 0] / s) * 1000;

                g_AWRectX = world_X;
                g_AWRectY = world_Y;
                Console.WriteLine("g_AWRectX" + g_AWRectX);
                Console.WriteLine("g_AWRectY" + g_AWRectY);
            }

            /*输出圆的圆心*/
            //Console.WriteLine("圆形的个数：" + circles.Count());
            if (circles.Count() == 0 || (circles[0].Area >= 4600 || circles[0].Area <= 3000))
            {
                Console.WriteLine("Circle is no found!");

            }
            else
            {
                ACircleX = (double)(circles[0].Center.X);
                ACircleY = (double)(circles[0].Center.Y);
                Console.WriteLine("圆形的重心：" + circles[0].Center);
                //circle_location
                double[,] image_pix = new double[3, 1] { { ACircleX }, { ACircleY }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X_circle = (world_cor[0, 0] / s) * 1000;
                world_Y_circle = (world_cor[1, 0] / s) * 1000;

                g_AWCircleX = world_X_circle;
                g_AWCircleY = world_Y_circle;

                Console.WriteLine("g_AWCircleX:" + g_AWCircleX);
                Console.WriteLine("g_AWCircleY:" + g_AWCircleY);
            }

            return 14;
        }
        #endregion


        #region 相机B的处理过程
        private void ImageProcess_B(string ImagePath)
        {
            /*canny算子处理图像*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(ImagePath);
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();

            Image<Gray, Byte> MedianImage = grayImage.SmoothMedian(13);

            double cannyThreshold = 10.0;
            double circleAccumulatorThreshold = 20.0;

            #region Find circles
            /*检测圆形*/
            circles = MedianImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                1.5, //Resolution of the accumulator used to detect centers of the circles
                MedianImage.Width, //min distance 
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion

            #region Canny and edge detection
            double cannyThresholdLinking = 100.0;
            Image<Gray, Byte> cannyEdges = MedianImage.Canny(cannyThreshold, cannyThresholdLinking);

            #endregion

            #region Find rectangles
            //存放矩形的形状
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
            // PointF[] GetVertices();
            using (MemStorage storage1 = new MemStorage()) //allocate storage for contour approximation
                for (
                   Contour<Point> contours = cannyEdges.FindContours(
                      Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                      Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                      storage1);
                   contours != null;
                   contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.03, storage1);//注意这里的The desired approximation accuracy为0.04
                    //Console.WriteLine("currentContour.Area:" + currentContour.Area);
                    if (currentContour.Area > 400) //only consider contours with area greater than 4300
                    {
                        if (currentContour.Total == 4)  //The contour has 4 vertices.
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
                    }
                }
            #endregion

            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            //draw the rectangles           
            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.DarkOrange), 2);
                Console.WriteLine("B相机中的矩形的面积：" + boxList[0].size.Height);
            }

            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circle.Area);
                if (AreaCircle >= 3000)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 3);
                }
                else { continue; }
            }
            #endregion

            /*显示结果，在B相机的图像中显示出来*/
            //pictureBox_A_processed.Image = triangleRectangleImage.ToBitmap();
            pictureBox_B_Processing.Image = triangleRectangleImage.ToBitmap();

        }

        #endregion


        #region 相机C的处理过程,返回15表示C相机传值给modbus成功
        private int ImageProcess_C(string ImagePath)
        {
            //矩形和圆形的中心点坐标
            double ARectangleX = 0;
            double ARectangleY = 0;

            double ACircleX = 0;
            double ACircleY = 0;

            /*canny算子处理图像*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(ImagePath);
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();

            Image<Gray, Byte> MedianImage = grayImage.SmoothMedian(13);

            double cannyThreshold = 200.0;
            double circleAccumulatorThreshold = 50.0;

            #region Find circles
            /*检测圆形*/
            circles = MedianImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                1.5, //Resolution of the accumulator used to detect centers of the circles
                MedianImage.Width, //min distance 
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion

            #region Canny and edge detection
            double cannyThresholdLinking = 50.0;
            Image<Gray, Byte> cannyEdges = MedianImage.Canny(cannyThreshold, cannyThresholdLinking);

            #endregion

            #region search rectangles
            //存放矩形的形状
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
            // PointF[] GetVertices();
            using (MemStorage storage1 = new MemStorage()) //allocate storage for contour approximation
                for (
                   Contour<Point> contours = cannyEdges.FindContours(
                      Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                      Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                      storage1);
                   contours != null;
                   contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.03, storage1);//注意这里的The desired approximation accuracy为0.04

                    if (currentContour.Area > 1000) //only consider contours with area greater than 4300
                    {
                        if (currentContour.Total == 4)  //The contour has 4 vertices.
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
                    }
                }
            #endregion

            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            //draw the rectangles           
            Console.WriteLine("A相机中的矩形的个数：" + boxList.Count());

            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.DarkOrange), 2);
            }

            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circle.Area);
                if (AreaCircle <= 8000 && AreaCircle >= 3000)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
                }
                else { continue; }
            }
            #endregion

            /*显示结果，在A相机的图像中显示出来*/
            pictureBox_C_processed.Image = triangleRectangleImage.ToBitmap();
            //pictureBox_A_processed.Image = cannyEdges.ToBitmap();

            double[,] a = new double[3, 3] { { fc1_c, 0, cc1_c }, { 0, fc2_c, cc2_c }, { 0, 0, 1 } };
            double[,] b = new double[3, 3] { { R11_c, R21_c, T1_c }, { R12_c, R22_c, T2_c }, { R13_c, R23_c, T3_c } };


            if (boxList.Count() == 0)
            {
                Console.WriteLine("Rectangle is no found!");
            }
            else
            {
                //图像中x,y的坐标位置
                ARectangleX = (double)(boxList[0].center.X);
                ARectangleY = (double)(boxList[0].center.Y);
                Console.WriteLine("矩形的重心：" + boxList[0].center);
                Console.WriteLine("ARectangleX:" + boxList[0].center.X);
                Console.WriteLine("ARectangleY:" + boxList[0].center.Y);


                //rectangle_location
                double[,] image_pix = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X_c = (world_cor[0, 0] / s) * 1000;
                world_Y_c = (world_cor[1, 0] / s) * 1000;

                g_AWRectX = world_X_c;
                g_AWRectY = world_Y_c;
                Console.WriteLine("g_AWRectX" + g_AWRectX);
                Console.WriteLine("g_AWRectY" + g_AWRectY);
            }

            /*输出圆的圆心*/
            //Console.WriteLine("圆形的个数：" + circles.Count());
            if (circles.Count() == 0 || (circles[0].Area >= 4600 || circles[0].Area <= 3000))
            {
                Console.WriteLine("Circle is no found!");

            }
            else
            {
                ACircleX = (double)(circles[0].Center.X);
                ACircleY = (double)(circles[0].Center.Y);
                Console.WriteLine("圆形的重心：" + circles[0].Center);
                //circle_location
                double[,] image_pix = new double[3, 1] { { ACircleX }, { ACircleY }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X_circle = (world_cor[0, 0] / s) * 1000;
                world_Y_circle = (world_cor[1, 0] / s) * 1000;

                g_AWCircleX = world_X_circle;
                g_AWCircleY = world_Y_circle;

                Console.WriteLine("g_AWCircleX:" + g_AWCircleX);
                Console.WriteLine("g_AWCircleY:" + g_AWCircleY);
            }

            return 15;
        }
        #endregion


        /* ****
         * Modbus程序
         * ****/
        #region /***小端封装***/
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
        #region /***ILog 成员***/
        public void Write(string log)
        {
            this.tbxHistory.AppendText(log + Environment.NewLine);
            this.tbxHistory.Select(this.tbxHistory.TextLength, 0);
            this.tbxHistory.ScrollToCaret();
        }
        #endregion
        #region /***释放Modbus资源***/
        private void TestModBus_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Wrapper.Dispose();
        }
        #endregion

        #region /***接收来自Modbus端值：若为1则表示需要抓取物件，若为2则表示需要松开物件***/
        private int RecieveFlagFromModbus()
        {
            byte[] FlagResult = this.Wrapper.Receive();
            Console.WriteLine("FlagResult.Length:" + FlagResult.Length);
            for (int i = 0; i < FlagResult.Length; i++)
            {
                Console.WriteLine("before byte b[{0}]:{1},", i, FlagResult[i]);
            }


            return FlagResult[1];
        }
        #endregion
        #region /***将世界坐标值传到机械手端***/
        private void SendXY2ModBus(double WorldX, double WorldY, int Flag)
        {
            WorldX = WorldX * 1000;
            Int32 Int32WorldX = (Int32)WorldX;

            WorldY = WorldY * 1000;
            Int32 Int32WorldY = (Int32)WorldY;

            Int32 Int32WorldZ = 0;

            Int32 FlagDone = (Int32)Flag;

            byte[] a = BitConverter.GetBytes(Int32WorldX);
            a = LittleEncodingFloat(a);

            byte[] b = BitConverter.GetBytes(Int32WorldY);
            //for (int i = 0; i < b.Length; i++)
            //{
            //    Console.WriteLine("before byte b[{0}]:{1},", i, b[i]);
            //}
            b = LittleEncodingFloat(b);

            byte[] c = BitConverter.GetBytes(Int32WorldZ);
            c = LittleEncodingFloat(c);

            byte[] d = BitConverter.GetBytes(FlagDone);
            d = LittleEncodingFloat(d);

            byte[] z = new byte[a.Length + b.Length + c.Length + d.Length];
            a.CopyTo(z, 0);
            b.CopyTo(z, a.Length);
            c.CopyTo(z, a.Length + b.Length);
            d.CopyTo(z, a.Length + b.Length + c.Length);

            this.Wrapper.Send(z);
        }
        #endregion

        /* ****
         * 机械爪的窗口通讯程序代码
         *（1）按键设置Baudrate等操作 
         *（2）做一个发送和接受串口信号的程序
         *（3）String 转 16进制的发送函数
         * ****/
        #region /***串口通讯控制机械爪***/
        //Instantiate SerialPort
        SerialPort sp = new SerialPort();

        //Define 4 Common Variable to transfer parameters between two winforms
        public static string strPortName = "";
        public static string strBaudRate = "";
        public static string strDataBits = "";
        public static string strStopBits = "";

        //抓手最大速度释放，成功返回20
        private int RS232_Releasing()
        {
            txtShow_Recieved.Clear();
            txtShow_Recieved.AppendText("Releasing");
            txtShow_Recieved.ScrollToCaret();

            if (sp.IsOpen)
            {
                Console.WriteLine("<<Release Sending>>");
                //高低位要分出来
                byte[] SSReleaseMax = new byte[] { 0xEB, 0x90, 0x01, 0x03, 0x11, 0xE8, 0x03, 0x00 };

                sp.Write(SSReleaseMax, 0, SSReleaseMax.Length);
            }
            else
            {

                MessageBox.Show("sp is not open!");
            }
            return 20;
        }
        //抓手最大速度抓取，成功返回10
        private int RS232_Grabing()
        {
            txtShow_Recieved.Clear();
            txtShow_Recieved.AppendText("Grabing");
            txtShow_Recieved.ScrollToCaret();

            Console.WriteLine("<<Grabing Sending>>");
            ChangeGrabArray(textSpeed.Text, textPower.Text);
            return 10;
        }

        //Grabing==>设置夹爪的阈值和速度（默认是255，500）
        private void btnGrabing_Click(object sender, EventArgs e)
        {
            txtShow_Recieved.Clear();
            txtShow_Recieved.AppendText("Grabing");
            txtShow_Recieved.ScrollToCaret();

            Console.WriteLine("<<Grabing Sending>>");
            ChangeGrabArray(textSpeed.Text, textPower.Text);
        }
        private void ChangeGrabArray(string Speed, string Power)
        {
            if (btnOpenSerial.Text == "OpenSerial")
            {
                MessageBox.Show("Set the SerialPort and open it!");
            }
            else
            {
                if (Power == "" && Speed == "")
                {
                    g_GrabArray[5] = (byte)0x00;
                    g_GrabArray[6] = (byte)0x00;
                    g_GrabArray[7] = (byte)0x00;
                    g_GrabArray[8] = (byte)0x00;
                    g_GrabArray[5] = (byte)0xFF;
                    g_GrabArray[6] = (byte)0xF4;
                    g_GrabArray[7] = (byte)0x01;

                    int ADD = 0;
                    for (int i = 2; i < g_GrabArray.Length - 1; i++)
                    {
                        ADD += g_GrabArray[i];
                    }
                    byte[] ByteADD = BitConverter.GetBytes(ADD);

                    //BitConverter.GetByte()=>[0]:low;[1]:high
                    g_GrabArray[8] = ByteADD[0];
                    sp.Write(g_GrabArray, 0, g_GrabArray.Length);
                }
                else if (Power == "")
                {
                    MessageBox.Show("Please set the Power & Speed correct,Or you can ignore it!!!");
                }
                else if (Speed == "")
                {
                    MessageBox.Show("Please set the Power & Speed correct,Or you can ignore it!!!");
                }

                else
                {
                    g_GrabArray[5] = (byte)0x00;
                    g_GrabArray[6] = (byte)0x00;
                    g_GrabArray[7] = (byte)0x00;
                    g_GrabArray[8] = (byte)0x00;
                    //TODO:精华所在
                    int IntSpeed = int.Parse(Speed);
                    byte[] ByteSpeed = BitConverter.GetBytes(IntSpeed);
                    if (IntSpeed < 1 || IntSpeed > 255)
                    {
                        MessageBox.Show("Input Speed is wrong!");
                    }
                    else
                    {
                        //high&low transfer
                        g_GrabArray[5] = ByteSpeed[0];//low
                    }

                    int IntPower = int.Parse(Power);
                    byte[] BytePower = BitConverter.GetBytes(IntPower);
                    if (IntPower < 100 || IntPower > 1500)
                    {
                        MessageBox.Show("Input Power is wrong!");
                    }
                    else
                    {
                        g_GrabArray[6] = ByteSpeed[0];
                        g_GrabArray[7] = ByteSpeed[1];
                    }
                    //TODO:Notice
                    int ADD = 0;
                    for (int i = 2; i < g_GrabArray.Length - 1; i++)
                    {
                        ADD += g_GrabArray[i];
                    }
                    byte[] ByteADD = BitConverter.GetBytes(ADD);
                    g_GrabArray[8] = ByteADD[0];
                    timer1.Enabled = true;
                    sp.Write(g_GrabArray, 0, g_GrabArray.Length);
                }
            }
        }

        //Releasing==>以最大速度放开:1000
        private void btnReleaseMax_Click(object sender, EventArgs e)
        {
            txtShow_Recieved.Clear();
            txtShow_Recieved.AppendText("Releasing");
            txtShow_Recieved.ScrollToCaret();

            if (sp.IsOpen)
            {
                Console.WriteLine("<<Release Sending>>");
                //高低位要分出来
                byte[] SSReleaseMax = new byte[] { 0xEB, 0x90, 0x01, 0x03, 0x11, 0xE8, 0x03, 0x00 };

                sp.Write(SSReleaseMax, 0, SSReleaseMax.Length);
            }
            else
            {

                MessageBox.Show("sp is not open!");
            }
        }
        private void btnSetPorts_Click(object sender, EventArgs e)
        {
            g_GrabArray[5] = (byte)0x00;
            g_GrabArray[6] = (byte)0x00;
            g_GrabArray[7] = (byte)0x00;
            g_GrabArray[8] = (byte)0x00;
            //Main WinForm
            sp.Close();
            //Call the new SetPorts Winform
            SetPorts SetPort_dlg = new SetPorts();
            if (SetPort_dlg.ShowDialog() == DialogResult.OK)
            {
                sp.PortName = strPortName;
                //Parse:用来类型转换的，将string 类型转换成int类型
                sp.BaudRate = int.Parse(strBaudRate);
                sp.DataBits = int.Parse(strDataBits);
                sp.StopBits = (StopBits)int.Parse(strStopBits);
                sp.ReadTimeout = 5000;
                sp.WriteTimeout = 5000;
            }
        }
        private void btnOpenSerial_Click(object sender, EventArgs e)
        {
            if (btnOpenSerial.Text == "OpenSerial")
            {
                if (strPortName != "" && strBaudRate != "" && strDataBits != "" && strStopBits != "")
                {
                    try
                    {
                        if (sp.IsOpen)
                        {
                            sp.Close();
                            sp.Open();
                            //open the serial port
                        }
                        else
                        {
                            sp.Open();
                        }
                        btnOpenSerial.Text = "Close";
                        groupBox2.Enabled = true;
                        groupBox3.Enabled = true;

                        this.toolStripStatusLabel1.Text = "SPNumber: " + sp.PortName + " |";
                        this.toolStripStatusLabel2.Text = "SPBaudRate: " + sp.BaudRate + " |";
                        this.toolStripStatusLabel3.Text = "SPDataBits: " + sp.DataBits + " |";
                        this.toolStripStatusLabel4.Text = "SPStopBit " + sp.StopBits + " |";
                        this.toolStripStatusLabel5.Text = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("错误：" + ex.Message, "C# SerialPorts Communication");
                    }
                }
                else
                {
                    MessageBox.Show("Please Set the Serial Ports First!", "RS232 SerialPorts Communication");
                }
            }
            else
            {
                btnOpenSerial.Text = "OpenSerial";
                if (sp.IsOpen)
                    sp.Close();
                groupBox2.Enabled = false;
                groupBox3.Enabled = false;

                this.toolStripStatusLabel1.Text = "SPNumber:Ports failed | ";
                this.toolStripStatusLabel1.Text = "SPBaudRate:Ports failed | ";
                this.toolStripStatusLabel1.Text = "SPDataBits:Ports failed | ";
                this.toolStripStatusLabel1.Text = "SPStopBit:Ports failed | ";
                this.toolStripStatusLabel1.Text = "";
            }
        }
        #endregion

        private void timer2_Tick(object sender, EventArgs e)
        {

            myFactory.CameraList[1].StartImageAcquisition(true, 5, pictureBox_B.Handle);
            myFactory.CameraList[1].SaveNextFrame(ImagePath_B);

            ImageProcess_B(ImagePath_B);

            ImageProcess_C(ImagePath_C);
            //double a = 22.2223;
            //double b = 223.1111F;


            ////float b = 111.22F;
            ////int b = 12;
            //SendXY2ModBus(a, b);


            //g_ModbusFlagResult = RecieveFlagFromModbus();
            //Console.WriteLine("ModbusFlagResult:{0}", g_ModbusFlagResult);
        }

        private void btnImageProcess_Click(object sender, EventArgs e)
        {


            int PCAXY = 0;
            int PCCXY = 0;

            int PCGrabDone = 10;
            int PCReleaseDone = 20;
            int MechineFlag = 0;
            int MechineFlag2 = 0;



            //PCAXY == 14 表示得到了A相机处理后的结果
            PCAXY = ImageProcess_A(ImagePath_A);

            SendXY2ModBus(g_AWRectX, g_AWRectY, PCAXY);
            do
            {
                MechineFlag = RecieveFlagFromModbus();
                Console.WriteLine("MechineFlag:{0}", MechineFlag);
            } while (MechineFlag == 1);
            Thread.Sleep(2000);
            //抓取指令
            RS232_Grabing();
            Thread.Sleep(2000);

            SendXY2ModBus(g_AWRectX, g_AWRectY, PCGrabDone);

            PCCXY = ImageProcess_C(ImagePath_C);
            do
            {
                SendXY2ModBus(g_AWRectX, g_AWRectY, PCCXY);
                Console.WriteLine("+++++++++");
            } while (PCCXY == 15);

            do
            {
                MechineFlag2 = RecieveFlagFromModbus();
                Console.WriteLine("==========");
            }while(MechineFlag2 == 2);

            Thread.Sleep(2000);
            //RS232_Releasing();
            SendXY2ModBus(g_AWRectX, g_AWRectY, PCReleaseDone);

        }
    }


}

        #region /*注释补充*/
        /**************************************************/
#region /***将世界坐标值传到机械手端*******/
        ////***将世界坐标值传到机械手端*******/
        //private void SendDataToModBus(double WorldX,double WorldY)
        //{
        //    Int32 x = (Int32)(WorldX);
        //    Int32 y = (Int32)(WorldY);
        //    Console.WriteLine("传给modbus_x:" + x);
        //    Console.WriteLine("传给modbus_y:" + y);
        //    //
        //    Int32 m = (Int32)(0.011);

        //    byte[] a = BitConverter.GetBytes(x);
        //    a = LittleEncodingFloat(a);
        //    byte[] b = BitConverter.GetBytes(y);
        //    b = LittleEncodingFloat(b);
        //    byte[] c = BitConverter.GetBytes(m);
        //    c = LittleEncodingFloat(c);
        //    byte[] z = new byte[a.Length + b.Length + c.Length];
        //    a.CopyTo(z, 0);
        //    b.CopyTo(z, a.Length);
        //    c.CopyTo(z, a.Length + b.Length);
        //    //TODO:利用Modbus发送坐标
        //    this.Wrapper.Send(z);
        //}
#endregion

#region byte[] => 16进制的string

        //public static string ToHexString ( byte[] bytes ) 
        //{
        //    string hexString = string.Empty;

        //    if ( bytes != null )

        //    {                
        //        StringBuilder strB = new StringBuilder ();

        //        for ( int i = 0; i < bytes.Length; i++ )
        //        {                    
        //            strB.Append ( bytes[i].ToString ( "X2" ) );                
        //        }
        //        Console.WriteLine("bytes:" + bytes.Length);
        //        hexString = strB.ToString ();            

        //    }
        //    return hexString;        
        //}
#endregion

#region timer1
        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    Console.WriteLine("进入timer1!");
        //    //sp.Write(g_SReadStatus, 0, g_g_SReadStatus.Length);

        //    byte[] readBuffer = new byte[50];
        //    ////Console.WriteLine("readBuffer.Length:{0}",readBuffer.Length);

        //    g_IsRead = sp.Read(readBuffer, 0, 49);
        //    Console.WriteLine("g_IsRead:{0}", g_IsRead);
        //    //串口通讯超时问题
        //    if (g_IsRead == 0)
        //    {
        //        timer1.Enabled = false;
        //        timer1.Enabled = true;
        //        Console.WriteLine("g_IsRead == 0");
        //    }
        //    else
        //    {
        //        if (readBuffer[4] == 16)
        //        {
        //            string recieveStr = ToHexString(readBuffer);

        //            txtShow_Recieved.Clear();
        //            txtShow_Recieved.AppendText(recieveStr.ToString() + "Grab!!!");
        //            txtShow_Recieved.ScrollToCaret();
        //        }
        //        else
        //        {
        //            string recieveStr = ToHexString(readBuffer);

        //            txtShow_Recieved.Clear();
        //            txtShow_Recieved.AppendText(recieveStr.ToString() + "Release!!!");
        //            txtShow_Recieved.ScrollToCaret();
        //        }
        //    }
        //    timer1.Enabled = false;
        //}
#endregion  

#region 注释--定时器来控制整个程序的图像处理
        //可能这里需要更换成定时器模式来完成
        /********线程控制-------main()-----图像处理*******/
        // private void timer2_Tick_1(object sender, EventArgs e)
        // {
        //     #region 5000毫秒进入一次图像处理
            
        //     //Console.WriteLine("image processing!!!");
        //     myFactory.CameraList[0].SaveNextFrame(ImagePath);
        //     //增加一个相机的图像处理操作
        //     myFactory.CameraList[1].SaveNextFrame(ImagePath1);

        //     ImageProcess_A(ImagePath);

        //     ImageProcess_C(ImagePath1);
        
        //     //Console.WriteLine("===Flag_t====" + Flag_t);


        //     //x = 34.3333;
        //     //point_X = (Int32)(circles[0].Center.X);
        //     //point_Y = (Int32)(circles[0].Center.Y);
        //     // if (Flag_t == 0)
        //     // {
        //     //     SendDataToModBus(world_X,world_Y);  
        //     // }
        //     // else if (Flag_t == 1)
        //     // {
        //     //    //do something 
        //     // }
        //     // else
        //     // {
        //     //     Flag_t = 1;
        //     // }

        //     #endregion
        // }
#endregion

#region 将LineSegment2D[] lines去掉看看效果
        //将LineSegment2D[] lines去掉看看效果
        // LineSegment2D[] lines = cannyEdges.HoughLinesBinary(
        //     1, //Distance resolution in pixel-related units
        //     Math.PI / 90.0, //Angle resolution measured in radians.
        //     20, //threshold
        //     30, //min Line width
        //     10 //gap between lines
        //     )[0]; //Get the lines from the first channel
#endregion

#region Modbus 的传送的数据格式处理
        //private void btnSend_Click_1(object sender, EventArgs e)
        //{
           
        
        //    string str1 = this.tbxSendText.Text.Trim().ToString();

        //    //x = 34.3333;
        //    //point_X = (Int32)(circles[0].Center.X);
        //    //point_Y = (Int32)(circles[0].Center.Y);

        //    Int32 x = (Int32)(world_X);

        //    Int32 y = (Int32)(world_Y);
        //    Console.WriteLine("传给modbus_x:" + x);
        //    Console.WriteLine("传给modbus_y:" + y);
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

#region 注释--相机放大缩小
///**********相机放大缩小***********/
//#region zoom放大缩小
//private void ZoomInbutton_Click(object sender, EventArgs e)
//{
//    //Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;
//    if (myFactory.CameraList[0] != null)
//        myFactory.CameraList[0].ZoomIn();
//}

//private void ZoomResetbutton_Click(object sender, EventArgs e)
//{
//    if (myFactory.CameraList[0] != null)
//        myFactory.CameraList[0].ZoomReset();
//}

//private void ZoomOutbutton_Click(object sender, EventArgs e)
//{
//    if (myFactory.CameraList[0] != null)
//        myFactory.CameraList[0].ZoomOut();
//}
#endregion

#region 注释--按一下处理图像并显示
        //
//public void button_circle_Click_1(object sender, EventArgs e)
//{
//    #region 按键图像处理
//    Console.WriteLine("1");
//    myFactory.CameraList[0].SaveNextFrame(".\\saveimg" + ".bmp");
//    ImageProcess();
//    #endregion
//}
#endregion

#region 注释--五角形的物体
 // foreach (MCvBox2D pen in pentagon)
            // {
            //     triangleRectangleImage.Draw(pen, new Bgr(Color.DarkOrange), 2);
            //     //Console.WriteLine("Center Of rectangle_penta 1:" + pen.center);
            //    // Console.WriteLine("Center Of rectangle_penta 2:" + pentagon[0].MinAreaRect());
            // }

//List<MCvBox2D> pentagon = new List<MCvBox2D>();
// else if (currentContour.Total == 5) //The contour has 5 vertices.
// {
//     #region determine if all the angles in the contour are within [65, 80] degree  5555
//     bool isPentagon = true;
//     Point[] pts2 = currentContour.ToArray();
//     // PointF[] p = p;
//     LineSegment2D[] edges = PointCollection.PolyLine(pts2, true);

//     for (int i = 0; i < edges.Length; i++)
//     {
//         double angle = Math.Abs(
//             edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
//         if (angle < 65 || angle > 80)
//         {
//             isPentagon = false;
//             break;
//         }
//     }
//     #endregion
//     if (isPentagon) pentagon.Add(currentContour.GetMinAreaRect());
// }
#endregion

#region 注释--图像处理画圆
            //CircleF[] circles = grayImage.HoughCircles(new Gray(250), new Gray(74.471), 1.0, grayImage.Width,0, 0)[0];//第二个参数
            /*在原图上画圆*/
            // Image<Bgr, Byte> imageLines = new Image<Bgr, Byte>(".\\saveimg" + ".bmp"); 
            // foreach (CircleF circle in circles)
            // {
            //    imageLines.Draw(circle, new Bgr(Color.Red), 2);
            /*输出圆的圆心*/
            //Console.WriteLine(circle.Center);
            // }
#endregion
    
#region  /*Form_load结束*/  
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
    //    //        MessageBox.Show(error.ToString(), "一号相机");
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
    //    //        MessageBox.Show(error.ToString(), "二号相机");
    //    //    }
    //    //    myCamera2.StretchLiveVideo = true;
    //    //    myCamera2.SkipImageDisplayWhenBusy = true;
    //    //    myCamera2.GetNode("TriggerMode").Value = "Off";

    //    //}
    //}
#endregion

#region /*注释--线程定时处理图像*/
        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    #region 500毫秒进行一次，将坐标值通过Modbus把值传给机械手
        //    timer1.Enabled = true;
        //    string str1 = this.tbxSendText.Text.Trim().ToString();
            
        //    //x = 34.3333;
        //    //point_X = (Int32)(circles[0].Center.X);
        //    //point_Y = (Int32)(circles[0].Center.Y);
        //    if (Flag_t == 2)
        //    {
        //        Int32 x = (Int32)(world_X);
        //        Int32 y = (Int32)(world_Y);
        //        Console.WriteLine("传给modbus_x:" + x);
        //        Console.WriteLine("传给modbus_y:" + y);
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
        //        Console.WriteLine("传给modbus_x:" + x);
        //        Console.WriteLine("传给modbus_y:" + y);
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
   // }  
//}
#endregion  

//测试一下FLY相机的隐藏功能
#region /*注释--Fly相机__hide()*/
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

#region /*注释--关于图像数组转换问题*/
/* 图像数组转换的问题
         * Image img = Properties.Resources.Form3_PIC_00;  //只能是system.drawing.image能读入，Mat和emgu的image读不了
            Bitmap bmpImage = new Bitmap(img); //这是关键，国外网站看到的
            Emgu.CV.Image<Bgr, Byte> currentFrame = new Emgu.CV.Image<Bgr, Byte>(bmpImage);  //只能这么转
       
            Mat invert = new Mat();
             CvInvoke.BitwiseAnd(currentFrame, currentFrame, invert);  //这是官网上的方法，变通用。没看到提供其它方法直接转换的。
         */
#endregion

#region /*注释--JAI相机初始化*/
/*************JAI__init***************/
//Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;
// Open the factory with the default Registry database
//error = myFactory.Open("");
// Search for cameras and update all controls
//SearchButton_Click(null, null);
#endregion
#endregion