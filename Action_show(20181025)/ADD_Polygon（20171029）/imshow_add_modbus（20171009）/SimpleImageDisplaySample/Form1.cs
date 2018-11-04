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
//导入Emgu.CV
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
//导入 System.Threading;
using System.Runtime.InteropServices; // 后面的[DllImport("kernel32")]
using System.Drawing.Imaging;
using System.Diagnostics;
//导入自己的Matrix矩阵模块
using matrix_test;
//导入JAI
using Jai_FactoryDotNET;
//导入flycapture
using FlyCapture2Managed;
using FlyCapture2Managed.Gui;
//分割字符串
using System.Text.RegularExpressions;

namespace SimpleImageDisplaySample
{
    public partial class Form1 : Form,ILog,IDisposable
    {
        #region 声明全局变量
            //ImagePath
        string ImagePath = ".\\saveimg.bmp";
        string ImagePath1 = ".\\saveimgC.bmp";

        //TODO:这里B相机的图像还没添加进去
        string ImagePath2 = ".\\saveimgB.bmp";

            #region 区分长方形、正方形和圆形的世界坐标点在A相机和C相机
            double world_X = 0;
            double world_Y = 0;
            double world_X_c = 0;
            double world_Y_c = 0;
            
            double world_X_circle = 0;
            double world_Y_circle = 0; 
            double world_X_circle_c = 0; 
            double world_Y_circle_c =0; 

            //TODO:将选择好需要传给Modbus的值public_x&&public_y
            double public_X = 0;
            double public_Y = 0;

            #endregion
            //TODO:这个标志位还有待考虑
            #region /*设置区分长方形，正方形和圆形的标志位*/
            int Flag = 0;

            //TODO:Flag_B标志位表示B相机是否检测到了物体的形状，
            //为1则表示检测到了长方形，
            //2则表示检测到了正方形，
            //3则表示检测到了圆形
            int Flag_B  = 0;
    
            int AreaCircle;

            #endregion 

            #region /*ini文件变量声明*/
        //系统文件用来读取ini文件
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
        
            #region /*定标变量声明*/
        //图像坐标与世界坐标初始化
        public static double fc1, fc2, cc1, cc2, R11, R12, R13, R21, R22, R23, T1, T2, T3, s;

        //TODO:添加的C相机的定标参数
        public static double fc1_c, fc2_c, cc1_c, cc2_c, R11_c, R12_c, R13_c, R21_c, R22_c, R23_c, T1_c, T2_c, T3_c, s_c;
    
        double[,] c = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] c_ = new double[3, 3] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
        double[,] world_cor = new double[3, 1] { { 0 }, { 0 }, { 1 } };

        #endregion
        
            #region /*Fly相机变量声明*/
        //相机的初始化程序
        private FlyCapture2Managed.Gui.CameraControlDialog m_camCtlDlg;
        private ManagedCameraBase m_camera = null;
        private ManagedImage m_rawImage;
        private ManagedImage m_processedImage;
        private bool m_grabImages;
        private AutoResetEvent m_grabThreadExited;
        private BackgroundWorker m_grabThread;
        #endregion
        
            #region /*JAI相机和图像处理变量声明*/
        //图像处理--圆
        public CircleF circle;
        public CircleF[] circles;
        //图像处理--矩形
        public MCvBox2D box1;
        public List<MCvBox2D> boxList;
    
        //JAI相机
        CFactory myFactory = new CFactory();
         
        // Opened camera obejct
        //CCamera myCamera1;
        //CCamera myCamera2;
        //Jai_FactoryWrapper.EFactoryError error;
        

        /***********Modbus--Tcp*********/
        private ModBusWrapper Wrapper = null;
        #endregion 
        #endregion
        
