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
//导入自己的矩阵模块
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
            #region /*ini文件变量声明*/
        //系统文件用来读取ini文件
        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        #endregion
        
            #region /*定标变量声明*/
        //图像坐标与世界坐标初始化
        public static double fc1, fc2, cc1, cc2, R11, R12, R13, R21, R22, R23, T1, T2, T3, s;
      
        //设置一个Flag来区分矩形和圆形
        int Flag = 0;
        int Flag_t = 1;
  
        int AreaCircle;

        int point_X, point_Y;
        int point_X_circle, point_Y_circle;

        double world_X, world_Y;
        //TODO 待删减
        double world_X_circle, world_Y_circle; 

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
        CCamera myCamera1;
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

        #region /*B相机__在相框3中显示*/
        private void UpdateUI(object sender, ProgressChangedEventArgs e)
        {
           
            pictureBox3.Image = m_processedImage.bitmap;
            pictureBox3.Invalidate();     
        }
        #endregion
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

                toolStripButtonStart.Enabled = false;
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
            #region 定标参数的初始化
            //加载标定参数
            StringBuilder str = new StringBuilder(100);
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
        }
           
        /**************FLY-B相机*******************/
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
        private void toolStripButtonStop_Click_1(object sender, EventArgs e)
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
        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            #region 标定框出来
            calib cab = new calib();
            cab.Show();
            #endregion
        }

        /**********EgiE相机程序***********/
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
        //EgiE相机开始停止    
        private void StartButton_Click(object sender, EventArgs e)
        {
            //#region 相机开始，停止按键
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
            #region Stop_0.JAI相机停止按键
            for (int i = 0; i < myFactory.CameraList.Count; i++)
            {
                myFactory.CameraList[i].StopImageAcquisition();
            }

            StartButton.Enabled = true;
            StopButton.Enabled = false;
            SearchButton.Enabled = true;
            #endregion
 
        }        
        /*******关于图像处理程序**********/
        //按一下处理图像并显示
        //public void button_circle_Click_1(object sender, EventArgs e)
        //{
        //    #region 按键图像处理
        //    Console.WriteLine("1");
        //    myFactory.CameraList[0].SaveNextFrame(".\\saveimg" + ".bmp");
        //    ImageProcess();
        //    #endregion
        //}
        /********直接线程控制图像处理*******/
        private void timer2_Tick_1(object sender, EventArgs e)
        {
            #region 5000毫秒进入一次图像处理
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

                // System.Console.WriteLine( x);
                //System.Console.WriteLine( y);
                this.Wrapper.Send(z);

            }
            else if (Flag_t == 1)
            {
                Int32 x = (Int32)(world_X_circle);
                Int32 y = (Int32)(world_Y_circle);
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
        
        //图像处理程序模块
        #region 图像处理模块
        private void ImageProcess()
        {
            //*canny*/
            Image<Bgr, Byte> image1 = new Image<Bgr, Byte>(".\\saveimg" + ".bmp");
            Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();
            double cannyThreshold =200.0;
            double circleAccumulatorThreshold = 55;
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
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.03, storage);//注意这里的The desired approximation accuracy为0.04

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
                //图像中x,y的坐标位置
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
                    /*输出圆的圆心*/
                    //Console.WriteLine("Center Of Circle:" + circle.Center);
                    point_X_circle = (Int32)(circles[0].Center.X);
                    point_Y_circle = (Int32)(circles[0].Center.Y);
                }

                //Flag = 1;

                //Console.WriteLine("circle_area:" + AreaCircle);
              
                //Console.WriteLine("circle-x:" + circles[0].Center.X);
                //Console.WriteLine("circle-y:" + circles[0].Center.Y); //circle[0]或者circle[1]是指找到的圆中的第一个和第二个

            }
            #endregion
            //显示结果
            pictureBox_circle.Image = triangleRectangleImage.ToBitmap();
            //point_X = (Int32)(box1.center.X);   //圆的x坐标和圆的y坐标 SimpleImageDisplaySample.Form1.box1”冲突	C:\Users\Administrator\Desktop\zsx__PC\20180108_PC_xiamen\5ADD_calib(20180108)\ADD_calib(20180108)_changing\ADD_Polygon（20171029）\imshow_add_modbus（20171009）\SimpleImageDisplaySample\Form1.cs	716	31	SimpleImageDisplaySample
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
                //point_X = (Int32)(circles[0].Center.X);   //圆的x坐标和圆的y坐标
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
                //point_X = (Int32)(circles[0].Center.X);   //圆的x坐标和圆的y坐标
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

        /**********Modbus程序***********/
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
        
        /******相机放大缩小*********/
        #region zoom放大缩小
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


#region /*注释补充*/
/**************************************************/
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