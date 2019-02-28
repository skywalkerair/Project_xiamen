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
        //string ImagePath_A = ".\\saveimgA.jpg";
        string ImagePath_A = ".\\saveimgA.jpg";

        string ImagePath_B = ".\\saveimgB.jpg";
        string ImagePath_C = ".\\saveimgC.bmp";
        #endregion

        #region /***区分长方形、正方形和圆形的世界坐标点在A相机和C相机***/
        #region A相机的全局变量
        //A相机，中间变量，用来呈递参数
        double g_AtmpWCircleX = 0;
        double g_AtmpWCircleY = 0;
        double g_AtmpWRectX = 0;
        double g_AtmpWRectY = 0;

        //A相机中矩形的最后坐标值，传给机械手
        double g_AWRectX = 0;
        double g_AWRectY = 0;
        //A相机中圆的最后坐标值，传给机械手
        double g_AWCircleX = 0;
        double g_AWCircleY = 0;

        //A相机中检测到长矩形短矩形还是圆形的标志位
        int g_isARectMax = 0;
        int g_isARectMin = 0;

        int g_isACircle = 0;

        #endregion

        #region B相机的标志位
        //B相机检测到的形状，圆，长矩形，短矩形
        //g_BFlag = 0;初始值
        //g_BFlag = 1：检测到了圆形
        //g_BFlag = 2：检测到了长矩形
        //g_BFlag = 3：检测到了短矩形
        //int g_BFlag = 0;
        
        #endregion



        #region C相机的全局变量
        //C相机，中间变量，用来呈递参数
        double g_CtmpWCircleX = 0;
        double g_CtmpWCircleY = 0;
        double g_CtmpWRectX = 0;
        double g_CtmpWRectY = 0;

        //C相机中矩形的最后坐标值，传给机械手
        double g_CWRectMaxX = 0;
        double g_CWRectMaxY = 0;
        double g_CWRectMinX = 0;
        double g_CWRectMinY = 0;

        //C相机中圆的最后坐标值，传给机械手
        double g_CWCircleX = 0;
        double g_CWCircleY = 0;
        //C相机的角度值
        double g_CWRectMaxAngle = 0;
        double g_CWRectMinAngle = 0;

        #endregion
        #endregion

        #region /***设置区分长方形，正方形和圆形的标志位***/
        //TODO:Flag_B标志位表示B相机是否检测到了物体的形状，
        //为1则表示检测到了长方形，
        //2则表示检测到了正方形，
        //3则表示检测到了圆形
        //int Flag_B = 1;

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
            //TODO：注释掉了下面一行
            Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

            // Open the factory with the default Registry database
            //TODO:注释掉了下面1行
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
            //timer2.Enabled = false;

            #region /***Fly-C相机__hide()***/
            CameraSelectionDialog camSlnDlg = new CameraSelectionDialog();
            bool retVal = camSlnDlg.ShowModal();
            if (retVal)
            {
                try
                {
                    ManagedPGRGuid[] selectedGuids = camSlnDlg.GetSelectedCameraGuids();
                    //TODO:注释掉了下面1行
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
        private void SearchButton_Click(object sender, EventArgs e)
        {
            Jai_FactoryWrapper.EFactoryError error = Jai_FactoryWrapper.EFactoryError.Success;

            // enable Force IP
            myFactory.EnableAutoForceIP = true;
            // Search for any new cameras using Filter Driver
            //TODO:注释掉了下面1行
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
            RS232_Releasing();
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

            //"20192021:测试A相机的四种情况；取最左边X的坐标值；图像处理情况"
            //TODO：Done完成了
            //int tmpAReturn = ImageProcess_A(ImagePath_A);
            //Console.WriteLine("tmpAReturn:"+tmpAReturn);

            //double CircleHeight = -168.26 ;
            //SendXY2ModBus(0, 0,CircleHeight,10);

            //"20192021:测试C相机的四种情况；取最左边X的坐标值；图像处理情况"
            //TODO：
            //int tmpAReturn = ImageProcess_A(ImagePath_A);
            //Console.WriteLine("tmpAReturn:" + tmpAReturn);
            //Console.WriteLine("Before:g_BFlag:" + g_BFlag);
           //ImageProcess_B(ImagePath_B);
            //Console.WriteLine("After:g_BFlag:" + g_BFlag);
            //Console.WriteLine("tmpBReturn:", tmpAReturn);
            //int tmpCReturn = ImageProcess_C(ImagePath_C);
            //Console.WriteLine("tmpCReturn:", tmpCReturn);
            //int returnB = 0;
            //int oneCount = 0;
            //int twoCount = 0;
            //int threeCount = 0;
            //List<int> ResultBCount = new List<int>();
            //for (int i = 0; i < 5; i++)
            //{
            //    myFactory.CameraList[1].SaveNextFrame(ImagePath_B);
            //    returnB = ImageProcess_B(ImagePath_B);
            //    ResultBCount.Add(g_BFlag);
            //}
            //foreach (int j in ResultBCount)
            //{
            //    //Console.WriteLine("ResultBCount:" + j);
            //    if (j == 1)
            //    {
            //        oneCount += 1;
            //    }
            //    else if (j == 2)
            //    {
            //        twoCount += 1;
            //    }
            //    else
            //    {
            //        threeCount += 1;
            //    }
            //    if (oneCount >= twoCount)
            //    {
            //        if (oneCount >= threeCount)
            //        {
            //            g_BFlag = 1;
            //            Console.WriteLine("1最多！");
            //        }
            //    }
            //    else
            //    {//2多
            //        if (twoCount >= threeCount)
            //        {
            //            g_BFlag = 2;
            //            Console.WriteLine("2最多！");
            //        }
            //        else
            //        {
            //            g_BFlag = 3;
            //            Console.WriteLine("3最多！");
            //        }
            //    }
            //}

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

        /* ***
         * 关于图像处理程序
         * ***/
         /* ***
          * A相机处理结果
          * 返回10：A相机传值给modbus成功;
          * 返回11：A相机中既没有圆形也没有矩形
          * ***/
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
                MedianImage.Width/2, //min distance ;TODO:这里改成了1/2所以能检测到两个圆
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion

            #region Canny and edge detection
            double cannyThresholdLinking = 50.0;
            Image<Gray, Byte> cannyEdges = MedianImage.Canny(cannyThreshold, cannyThresholdLinking);
            //TODO:将结果显示在B中
           // pictureBox_B_Processing.Image = cannyEdges.ToBitmap();
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
                   //Console.WriteLine("currentContour.Area:" + currentContour.Area);
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
                            if (isRectangle)
                            {
                                //将90度的矩形加入boxList
                                boxList.Add(currentContour.GetMinAreaRect());
                            } 
                        }
                    }
                }
            #endregion
            //Console.WriteLine("Before boxList.size:" + boxList.Count());
            
            #region 将重复的很近的矩形去掉
            for (int q = 0; q<boxList.Count()-1;q++ )
            {
                //先看质点X的距离
                if (Math.Abs(boxList[q].center.X - boxList[q + 1].center.X) <= 10)
                {
                    boxList.RemoveAt(q);
                }
                else {
                    continue;
                }
                //Console.WriteLine("boxList[{0}].Area:{1}",q,boxList[q].MinAreaRect().Size);
            }
            #endregion
            //Console.WriteLine("After boxList.size:" + boxList.Count());

            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            
            //draw the rectangles           
            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.Red), 2);
                //Console.WriteLine("矩形1：Width:{0},Height:{1}.", box1.size.Width, box1.size.Height);
              
            }

            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circle.Area);
                Console.WriteLine("In A cemera,AreaCircle.Area:"+AreaCircle);
                if (AreaCircle >= 10000&&AreaCircle<=20000 )
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
          
            #region 对A中形状数量的判断  

            #region Situation 1 ：A相机中既没有圆也没有矩形
            if (boxList.Count() == 0 && (circles.Count() == 0 || (circles[0].Area >= 20000 || circles[0].Area <= 10000)))
            {
                Console.WriteLine("++++++++In Camera A,Situation 1!++++++");
                Console.WriteLine("In Camera A,The Rectangle and the Circle are no found!");
                return 11;
            }
            #endregion

            #region Situation 2 :只有圆形
            else if (boxList.Count() == 0)
            {

                Console.WriteLine("++++++++In Camera A,Situation 2!++++++");
                 if (circles.Count() == 0 || (circles[0].Area >= 20000 || circles[0].Area <= 10000))
                {
                    Console.WriteLine("In Camera A,The Rectangle and the Circle are no found!");
                    return 11;
                }
                 else{//对圆形排序
                    //Console.WriteLine("圆形的个数：" + circles.Count());
                    double tmpMinc = circles[0].Center.X;//记录第一个圆形X为初始值
                    int tmpRecordIc = 0;//记录I的值，最小圆形的序号
                    for (int RectBig2 = 0; RectBig2 < circles.Count(); RectBig2++)
                    {
                        //找最小值
                        if (circles[RectBig2].Center.X <= tmpMinc)
                        {
                            tmpMinc = circles[RectBig2].Center.X;
                            tmpRecordIc = RectBig2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    ACircleX = (double)(circles[tmpRecordIc].Center.X);
                    ACircleY = (double)(circles[tmpRecordIc].Center.Y);
                    Console.WriteLine("圆形的重心：" + circles[tmpRecordIc].Center);
                    //circle_location
                    double[,] image_pix_circle = new double[3, 1] { { ACircleX }, { ACircleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_circle, ref world_cor);

                    g_AtmpWCircleX = (world_cor[0, 0] / s) * 1000;
                    g_AtmpWCircleY = (world_cor[1, 0] / s) * 1000;

                    g_AWCircleX = g_AtmpWCircleX;
                    g_AWCircleY = g_AtmpWCircleY;
                    Console.WriteLine("A相机中的圆");
                    Console.WriteLine("g_AWCircleX:" + g_AWCircleX);
                    Console.WriteLine("g_AWCircleY:" + g_AWCircleY);
                    g_isACircle = 1;      
                 }   
            }
            #endregion

            #region Situation 3 :只有矩形
            else if (circles.Count() == 0 || (circles[0].Area >= 20000 || circles[0].Area <= 10000))
            {
                /*
                 * 如果圆不存在，则判断矩形，矩形的数量
                 */
                Console.WriteLine("++++++++In Camera A,Situation 3!++++++");
                if (boxList.Count() == 0)
                {
                    Console.WriteLine("In Camera A,The Rectangle and the Circle are no found!");
                    return 11;
                }
                else
                {//对矩形排序
                    #region 判断矩形中最左边的和圆形中最左边的
                    double tmpMin = boxList[0].center.X;//记录第一个矩形X为初始值
                    int tmpRecordI = 0;//记录I的值，最小矩形的序号
                    for (int RectBig1 = 0; RectBig1 < boxList.Count(); RectBig1++)
                    {
                        //找最小值
                        if (boxList[RectBig1].center.X <= tmpMin)
                        {
                            tmpMin = boxList[RectBig1].center.X;
                            tmpRecordI = RectBig1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    #endregion

                        //TODO:增加一个判定是长矩形还是短矩形，需要设置抓手的高度
                        if (Math.Abs(boxList[tmpRecordI].size.Height - boxList[tmpRecordI].size.Width) >= 10)
                        {
                            //说明是长矩形
                            g_isARectMax = 1;
                            Console.WriteLine("g_isARectMax:" + g_isARectMax);
                        }
                        else
                        { //说明是短矩形
                            g_isARectMin = 1;
                            Console.WriteLine("g_isARectMin:" + g_isARectMin);
                        }
                        //图像中x,y的坐标位置
                        ARectangleX = (double)(boxList[tmpRecordI].center.X);
                        ARectangleY = (double)(boxList[tmpRecordI].center.Y);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("ARectangleX[0]:" + boxList[tmpRecordI].center.X);
                        Console.WriteLine("ARectangleY[0]:" + boxList[tmpRecordI].center.Y);
                        Console.WriteLine("ARectangleY[0]:" + boxList[tmpRecordI].angle);

                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        g_AtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_AtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_AWRectX = g_AtmpWRectX;
                        g_AWRectY = g_AtmpWRectY;
                        Console.WriteLine("g_AWRectX[0]" + g_AWRectX);
                        Console.WriteLine("g_AWRectY[0]" + g_AWRectY);
                    }
                }
            #endregion

            #region Situation 4 :既有圆形又有矩形
            else
            {
                Console.WriteLine("++++++++In Camera A,Situation 4!++++++");
                #region 判断矩形中最左边的和圆形中最左边的
                double tmpMin = boxList[0].center.X;//记录第一个矩形X为初始值
                int tmpRecordI = 0;//记录I的值，最小矩形的序号
                for (int RectBig1 = 0; RectBig1 < boxList.Count(); RectBig1++)
                {
                    //找最小值
                    if (boxList[RectBig1].center.X <= tmpMin)
                    {
                        tmpMin = boxList[RectBig1].center.X;
                        tmpRecordI = RectBig1;
                    }
                    else
                    {
                        continue;
                    }
                }
                double tmpMinc = circles[0].Center.X;//记录第一个圆形X为初始值
                int tmpRecordIc = 0;//记录I的值，最小圆形的序号
                for (int RectBig2 = 0; RectBig2 < circles.Count(); RectBig2++)
                {
                    //找最小值
                    if (circles[RectBig2].Center.X <= tmpMinc)
                    {
                        tmpMinc = circles[RectBig2].Center.X;
                        tmpRecordIc = RectBig2;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (boxList[tmpRecordI].center.X <= circles[tmpRecordIc].Center.X)
                {
                    //TODO:增加一个判定是长矩形还是短矩形，需要设置抓手的高度
                    if (Math.Abs(boxList[tmpRecordI].size.Height - boxList[tmpRecordI].size.Width) >= 10)
                    {
                        //说明是长矩形
                        g_isARectMax = 1;
                    }
                    else { //说明是短矩形
                        g_isARectMin = 1;
                        Console.WriteLine("g_isARectMin:" + g_isARectMin);
                    }

                    //图像中x,y的坐标位置
                    ARectangleX = (double)(boxList[tmpRecordI].center.X);
                    ARectangleY = (double)(boxList[tmpRecordI].center.Y);
                    //Console.WriteLine("矩形的重心：" + boxList[0].center);
                   // Console.WriteLine("ARectangleX[1]:" + boxList[tmpRecordI].center.X);
                   // Console.WriteLine("ARectangleY[1]:" + boxList[tmpRecordI].center.Y);
                   // Console.WriteLine("ARectangleY[1]:" + boxList[tmpRecordI].angle);
                    //rectangle_location
                    double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                    g_AtmpWRectX = (world_cor[0, 0] / s) * 1000;
                    g_AtmpWRectY = (world_cor[1, 0] / s) * 1000;

                    g_AWRectX = g_AtmpWRectX;
                    g_AWRectY = g_AtmpWRectY;
                   // Console.WriteLine("g_AWRectX[1]:" + g_AWRectX);
                    //Console.WriteLine("g_AWRectY[1]:" + g_AWRectY);
                }
                else
                {
                    //圆形在左
                    ACircleX = (double)(circles[tmpRecordIc].Center.X);
                    ACircleY = (double)(circles[tmpRecordIc].Center.Y);
                   // Console.WriteLine("圆形的重心：" + circles[tmpRecordIc].Center);
                    //circle_location
                    double[,] image_pix_circle = new double[3, 1] { { ACircleX }, { ACircleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_circle, ref world_cor);

                    g_AtmpWCircleX = (world_cor[0, 0] / s) * 1000;
                    g_AtmpWCircleY = (world_cor[1, 0] / s) * 1000;

                    g_AWCircleX = g_AtmpWCircleX;
                    g_AWCircleY = g_AtmpWCircleY;
                    //Console.WriteLine("A相机中的圆");
                    //Console.WriteLine("g_AWCircleX:" + g_AWCircleX);
                   // Console.WriteLine("g_AWCircleY:" + g_AWCircleY);
                    
                    g_isACircle = 1;
                }
                #endregion
            }
            return 10;
            #endregion

            #endregion
            
        }

        /* ***
         * B相机处理结果
         * 识别出形状和角度
         * 返回 g_BFlagCircle;g_BFlagRectMax;g_BFlagRectMin;
         * 为 1 表示的是检测到了相应的形状
         * ***/
        private int ImageProcess_B(string ImagePath)
        {
            /*canny算子处理图像*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(ImagePath);
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();

            Image<Gray, Byte> MedianImage = grayImage.SmoothMedian(13);

            double cannyThreshold = 10.0;
            double circleAccumulatorThreshold = 50.0;

            #region Find circles
            /*检测圆形*/
            circles = MedianImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                1.5, //Resolution of the accumulator used to detect centers of the circles
                MedianImage.Width, //min distance 
                100, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion

            
            #region Canny and edge detection
            double cannyThresholdLinking = 180.0;
            Image<Gray, Byte> cannyEdges = MedianImage.Canny(cannyThreshold, cannyThresholdLinking);
            CvInvoke.cvDilate(cannyEdges, cannyEdges, IntPtr.Zero, 3);
            CvInvoke.cvErode(cannyEdges, cannyEdges, IntPtr.Zero, 2);
            //pictureBox_C_processed.Image = cannyEdges.ToBitmap();
            //cannyEdges.Save("Before_B_Binary.jpg");
            #endregion

            //存放矩形的形状
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
            // PointF[] GetVertices();
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            using (MemStorage storage1 = new MemStorage()) //allocate storage for contour approximation
                for (
                    Contour<Point> contours = cannyEdges.FindContours(
                        Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE,//CV_CHAIN_APPROX_SIMPLE
                        Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_LIST,
                        storage1);
                    contours != null;
                    contours = contours.HNext)
                {
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.02, storage1);//注意这里的The desired approximation accuracy为0.04
                    //Console.WriteLine("currentContour.BoundingRectangle:" + currentContour.BoundingRectangle);
                    //做第一道筛选，将小矩形去掉
                    if (currentContour.BoundingRectangle.Width >= 300)
                    {
                        Console.WriteLine("circles[0].Area :" + circles[0].Area);
                        //做第一道筛选，将小矩形去掉
                        if (circles.Count() == 1 && circles[0].Area >= 300000)
                        {
                            Console.WriteLine("B相机中检测结果是：圆形");
                            //g_BFlag = 1;
                        }
                        else {
                            //区别长矩形和短矩形
                            if (Math.Abs((currentContour.BoundingRectangle.Width) - (currentContour.BoundingRectangle.Height)) >= 100)
                            {
                                //是长矩形
                                Console.WriteLine("B相机中检测结果是：长矩形");
                                boxList.Add(currentContour.GetMinAreaRect());//5188
                                //g_BFlag = 2;
                            }
                            else
                            { //是短矩形 
                                if (circles.Count() == 1)
                                {
                                    Console.WriteLine("B相机中检测结果是：圆形");
                                    //g_BFlag = 1;
                                }
                                else {
                                    Console.WriteLine("B相机中检测结果是：短矩形");
                                    boxList.Add(currentContour.GetMinAreaRect());
                                    Console.WriteLine("短矩形");
                                    //g_BFlag = 3;
                                }   
                            }
                        }
                    }
                   }
            //排除重合的矩形
            for (int q = 0; q < boxList.Count() - 1; q++)
            {
                //先看质点X的距离
                if (Math.Abs(boxList[q].center.X - boxList[q + 1].center.X) <= 20)
                {
                    boxList.RemoveAt(q);
                }
                else
                {
                    continue;
                }
            }
            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.Red), 2);
            }
            foreach (CircleF circle in circles)
            {
                //AreaCircle = (Int32)(circle.Area);
                //Console.WriteLine("In Camera C,the Area of Circle:" + AreaCircle);
                triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
            }
            //结果显示在pictureBox_B_Processing的方框中
            pictureBox_B_Processing.Image = triangleRectangleImage.ToBitmap();
            //Console.WriteLine("currentContour.area:" + currentContour.Area);    

            return 10;
        }

        /* ***
          * C相机处理结果
          * 返回20：C相机传值给modbus成功;
          * 返回21：C相机中既没有圆形也没有矩形
          * ***/
        //g_BFlag = 0;初始值
        //g_BFlag = 1：检测到了圆形
        //g_BFlag = 2：检测到了长矩形
        //g_BFlag = 3：检测到了短矩形
        private int ImageProcess_C(string ImagePath)
        {
            //矩形和圆形的中心点坐标
            double CRectangleX = 0;
            double CRectangleY = 0;

            double CRectangleYMin = 0;
            double CRectangleXMin = 0;

            double CRectangleYMax = 0;
            double CRectangleXMax = 0;

            double CCircleX = 0;
            double CCircleY = 0;

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
            //TODO:将结果显示在B中
           // pictureBox_B_Processing.Image = cannyEdges.ToBitmap();
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

            #region 去除掉相近的矩形
            //Console.WriteLine("Before boxList.size:"+boxList.Count());
            //将重复的很近的矩形去掉
            for (int q = 0; q < boxList.Count() - 1; q++)
            {
                //先看质点X的距离
                if (Math.Abs(boxList[q].center.X - boxList[q + 1].center.X) <= 10)
                {
                    boxList.RemoveAt(q);
                }
                else
                {
                    continue;
                }
                //Console.WriteLine("boxList[{0}].Area:{1}",q,boxList[q].MinAreaRect().Size);
            }
            //Console.WriteLine("After Before boxList.size:" + boxList.Count());
            #endregion

            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            //draw the rectangles           
            Console.WriteLine("C相机中的矩形的个数：" + boxList.Count());

            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.Red), 2);
            }

            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circle.Area);
                //Console.WriteLine("In Camera C,the Area of Circle:" + AreaCircle);
                if (AreaCircle <= 250000 && AreaCircle >=90000)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
                }
                else { continue; }
            }
            #endregion

            /*显示结果，在C相机的图像中显示出来*/
            pictureBox_C_processed.Image = triangleRectangleImage.ToBitmap();

            double[,] a = new double[3, 3] { { fc1_c, 0, cc1_c }, { 0, fc2_c, cc2_c }, { 0, 0, 1 } };
            double[,] b = new double[3, 3] { { R11_c, R21_c, T1_c }, { R12_c, R22_c, T2_c }, { R13_c, R23_c, T3_c } };

            #region 对C中形状数量的判断
            #region Situation 1 ：C相机中既没有圆也没有矩形
            if (boxList.Count() == 0 && (circles.Count() == 0 || (circles[0].Area >= 250000 || circles[0].Area <= 90000)))
            {
                Console.WriteLine("+++In Camera C,Situation 1+++");
                Console.WriteLine("In Camera C,The Rectangle and the Circle are no found!");
                return 21;
            }
            #endregion

            #region Situation 2 :只有圆形
            else if (boxList.Count() == 0)
            {
                Console.WriteLine("+++In Camera C,Situation 2+++");
                /*输出圆的圆心*/
                Console.WriteLine("圆形的个数：" + circles.Count());
                if (circles.Count() == 0 || (circles[0].Area >= 250000 || circles[0].Area <= 90000))
                {
                    Console.WriteLine("In Camera C,The Rectangle and the Circle are no found!");
                    return 21;
                }
                else
                {
                    //只有一个圆形，所以直接给坐标
                    CCircleX = (double)(circles[0].Center.X);
                    CCircleY = (double)(circles[0].Center.Y);
                    //Console.WriteLine("圆形的重心：" + circles[tmpRecordIc].Center);
                    //circle_location
                    double[,] image_pix_circle = new double[3, 1] { { CCircleX }, { CCircleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_circle, ref world_cor);

                    g_CtmpWCircleX = (world_cor[0, 0] / s) * 1000;
                    g_CtmpWCircleY = (world_cor[1, 0] / s) * 1000;

                    g_CWCircleX = g_CtmpWCircleX;
                    g_CWCircleY = g_CtmpWCircleY;
                    Console.WriteLine("C相机中的圆");
                    Console.WriteLine("g_CWCircleX:" + g_CWCircleX);
                    Console.WriteLine("g_CWCircleY:" + g_CWCircleY);   
                }
            }
            #endregion

            #region Situation 3 :只有矩形
            else if (circles.Count() == 0 || (circles[0].Area >= 250000 || circles[0].Area <= 90000))
            {
                /*
                 * 如果圆不存在，则判断矩形，矩形的数量
                 */
                Console.WriteLine("+++In Camera C,Situation 3+++");
                //TODO:这里需要知道长矩形和短矩形的Width，待定需要测量
                //width:256;height:250
                //width;358;height:236
                //for (int m = 0; m < boxList.Count(); m++)
                //{
                //    Console.WriteLine("In Camera C,boxlist[{0}]:width:{1},height:{2}", m, boxList[m].size.Width, boxList[m].size.Height);
                //}
                 
                if (boxList.Count() == 0)
                {
                    Console.WriteLine("In Camera C,The Rectangle and the Circle are no found!");
                    return 21;
                }
                #region C相机中只有一个矩形
                else if (boxList.Count() == 1)
                {
                    if ((boxList[0].size.Width * boxList[0].size.Height) >= 70000)//C中的是长矩形
                        {
                            //图像中x,y的坐标位置
                            CRectangleX = (double)(boxList[0].center.X);
                            CRectangleY = (double)(boxList[0].center.Y);
                            g_CWRectMaxAngle = (double)(boxList[0].angle);
                            //Console.WriteLine("矩形的重心：" + boxList[0].center);
                            Console.WriteLine("g_CWRectMaxAngle:" + boxList[0].angle);

                            //rectangle_location
                            double[,] image_pix_rect = new double[3, 1] { { CRectangleX }, { CRectangleY }, { 1 } };

                            Matrix.MatrixMultiply(a, b, ref c);
                            Matrix.MatrixOpp(c, ref c_);
                            Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                            g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                            g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                            g_CWRectMaxX = g_CtmpWRectX;
                            g_CWRectMaxY = g_CtmpWRectY;
                            Console.WriteLine("g_CWRectMaxX[0]" + g_CWRectMaxX);
                            Console.WriteLine("g_CWRectMaxY[0]" + g_CWRectMaxY);
                        }
                        else//C相机中的是短矩形
                        {
                            //图像中x,y的坐标位置
                            CRectangleX = (double)(boxList[0].center.X);
                            CRectangleY = (double)(boxList[0].center.Y);
                            g_CWRectMinAngle = (double)(boxList[0].angle);
                            //Console.WriteLine("矩形的重心：" + boxList[0].center);
                            Console.WriteLine("g_CWRectMinAngle[0]:" + boxList[0].angle);

                            //rectangle_location
                            double[,] image_pix_rect = new double[3, 1] { { CRectangleX }, { CRectangleY }, { 1 } };

                            Matrix.MatrixMultiply(a, b, ref c);
                            Matrix.MatrixOpp(c, ref c_);
                            Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                            g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                            g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                            g_CWRectMinX = g_CtmpWRectX;
                            g_CWRectMinY = g_CtmpWRectY;
                            Console.WriteLine("g_CWRectMinX[0]" + g_CWRectMinX);
                            Console.WriteLine("g_CWRectMinY[0]" + g_CWRectMinY);
                        }
                }
                #endregion
                #region C相机中有两个
                else
                {
                    //for (int m = 0; m < boxList.Count(); m++)
                    //{
                    //    Console.WriteLine("In Camera C,boxlist[{0}]:area:{1}", m, (boxList[m].size.Width * boxList[m].size.Height));
                    //}
                    //此时的矩形是两个
                    #region 先将C相机中的长短矩形判断出来
                    if ((boxList[0].size.Width * boxList[0].size.Height) <= (boxList[1].size.Width * boxList[1].size.Height))
                    {
                        //图像中x,y的坐标位置
                        CRectangleXMin = (double)(boxList[0].center.X);
                        CRectangleYMin = (double)(boxList[0].center.Y); 
                        g_CWRectMinAngle = (double)(boxList[0].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);

                        Console.WriteLine("g_CWRectMinAngle[0]:" + boxList[0].angle);

                        //需要区分长短矩形
                        double[,] image_pix_rect_Min = new double[3, 1] { { CRectangleXMin }, { CRectangleYMin }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Min, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMinX = g_CtmpWRectX;
                        g_CWRectMinY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMinX[0]" + g_CWRectMinX);
                        Console.WriteLine("g_CWRectMinY[0]" + g_CWRectMinY);

                        //图像中x,y的坐标位置
                        CRectangleXMax = (double)(boxList[1].center.X);
                        CRectangleYMax = (double)(boxList[1].center.Y);
                        g_CWRectMaxAngle = (double)(boxList[1].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("g_CWRectMaxAngle[1]:" + boxList[1].angle);
                        //rectangle_location
                        double[,] image_pix_rect_Max = new double[3, 1] { { CRectangleXMax }, { CRectangleYMax }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Max, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMaxX = g_CtmpWRectX;
                        g_CWRectMaxY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMaxX[1]:" + g_CWRectMaxX);
                        Console.WriteLine("g_CWRectMaxY[1]:" + g_CWRectMaxY);
                    }
                    else
                    {
                        //图像中x,y的坐标位置
                        CRectangleXMin = (double)(boxList[1].center.X);
                        CRectangleYMin = (double)(boxList[1].center.Y);
                        g_CWRectMinAngle = (double)(boxList[1].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("g_CWRectMinAngle[1]:" + boxList[1].angle);

                        //需要区分长短矩形
                        double[,] image_pix_rect_Min = new double[3, 1] { { CRectangleXMin }, { CRectangleYMin }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Min, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMinX = g_CtmpWRectX;
                        g_CWRectMinY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMinX[1]" + g_CWRectMinX);
                        Console.WriteLine("g_CWRectMinY[1]" + g_CWRectMinY);

                        //图像中x,y的坐标位置
                        CRectangleXMax = (double)(boxList[0].center.X);
                        CRectangleYMax = (double)(boxList[0].center.Y);
                        g_CWRectMaxAngle = (double)(boxList[0].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("CRectangleXMax[0]:" + boxList[0].center.X);
                        Console.WriteLine("CRectangleYMax[0]:" + boxList[0].center.Y);
                        Console.WriteLine("g_CWRectMaxAngle[0]:" + boxList[0].angle);
                        //rectangle_location
                        double[,] image_pix_rect_Max = new double[3, 1] { { CRectangleXMax }, { CRectangleYMax }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Max, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMaxX = g_CtmpWRectX;
                        g_CWRectMaxY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMaxX[0]:" + g_CWRectMaxX);
                        Console.WriteLine("g_CWRectMaxY[0]:" + g_CWRectMaxY);
                    }
                    #endregion

                }
                #endregion
            }
            #endregion

            #region Situation 4 :既有圆形又有矩形
            else
            {
                Console.WriteLine("+++In Camera C,Situation 4+++");
                //圆形先找出来
                CCircleX = (double)(circles[0].Center.X);
                CCircleY = (double)(circles[0].Center.Y);
                //Console.WriteLine("圆形的重心：" + circles[0].Center);
                //circle_location
                double[,] image_pix_circle = new double[3, 1] { { CCircleX }, { CCircleY }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix_circle, ref world_cor);

                g_CtmpWCircleX = (world_cor[0, 0] / s) * 1000;
                g_CtmpWCircleY = (world_cor[1, 0] / s) * 1000;

                g_CWCircleX = g_CtmpWCircleX;
                g_CWCircleY = g_CtmpWCircleY;
                //Console.WriteLine("C相机中的圆");
                Console.WriteLine("g_CWCircleX:" + g_CWCircleX);
                Console.WriteLine("g_CWCircleY:" + g_CWCircleY);
                if (boxList.Count() == 1)
                {
                    //只有一个矩形
                    if ((boxList[0].size.Width * boxList[0].size.Height) >= 70000)//C中的是长矩形
                    {
                        //图像中x,y的坐标位置
                        CRectangleX = (double)(boxList[0].center.X);
                        CRectangleY = (double)(boxList[0].center.Y);
                        g_CWRectMaxAngle = (double)(boxList[0].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("g_CWRectMaxAngle:" + boxList[0].angle);

                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { CRectangleX }, { CRectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMaxX = g_CtmpWRectX;
                        g_CWRectMaxY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMaxX[0]" + g_CWRectMaxX);
                        Console.WriteLine("g_CWRectMaxY[0]" + g_CWRectMaxY);
                    }
                    else//C相机中的是短矩形
                    {
                        //图像中x,y的坐标位置
                        CRectangleX = (double)(boxList[0].center.X);
                        CRectangleY = (double)(boxList[0].center.Y);
                        g_CWRectMinAngle = (double)(boxList[0].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("g_CWRectMinAngle[0]:" + boxList[0].angle);

                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { CRectangleX }, { CRectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMinX = g_CtmpWRectX;
                        g_CWRectMinY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMinX[0]" + g_CWRectMinX);
                        Console.WriteLine("g_CWRectMinY[0]" + g_CWRectMinY);
                    }

                }
                else {
                    if ((boxList[0].size.Width * boxList[0].size.Height) <= (boxList[1].size.Width * boxList[1].size.Height))
                    {
                        //图像中x,y的坐标位置
                        CRectangleXMin = (double)(boxList[0].center.X);
                        CRectangleYMin = (double)(boxList[0].center.Y);
                        g_CWRectMinAngle = (double)(boxList[0].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        // Console.WriteLine("CRectangleX[0]:" + boxList[0].center.X);
                        //Console.WriteLine("CRectangleY[0]:" + boxList[0].center.Y);
                        //Console.WriteLine("CRectangleY[0]:" + boxList[0].angle);

                        //需要区分长短矩形
                        double[,] image_pix_rect_Min = new double[3, 1] { { CRectangleXMin }, { CRectangleYMin }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Min, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMinX = g_CtmpWRectX;
                        g_CWRectMinY = g_CtmpWRectY;
                        Console.WriteLine("g_CWRectMinX[0]" + g_CWRectMinX);
                        Console.WriteLine("g_CWRectMinY[0]" + g_CWRectMinY);

                        //图像中x,y的坐标位置
                        CRectangleXMax = (double)(boxList[1].center.X);
                        CRectangleYMax = (double)(boxList[1].center.Y);
                        g_CWRectMaxAngle = (double)(boxList[1].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        //Console.WriteLine("CRectangleXMax[1]:" + boxList[1].center.X);
                        // Console.WriteLine("CRectangleYMax[1]:" + boxList[1].center.Y);
                        //Console.WriteLine("g_CWRectMaxAngle[1]:" + boxList[1].angle);
                        //rectangle_location
                        double[,] image_pix_rect_Max = new double[3, 1] { { CRectangleXMax }, { CRectangleYMax }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Max, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMaxX = g_CtmpWRectX;
                        g_CWRectMaxY = g_CtmpWRectY;
                        // Console.WriteLine("g_AWRectX[1]:" + g_CWRectMaxX);
                        // Console.WriteLine("g_AWRectY[1]:" + g_CWRectMaxX);
                    }
                    else
                    {
                        //图像中x,y的坐标位置
                        CRectangleXMin = (double)(boxList[1].center.X);
                        CRectangleYMin = (double)(boxList[1].center.Y);
                        g_CWRectMinAngle = (double)(boxList[1].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        // Console.WriteLine("CRectangleX[1]:" + boxList[1].center.X);
                        //Console.WriteLine("CRectangleY[1]:" + boxList[1].center.Y);
                        // Console.WriteLine("CRectangleY[1]:" + boxList[1].angle);

                        //需要区分长短矩形
                        double[,] image_pix_rect_Min = new double[3, 1] { { CRectangleXMin }, { CRectangleYMin }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Min, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMinX = g_CtmpWRectX;
                        g_CWRectMinY = g_CtmpWRectY;
                        // Console.WriteLine("g_CWRectMinX[1]" + g_CWRectMinX);
                        //  Console.WriteLine("g_CWRectMinY[1]" + g_CWRectMinY);

                        //图像中x,y的坐标位置
                        CRectangleXMax = (double)(boxList[0].center.X);
                        CRectangleYMax = (double)(boxList[0].center.Y);
                        g_CWRectMaxAngle = (double)(boxList[0].angle);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        // Console.WriteLine("CRectangleXMax[0]:" + boxList[0].center.X);
                        // Console.WriteLine("CRectangleYMax[0]:" + boxList[0].center.Y);
                        // Console.WriteLine("g_CWRectMaxAngle[0]:" + boxList[0].angle);
                        //rectangle_location
                        double[,] image_pix_rect_Max = new double[3, 1] { { CRectangleXMax }, { CRectangleYMax }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect_Max, ref world_cor);

                        g_CtmpWRectX = (world_cor[0, 0] / s) * 1000;
                        g_CtmpWRectY = (world_cor[1, 0] / s) * 1000;

                        g_CWRectMaxX = g_CtmpWRectX;
                        g_CWRectMaxY = g_CtmpWRectY;
                        // Console.WriteLine("g_AWRectX[0]:" + g_CWRectMaxX);
                        //Console.WriteLine("g_AWRectY[0]:" + g_CWRectMaxX);
                    }
                }
                
            }
            return 20;
     
            #endregion
            #endregion
        }

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

        #region /***接收来自Modbus端值：若为5则表示需要抓取物件，若为4则表示需要松开物件***/
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
        private void SendXY2ModBus(double WorldX, double WorldY, double WorldZ ,double Flag)
        {
            WorldX = WorldX * 1000;
            Int32 Int32WorldX = (Int32)WorldX;

            WorldY = WorldY * 1000;
            Int32 Int32WorldY = (Int32)WorldY;

            WorldZ = WorldZ * 1000;
            Int32 Int32WorldZ = (Int32)WorldZ;

            Flag = Flag * 1000;
            Int32 FlagDone = (Int32)Flag;

            byte[] a = BitConverter.GetBytes(Int32WorldX);
            a = LittleEncodingFloat(a);

            byte[] b = BitConverter.GetBytes(Int32WorldY);
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
            return 44;
        }
        //抓手最大速度抓取，成功返回10
        private int RS232_Grabing()
        {
            txtShow_Recieved.Clear();
            txtShow_Recieved.AppendText("Grabing");
            txtShow_Recieved.ScrollToCaret();

            Console.WriteLine("<<Grabing Sending>>");
            ChangeGrabArray(textSpeed.Text, textPower.Text);
            return 55;
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
            //TODO:注释掉了下面4行
            //myFactory.CameraList[1].StartImageAcquisition(true, 5, pictureBox_B.Handle);
            //myFactory.CameraList[1].SaveNextFrame(ImagePath_B);
            //ImageProcess_B(ImagePath_B);
            //ImageProcess_C(ImagePath_C);
            //double a = 22.2223;
            //double b = 223.1111F;


            ////float b = 111.22F;
            ////int b = 12;
            //SendXY2ModBus(a, b);


            //g_ModbusFlagResult = RecieveFlagFromModbus();
            //Console.WriteLine("ModbusFlagResult:{0}", g_ModbusFlagResult);
        }

        //整个流程的启动入口
        private void btnImageProcess_Click(object sender, EventArgs e)
        {
            RS232_Releasing();
            //returnA：10则表示检测到了图形；11没检测到形状
            int returnA = 0;
            int returnC = 0;
            int returnB = 0;

            int PCGrabDone_A = 0;
            int PCReleaseDone_C = 0;
            //MechineFlag_A&&B&&C:5表示需要抓取物体；4表示需要放物体；13表示B相机识别到了物体
            int MechineFlag_A = 0;
            int MechineFlag_B = 0;

            int WaitB2C = 0;
            int CDown = 0;


            //0.处理A相机中的图像，返回10则表示成功，返回11则表示失败
            do
            {
                Console.WriteLine("######### 1 ##########");
                returnA = ImageProcess_A(ImagePath_A);
                Console.WriteLine("处理A相机中的图像........");
            } while (returnA != 10);
           
            //1.判断A相机得到的是长矩形短矩形还是圆形，并将A相机的返回值给Modbus
            //此时的returnA ==10的
            if (g_isACircle == 1)
            {
                Console.WriteLine("######### 2 ##########");
                //如果是圆形，设置抓手的高度
                double CircleHeight = -170.264;
                SendXY2ModBus(g_AWCircleX, g_AWCircleY, CircleHeight, returnA);
                Console.WriteLine("A相机中最左侧的是：圆形！传送坐标......");
                //g_isACircle = 0;
            }
            else if(g_isARectMax ==1) {
                Console.WriteLine("######### 3 ##########");
                double RectMaxHeight = -153.697;
                SendXY2ModBus(g_AWRectX, g_AWRectY,RectMaxHeight, returnA);
                Console.WriteLine("A相机中最左侧的是：长矩形！传送坐标......");
                //g_isARectMax = 0;
            }
            else{
                Console.WriteLine("######### 4 ##########");
                double RectMinHeight = -132.202;
                SendXY2ModBus(g_AWRectX, g_AWRectY, RectMinHeight, returnA);
                Console.WriteLine("A相机中最左侧的是：短矩形！传送坐标......");
                //g_isARectMin = 0;
            }

            //2.等待机械手的返回值，是否抓取
            //此时应该是机械手去A相机的坐标值，然后下降，给一个返回值给PC，告诉PC要启动抓手了
            //MechineGrab = 5表示需要抓取了；55表示抓取成功了
            do
            {
                Console.WriteLine("######### 5 ##########");
                MechineFlag_A = RecieveFlagFromModbus();
                Console.WriteLine("到达A相机中的指定形状坐标，等待抓取指令......");
            } while (MechineFlag_A != 5);

            //是否抓取成功
            do
            {
                Console.WriteLine("######### 6 ##########");
                PCGrabDone_A = RS232_Grabing();
                //将抓取成功后的值发给机械手,55表示抓取成功
                SendXY2ModBus(0, 0, 0,PCGrabDone_A);
                Console.WriteLine("我在抓取工件咯！抓取成功!!!正在赶往B相机处.......");
            } while (PCGrabDone_A != 55);
           
            //机械手应该去B相机处
            //接收机械手端的到达B相机处的指令
            //3.判断B相机是否采集到了物体，13表示机械手到达了B相机处
            do
            {
                Console.WriteLine("######### 7 ##########");
                MechineFlag_B = RecieveFlagFromModbus();
                Console.WriteLine("到达B相机处!!!");
            } while (MechineFlag_B != 13);

            //B相机处理图像
            /*
                //g_BFlag = 0;初始值
                //g_BFlag = 1：检测到了圆形
                //g_BFlag = 2：检测到了长矩形
                //g_BFlag = 3：检测到了短矩形
             */
            #region 处理B相机5次选出识别出来的结果
            //int oneCount = 0;
            //int twoCount = 0;
            //int threeCount = 0;
            //List<int> ResultBCount = new List<int>();
            //for (int i = 0; i < 5; i++)
            //{
            //    myFactory.CameraList[1].SaveNextFrame(ImagePath_B);
            //    returnB = ImageProcess_B(ImagePath_B);
            //    ResultBCount.Add(g_BFlag);
            //}
            //foreach (int j in ResultBCount)
            //{
            //    //Console.WriteLine("ResultBCount:" + j);
            //    if (j == 1)
            //    {
            //        oneCount += 1;
            //    }
            //    else if (j == 2)
            //    {
            //        twoCount += 1;
            //    }
            //    else
            //    {
            //        threeCount += 1;
            //    }
            //}
            ////排序
            //if (oneCount >= twoCount)
            //{
            //    if (oneCount >= threeCount)
            //    {
            //        g_BFlag = 1;
            //        Console.WriteLine("1最多！");
            //    }
            //    else
            //    {//2大
            //        if (twoCount >= threeCount)
            //        {
            //            g_BFlag = 2;
            //            Console.WriteLine("2最多！");
            //        }
            //        else
            //        {
            //            g_BFlag = 3;
            //            Console.WriteLine("3最多！");
            //        }
            //    }
            //}
            #endregion
            //TODO:这里还需要想想B相机中的返回值
            //循环几次找到最优解
            do
            {
                myFactory.CameraList[1].SaveNextFrame(ImagePath_B);
                returnB = ImageProcess_B(ImagePath_B);
                Console.WriteLine("######### 8 ##########");
                Console.WriteLine("returnB的值是：" + returnB);
                SendXY2ModBus(0, 0, 0, returnB);
            } while (returnB != 10);
 
            //4.处理C相机中的图像，返回20则表示成功，返回21则表示失败 
            do
            {
                Console.WriteLine("######### 9 ##########");
                
                returnC = ImageProcess_C(ImagePath_C);
                SendXY2ModBus(0, 0, 0,returnC);
                Console.WriteLine("正在处理C相机......");
            } while (returnC != 20);

            //5.在机械手端先判断returnC的值
            //这里判断B相机采集到的形状是什么
            if(g_isARectMin == 1)//B相机中检测到的是短矩形
            {
               do{
                   Console.WriteLine("g_isARectMin:" + g_isARectMin);
                   Console.WriteLine("######### 10 ##########");
                 SendXY2ModBus(g_CWRectMinX,g_CWRectMinY,-22.628,g_CWRectMinAngle);
                 Console.WriteLine("C相机中的处理结果，我在给机械手传短矩形的值!");
                 WaitB2C = RecieveFlagFromModbus();
               }while(WaitB2C != 10);
               
            }
            else if(g_isACircle == 1)//B相机检测到了圆形
            {
                do{
                    Console.WriteLine("g_isACircle:" + g_isACircle);
                    Console.WriteLine("######### 11 ##########");
                    Console.WriteLine("C相机中的处理结果，我在给机械手传圆形的值!");
                    SendXY2ModBus(g_CWCircleX, g_CWCircleY, -22.628, returnC);
                    Console.WriteLine("g_CWCircleX:" + g_CWCircleX);
                    Console.WriteLine("g_CWCircleY:" + g_CWCircleY);

                    WaitB2C = RecieveFlagFromModbus();
               }while(WaitB2C != 10);
            }
            else if(g_isARectMax == 1)//B相机检测到了长矩形
            {
                 do{
                     Console.WriteLine("g_isARectMax:" + g_isARectMax);
                     Console.WriteLine("######### 12 ##########");
                    SendXY2ModBus(g_CWRectMaxX, g_CWRectMaxY, -22.628, g_CWRectMaxAngle);
                    Console.WriteLine("C相机中的处理结果，我在给机械手传长矩形的值!");
                   WaitB2C = RecieveFlagFromModbus();
               }while(WaitB2C != 10);
               
            }
            else
            {
                 do{
                     Console.WriteLine("######### 13 ##########");
                     Console.WriteLine("C相机中的处理结果，B相机什么都没检测到!");
                     //SendXY2ModBus(0, 0,0, g_BFlag);//返回值给机械手，告诉它回到B相机处 
                     WaitB2C = RecieveFlagFromModbus();
                }while(WaitB2C != 10);
               
            }

            //6.应该移动手臂到指定地点，下降
            //等待Modbus传来的释放指令
            do
            {
                Console.WriteLine("######### 14 ##########");
                CDown = RecieveFlagFromModbus();
            } while (CDown != 4);

            do
            {
                Console.WriteLine("######### 15 ##########");
                PCReleaseDone_C = RS232_Releasing();
                //SendXY2ModBus(0, 0, 0, PCReleaseDone_C);
                Console.WriteLine("我要爆发啦！我要释放自己呀！！！！");
            } while (PCReleaseDone_C != 44);

            //6.释放完毕之后应该怎么做
            //SendXY2ModBus(g_AWRectX, g_AWRectY, PCReleaseDone_C, PCReleaseDone_C);
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