        public Form1 ()
        {   
            #region 初始化主窗口
            InitializeComponent(); //窗口的初始化：比如拖一些框

            /*************Fly__init***************/
            #region /*Fly相机初始化*/
            m_rawImage = new ManagedImage();
            m_processedImage = new ManagedImage();
            m_camCtlDlg = new CameraControlDialog();
            m_grabThreadExited = new AutoResetEvent(false);  //非终止
            #endregion
           
            /*************JAI__init***************/
            #region /*JAI相机初始化*/
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
            #region Fly相机__hide()
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

                    m_camera.StartCapture();

                    m_grabImages = true;

                    //Function call
                    StartGrabLoop();
              }
                catch (FC2Exception ex)
                {
                    Debug.WriteLine("Failed to load form successfully: " + ex.Message);
                    Environment.ExitCode = -1;
                    Application.Exit();
                    return;
                }

                toolStripButtonStart.Enabled = true;
                toolStripButtonStop.Enabled = true;
        }
            else
            {
                Environment.ExitCode = -1;
                Application.Exit();
                return;
            }

            //TODO:Show()函数试一下什么效果
            //Show();
            #endregion
            #region A相机_定标参数的初始化
            //加载标定参数
            StringBuilder str = new StringBuilder(100);
            //calib2.ini:表示的是A相机定标之后所保存的定标参数
            GetPrivateProfileString("标定", "fc1", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                fc1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "fc2", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                fc2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "cc1", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                cc1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "cc2", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                cc2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R11", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R11 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R12", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R12 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R13", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R13 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R21", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R21 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R22", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R22 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "R23", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                R23 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "T1", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                T1 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "T2", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                T2 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "T3", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                T3 = Convert.ToDouble(str.ToString());
            GetPrivateProfileString("标定", "s", "", str, 100, Application.StartupPath + "/calib2.ini");
            if (str.ToString() != "")
                s = Convert.ToDouble(str.ToString());
            #endregion
            #region C相机_定标参数的初始化
            //加载标定参数
            StringBuilder str_c = new StringBuilder(100);
            GetPrivateProfileString("标定", "fc1", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                fc1_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "fc2", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                fc2_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "cc1", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                cc1_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "cc2", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                cc2_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R11", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                R11_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R12", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                R12_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R13", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                R13_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R21", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                R21_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R22", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                R22_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "R23", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                R23_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "T1", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                T1_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "T2", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                T2_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "T3", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                T3_c = Convert.ToDouble(str_c.ToString());
            GetPrivateProfileString("标定", "s", "", str_c, 100, Application.StartupPath + "/str_C_C.ini");
            if (str_c.ToString() != "")
                s_c = Convert.ToDouble(str_c.ToString());
            #endregion
        }
       
        /**************FLY-B相机**************/
        #region FLY-B相机
        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
            pictureBox_B.Image = m_processedImage.bitmap;
            pictureBox_B.Invalidate();     
        }

        //Form1_FormClosing 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            #region FLY相机关闭
            try
            {
                //修改—1.close()
                //toolStripButtonStop_Click_1(sender, e);
                StopButton_Click( sender,e);
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
            #endregion
        }
        
       //FLY-StartGrabLoop
        private void StartGrabLoop()
        {
        #region FLY相机开始拍摄
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
        #region FLY相机连续拍摄
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
                
                worker.ReportProgress(0);    //进度流程  
            }
            m_grabThreadExited.Set();
        #endregion
        }
        
        //FLY-begin
        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
        #region FLY相机启动
            m_camera.StartCapture();

            m_grabImages = true;

            StartGrabLoop();

            toolStripButtonStart.Enabled = false;
            toolStripButtonStop.Enabled = true;
        #endregion
        }

        //FLY-B_stop
        private void toolStripButtonStop_Click(object sender, EventArgs e)
        {
            #region FLY相机关闭
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
        #region Fly-相机隐藏控制设置
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
   
        //弹出定标的对话框   
        private void toolStripMenuItem1_Click_A(object sender, EventArgs e)
        {
            #region 标定框出来
            calib cab = new calib();
            cab.Show();
           
            #endregion
        }

        private void toolStripMenuItem_C_Click(object sender, EventArgs e)
        {
            calib_C cab_C = new calib_C();
            cab_C.Show();
        }
     
        #endregion

        /************AI-A&&C相机**************/
        #region JAI-A&&C相机,开始，结束和搜索
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
                //myCamera1 = myFactory.CameraList[0];
                //myCamera2 = myFactory.CameraList[1];
            }
            else
            {
                MessageBox.Show("No Cameras Found!");
            }
        }
        //EgiE相机开始停止
        //TODO:按下开始键打开JAI相机开始处理图像，主要函数在这里    
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (myFactory.CameraList[0] != null)
            {
                myFactory.CameraList[0].StartImageAcquisition(true, 5, pictureBox_A.Handle);

            }
            if (myFactory.CameraList[1] != null)
            {
                myFactory.CameraList[1].StartImageAcquisition(true, 5, pictureBox_C.Handle);
               //myFactory.CameraList[1].SaveNextFrame(".\\saveimgC_C" + ".bmp");

            }
            
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            SearchButton.Enabled = true;
            
            //TODO:直接设置死循环让其不停地找图像并进行坐标的传值：
            //TODO:如何判断C相机

            while(true)
            {
                myFactory.CameraList[0].SaveNextFrame(ImagePath);
                myFactory.CameraList[1].SaveNextFrame(ImagePath1);
                ImageProcess_A(ImagePath);

                SendDataToModBus(public_X,public_Y);

                //B相机Do something 

                ImageProcess_B(ImagePath2);


                //TODO:Flag_B的值是否变为1，若为1说明B相机检测到了物体
                if(Flag_B == 1)
                {
                    //TODO:如何判断长方形、正方形和圆形
                    if(Flag == 1)
                    {
                        ImageProcess_C(ImagePath1);
                    }
                }
                else continue;
            }
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < myFactory.CameraList.Count; i++)
            {
                myFactory.CameraList[i].StopImageAcquisition();
            }

            StartButton.Enabled = true;
            StopButton.Enabled = false;
            SearchButton.Enabled = true;
        }        
        #endregion
        
    
        /***将世界坐标值传到机械手端*******/
        private void SendDataToModBus(double WorldX,double WorldY)
        {
            Int32 x = (Int32)(WorldX);
            Int32 y = (Int32)(WorldY);
            Console.WriteLine("传给modbus_x:" + x);
            Console.WriteLine("传给modbus_y:" + y);
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
            this.Wrapper.Send(z);
        }
        
        /*******关于图像处理程序**********/
        //图像处理程序模块
        #region 相机A的处理过程
        private void ImageProcess_A(string ImagePath)
        {
            int point_X = 0;
            int point_Y = 0;

            int point_X_circle = 0;
            int point_Y_circle = 0;
            
            /*canny算子处理图像*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(ImagePath);
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();
            double cannyThreshold =200.0;
            double circleAccumulatorThreshold =50.0;
            
            #region Find circles
            /*检测圆形*/
            circles = grayImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                1.5, //Resolution of the accumulator used to detect centers of the circles
                grayImage.Width, //min distance 
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion
            
            #region Canny and edge detection
            double cannyThresholdLinking = 100.0;
            Image<Gray, Byte> cannyEdges = grayImage.Canny(cannyThreshold, cannyThresholdLinking);
            //TODO:将LineSegment2D[] lines去掉看看效果
            // LineSegment2D[] lines = cannyEdges.HoughLinesBinary(
            //     1, //Distance resolution in pixel-related units
            //     Math.PI / 90.0, //Angle resolution measured in radians.
            //     20, //threshold
            //     30, //min Line width
            //     10 //gap between lines
            //     )[0]; //Get the lines from the first channel
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

                    if (currentContour.Area >400) //only consider contours with area greater than 4300
                    {
                        if(currentContour.Total == 4)  //The contour has 4 vertices.
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
                        else
                        {
                            Console.WriteLine("The currentContour is more than 5!!!");
                        }
                    }
                }
            #endregion
           
            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            //draw the rectangles
            Console.WriteLine("矩形的个数：" + boxList.Count());
            Console.WriteLine("圆形的个数：" + circles.Count());
            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.DarkOrange), 2);  
            }
            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circles[0].Area);
                if (AreaCircle >= 3000 && AreaCircle <= 4600)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
                }
            }
            #endregion
            
            /*显示结果，在A相机的图像中显示出来*/
            pictureBox_A_processed.Image = triangleRectangleImage.ToBitmap();
            pictureBox_Processing.Image = cannyEdges.ToBitmap();

            double[,] a = new double[3, 3] { { fc1, 0, cc1 }, { 0, fc2, cc2 }, { 0, 0, 1 } };
            double[,] b = new double[3, 3] { { R11, R21, T1 }, { R12, R22, T2 }, { R13, R23, T3 } };
             
            //TODO:Flag == 0：表示的是长方形和正方形的坐标位置，但是还没有区分出长方形和正方形（可能面积的大小来区分）
            if (Flag == 1)
            { 
                boxList.Sort();
                //图像中x,y的坐标位置
                point_X = (Int32)(boxList[0].center.X);
                point_Y = (Int32)(boxList[0].center.Y);

                //rectangle_location
                double[,] image_pix = new double[3, 1] { { point_X }, { point_Y }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X = (world_cor[0, 0] / s) * 1000;
                world_Y = (world_cor[1, 0] / s) * 1000;

                Flag = 1;
           }
           //Flag == 1 表示的是圆形的坐标位置
            else if (Flag == 0)
            {
                /*输出圆的圆心*/

                point_X_circle = (Int32)(circles[0].Center.X);
                point_Y_circle = (Int32)(circles[0].Center.Y);

                //circle_location
                double[,] image_pix = new double[3, 1] { { point_X_circle }, { point_Y_circle }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X_circle = (world_cor[0, 0] / s) * 1000;
                world_Y_circle = (world_cor[1, 0] / s) * 1000;
              
                Flag = 1;
            }
            else
            {
                Console.WriteLine("the Flag U set is wrong!!!");
                Flag = 0;
            }
        }
        #endregion
        
        #region 相机C的处理过程
        private void ImageProcess_C(string ImagePath)
        {
            int point_X = 0;
            int point_Y = 0;

            int point_X_circle = 0;
            int point_Y_circle = 0;

            /*canny算子处理图像*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(ImagePath);
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();
            double cannyThreshold =210.0;
            double circleAccumulatorThreshold = 50.0;
            
            #region Find circles
            /*检测圆形*/
            circles = grayImage.HoughCircles(
                new Gray(cannyThreshold),
                new Gray(circleAccumulatorThreshold),
                2.0, //Resolution of the accumulator used to detect centers of the circles
                grayImage.Width, //min distance 
                20, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion
            
            #region Canny and edge detection
            double cannyThresholdLinking = 100.0;
            Image<Gray, Byte> cannyEdges = grayImage.Canny(cannyThreshold, cannyThresholdLinking);
            //TODO:将LineSegment2D[] lines去掉看看效果
            // LineSegment2D[] lines = cannyEdges.HoughLinesBinary(
            //     1, //Distance resolution in pixel-related units
            //     Math.PI / 90.0, //Angle resolution measured in radians.
            //     20, //threshold
            //     30, //min Line width
            //     10 //gap between lines
            //     )[0]; //Get the lines from the first channel
            #endregion
            
            #region search rectangles
            //存放矩形的形状
            List<MCvBox2D> boxList = new List<MCvBox2D>(); //a box is a rotated rectangle
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
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.03, storage);//注意这里的The desired approximation accuracy为0.04

                    if (currentContour.Area >400) //only consider contours with area greater than 4300
                    {
                        if(currentContour.Total == 4)  //The contour has 4 vertices.
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
                        else
                        {
                            Console.WriteLine("The currentContour is more than 5!!!");
                        }
                    }
                }
            #endregion
            
            #region draw rectangles and circles
            Image<Bgr, Byte> triangleRectangleImage = new Image<Bgr, Byte>(ImagePath);
            //draw the rectangles
            //triangleRectangleImage.Save(".\\11.bmp");
          
            foreach (MCvBox2D box1 in boxList)
            {
                triangleRectangleImage.Draw(box1, new Bgr(Color.DarkOrange), 2);
             
                //图像中x,y的坐标位置
                point_X = (Int32)(boxList[0].center.X);
                point_Y = (Int32)(boxList[0].center.Y);
            }
            //draw the circles
            foreach (CircleF circle in circles)
            {
                AreaCircle = (Int32)(circles[0].Area);
                if (AreaCircle >= 4000 && AreaCircle <= 6500)
                {
                    triangleRectangleImage.Draw(circle, new Bgr(Color.Red), 2);
                    /*输出圆的圆心*/
                    Console.WriteLine("圆的面积："+ circle.Area);
                    point_X_circle = (Int32)(circles[0].Center.X);
                    point_Y_circle = (Int32)(circles[0].Center.Y);
                }
            }
           // triangleRectangleImage.Save(".\\11.bmp");
           #endregion
           
            //TODO:A相机和C相机之间的差别就在显示框的位置的不同
            /*显示结果，在C相机的图像中显示出来*/
            pictureBox_C_processed.Image = triangleRectangleImage.ToBitmap();

            double[,] a = new double[3, 3] { { fc1_c, 0, cc1_c }, { 0, fc2_c, cc2_c }, { 0, 0, 1 } };
            double[,] b = new double[3, 3] { { R11_c, R21_c, T1_c }, { R12_c, R22_c, T2_c }, { R13_c, R23_c, T3_c } };
             
            if (Flag_B == 1) //1:长方形
            { 
                //rectangle_location
                double[,] image_pix = new double[3, 1] { { point_X }, { point_Y }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X_c = (world_cor[0, 0] / s) * 1000;
                world_Y_c = (world_cor[1, 0] / s) * 1000;

                Flag = 1;
            }
            else if (Flag_B == 3) //3:圆形
            {
                //circle_location
                double[,] image_pix = new double[3, 1] { { point_X_circle }, { point_Y_circle }, { 1 } };

                Matrix.MatrixMultiply(a, b, ref c);
                Matrix.MatrixOpp(c, ref c_);
                Matrix.MatrixMultiply(c_, image_pix, ref world_cor);

                world_X_circle_c = (world_cor[0, 0] / s) * 1000;
                world_Y_circle_c = (world_cor[1, 0] / s) * 1000;

                public_X = world_X_circle_c;
                public_Y = world_Y_circle_c;

                Flag = 0;
            }
            else if(Flag_B == 2) //2:正方形
            {
                //面积小的那一个
            }
        }
        #endregion
    
        //TODO:B相机的处理程序还没写，这里还有打光的技术，在论文中可以添加
        #region 相机B的处理过程
        private void ImageProcess_B(string ImagePath)
        {
            //Do something...

        }

        #endregion
        /**********Modbus程序***********/
        #region ModBus程序
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
        #region 小端封装
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
        #region ILog 成员
        public void Write(string log)
        {
            this.tbxHistory.AppendText(log + Environment.NewLine);
            this.tbxHistory.Select(this.tbxHistory.TextLength, 0);
            this.tbxHistory.ScrollToCaret();
        }
        #endregion
        #region 释放Modbus资源
        private void TestModBus_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Wrapper.Dispose();
        }
        #endregion

        #endregion
     
#region /*注释补充*/
        /**************************************************/
#region 注释--定时器来控制整个程序的图像处理
        //TODO:可能这里需要更换成定时器模式来完成
        /********线程控制-------main()-----图像处理*******/
        // private void timer2_Tick_1(object sender, EventArgs e)
        // {
        //     #region 5000毫秒进入一次图像处理
            
        //     //Console.WriteLine("image processing!!!");
        //     myFactory.CameraList[0].SaveNextFrame(ImagePath);
        //     //TODO:增加一个相机的图像处理操作
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
        //     //    //TODO:do something 
        //     // }
        //     // else
        //     // {
        //     //     Flag_t = 1;
        //     // }

        //     #endregion
        // }
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
    }  
}
#endregion  

//TODO:测试一下FLY相机的隐藏功能
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