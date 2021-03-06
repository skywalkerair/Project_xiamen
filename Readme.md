

## 课题进展

**20181226：**

1. 完成了RS232串口通讯的测试代码，也将代码融合到了厦门科技局的总体代码中（C#版）

2. 待机械爪到来之后，做好测试，测量好每个工件的抓取大小，设定好相应的参数，主要是力的大小

设置好相应的逻辑，比如给一个信号就去抓，这个要设计好。

***接下来：***

1. 看modbus的程序，写成双工通讯

***20181227：***

1. 增加两个按键=>设置机械手的力控值和速度

~~~c#
byte[] -> string:
public static string ByteArrayToString(byte[] a)
{
    string hex = BitConverter.ToString(a);
    return hex.Peplace("-","");
}
~~~

https://blog.csdn.net/andrewniu/article/details/72469023





***20181231：*** On the end of the year of 2018,there are too many things I have to do.Don't be afraid of them,just do it.

### 机械爪程序总结：

1. SerialPort sp = new SerialPort();使用C#的串口通讯库

2. 设置串口的四个要素：串口名，波特率，数据位，停止位

3. 指令帧命令：

   ~~~C#
   byte[] SReadStatus = new byte[]{0xEB,0x90,0x01,0x01,0x14,0x16};//读取夹爪的状态，可得到当前开口的大小，夹持力和当前阈值；
   byte[] SSetData = new byte[] { 0xEB, 0x90, 0x01,0x04,0x10,0x32,0x64,0x00,0xAB };//设置夹爪的抓取数据     
   byte[] SRelease = new byte[] {  0xEB, 0x90, 0x01, 0x03, 0x11, 0xE8,0x03,  0x00 };//设置夹爪的松开速度，MAX = 1000(10) = 0x03E8(16);将高位放在后面，以byte[]的形式发送
   ~~~

   ~~~C#
   # 设置抓手的配置
   private void function1(string power,string speed)
   {
       //将string类型=>int 类型
       int IntPower = int.Parse(Power);
       //int类型 =>byte[]类型
       byte[] BytePower = BitConverter.GetBytes(IntPower);
   }
   
   
   //TODO:Notice:这里值得注意，加起来的总和需要使用高8位和低8位来表示
   所以这里用int ADD = 0;//四个字节
   byte[] ADD是四个字节的
   BitConverter.GetBytes（int）：自动将低字节放在前面，高字节放后面
   ~~~

   4. 优化了代码，提高了发送抓取指令的鲁棒性
   5. 将收到的返回消息解码显示，已解决

***20190102：***完成了抓手的RS232的串口通讯程序，手动控制

***20190105：(明天需要把改过的代码都测试一遍)***

1. 重新改Modbus的程序，随时可以按指令关掉(Done)
2. 将RS232的程度预留出接口，可以通过判断值来抓取物体(X)

完成了：

1. 完成了Modbus的发送和接受的双工通讯程序代码，将Int 改为了float类型的数据
2. 这里需要测试一下机械手端的Modbus传过来什么值
   这里设置一个Flag,当读到Modbus返回的数据之后，更换我需要传递的值
   这里需要测试一下，接收Modbus端的数据，并显示
3. 写了两小段测试代码

~~~C#
//TODO:这里需要测试一下机械手端的Modbus传过来什么值
        //TODO：这里设置一个Flag,当读到Modbus返回的数据之后，更换我需要传递的值
        //TODO:接收Modbus端的数据，并显示
        private void TestRecieveModbus()
        {
            
            byte[] NewRecieve = new byte[4];
            NewRecieve = this.Wrapper.Receive();

            for (int i = 0; i < NewRecieve.Length; i++)
            {
                Console.WriteLine("NewRecieve[{0}]:{1}", i, NewRecieve[i]);
            }
        }

        
        //TODO:测试代码=》向Modbus传值，接收值
        private void TestSendModBus(float x)
        {
            float WorldX = x;
            Console.WriteLine("传值："+WorldX);
            //int WorldY = y;

            byte[] a = BitConverter.GetBytes(WorldX);

            //byte[] b = BitConverter.GetBytes(WorldY);
            byte[] SendData = new byte[a.Length];
            Console.WriteLine("SendData.Length:"+SendData.Length);

            a.CopyTo(SendData,0);
            //b.CopyTo(SendData,a.Length);

            this.Wrapper.Send(SendData);
        }
~~~

***20190107：***

1. 论文完成到了第二章，准备定标实验
2. 测试所写的代码
3. 将机械手端的程序优化，不能让机械爪子撞到



***20190116：***

1. 测试Modbus的程序，接受和发送的数据是什么

问题：为什么单个发送一个数据时收不到

解决：（1）在ModBusTCPIPWrapper.cs文件中的public override void Send(byte[] data)函数中

values.AddRange(ValueHelper.Instance.GetBytes((short)(***2***)));

这里的2与机械臂中Modbus所要读取的寄存器有关，当读取的是DW时，寄存器需要是双数，

所以当传给程序函数是两个值的时候,这里应该是***(short)4***

（2）float 单精度浮点型是占4个字节

（3）Modbus中 

字 word、字节 byte、位bit

1 word = 2 byte

1 byte = 8 bit

Dword 双字节 = 2 word = 4 byte= 32 bit

（4）

32位编译器：

| int           | 4个字节 |
| ------------- | ------- |
| unsigned int  | 4个字节 |
| float         | 4个字节 |
| double        | 8个字节 |
| long          | 4个字节 |
| long long     | 8个字节 |
| unsigned long | 4个字节 |

64位编译器：

| char  | 1字节 |
| ----- | ----- |
| float | 4字节 |
|       |       |
|       |       |
|       |       |
|       |       |
|       |       |

***20190117:***

1. Send Modbus 的程序成功，还需要定标之后测试最后的坐标值，将float型

***20190118:***

1. Modbus双工通讯的程序完成了：

   问题：得不到从机械手modbus写入的数据

   解决：这里的设备地址要写对，0x02，这里我重新设置了一个ModbusID号来解决这个问题

   ​	sendData.Add(0x02);
   ​        //7:Unit Identifier:This field is used for intra-system routing purpose.

***Modbus的总结：***

1. ModbusID这个设备地址，需要设置成0x02

2. 当发送Modbus数据时，

   （1）将float类型的物体坐标X，Y==>Int32型数据

   （2）Int32==>byte类型

   ***（3） values.AddRange(ValueHelper.Instance.GetBytes((short)(4)));***

   ***//如果是传一个X,则这里的GetBytes((short)2)两个寄存器，因为机器手端的DW需要双数的寄存器来存放数据***

   （4）这里我在设置的是0x1000-0x1004传X，Y的值，0x1006传返回的Modbus的信号







***20190218：项目总框架的设定规范***

A：

**抓取：：取最左边**

1. 一个物体

2. 两个物体（一圆一矩形，两个矩形）

3. 三个物体（）

4. 什么都没有，11


/* ***
   * 返回10：A相机传值给modbus成功;
        返回11：A相机中既没有圆形也没有矩形

        ***/

**解决方案：**

最右边的物体先抓；比较中心点x轴的坐标值，取最小的那个坐标值。



B：

/* ***
   * 识别出形状和角度

     ***/

***存在的问题：***

1. 在B相机的矩形长短边的判断上需要根据实际的长度再判断
2. 查看在B相机中矩形和圆形的面积和边长，设置长矩形还是短矩形

C:

返回20 表示成功；返回21 表示失败





Modbus：

接受端，如果收到5则表示需要抓取物件；如果为4则表示需要松开物件

***Notice：***

1. 更改了SendXY2ModBus（double WorldX, double WorldY, double Flag, double WorldZ = 0）

   可能需要增加寄存器来重新读值

***20190220：***

1. 完成了三个相机的逻辑编写

2. 完成了每个相机的三种情况和C相机的角度

3. 需要实践再次测量


backlog:

1. 测试C相机的长矩形和短矩形
2. 测出偏转角度
3. 将机械手端的程序流程写完

***20190220：***

**1. 测试A相机中的长短矩形和图像处理(Done)**

将很近的矩形去掉：

~~~C#
 //Console.WriteLine("Before boxList.size:"+boxList.Count());
            //将重复的很近的矩形去掉
            for (int q = 0; q<boxList.Count()-1;q++ )
            {
                //先看质点X的距离
                if (Math.Abs(boxList[q].center.X - boxList[q + 1].center.X) <= 30)
                {
                    boxList.RemoveAt(q);
                }
                else {
                    continue;
                }
                //Console.WriteLine("boxList[{0}].Area:{1}",q,boxList[q].MinAreaRect().Size);
            }
           // Console.WriteLine("After Before boxList.size:" + boxList.Count());
~~~

2.situation 4：既有圆形也有矩形，这里需要判断矩形的个数和圆形的个数

~~~C#

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
                    if (circles[RectBig2].Center.X <= tmpMin)
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
                    //图像中x,y的坐标位置
                    ARectangleX = (double)(boxList[tmpRecordI].center.X);
                    ARectangleY = (double)(boxList[tmpRecordI].center.Y);
                    //Console.WriteLine("矩形的重心：" + boxList[0].center);
                    Console.WriteLine("ARectangleX[1]:" + boxList[tmpRecordI].center.X);
                    Console.WriteLine("ARectangleY[1]:" + boxList[tmpRecordI].center.Y);
                    Console.WriteLine("ARectangleY[1]:" + boxList[tmpRecordI].angle);
                    //rectangle_location
                    double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                    g_AtmpWRectX = (world_cor[0, 0] / s) * 1000;
                    g_AtmpWRectY = (world_cor[1, 0] / s) * 1000;

                    g_AWRectX = g_AtmpWRectX;
                    g_AWRectY = g_AtmpWRectY;
                    Console.WriteLine("g_AWRectX[1]:" + g_AWRectX);
                    Console.WriteLine("g_AWRectY[1]:" + g_AWRectY);
                    isARect = 1;
                }
                else
                {
                    //圆形在左
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
                    isACircle = 1;
                }
                #endregion
            }    
~~~

**2.C相机工件的四种情况，和长矩形与短矩形的长度**

1. C相机的四种情况和图像处理（done）

**3.将PC端的整个流程梳理了一遍**

**B相机的处理还没有验证**

1. B相机中的矩形检测解决了，增加了膨胀腐蚀(done)

~~~C#
private int ImageProcess_B(string ImagePath)
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
                100, //min radius
                0 //max radius
                )[0]; //Get the circles from the first channel
            #endregion

            
            #region Canny and edge detection
            double cannyThresholdLinking = 200.0;
            Image<Gray, Byte> cannyEdges = MedianImage.Canny(cannyThreshold, cannyThresholdLinking);
            CvInvoke.cvDilate(cannyEdges, cannyEdges, IntPtr.Zero, 3);
            CvInvoke.cvErode(cannyEdges, cannyEdges, IntPtr.Zero, 2);
            pictureBox_C_processed.Image = cannyEdges.ToBitmap();
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
                    Contour<Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.01, storage1);//注意这里的The desired approximation accuracy为0.04
                    //Console.WriteLine("currentContour.BoundingRectangle:" + currentContour.BoundingRectangle);
                    //做第一道筛选，将小矩形去掉
                    if (currentContour.BoundingRectangle.Width >= 300)
                    {
                        Console.WriteLine("currentContour.BoundingRectangle:" + currentContour.BoundingRectangle);
                        //做第一道筛选，将小矩形去掉
                        //区别长矩形和短矩形
                        if (Math.Abs((currentContour.BoundingRectangle.Width) - (currentContour.BoundingRectangle.Height)) >= 100)
                        {
                            //是长矩形
                            Console.WriteLine("B相机中的长矩形");
                            boxList.Add(currentContour.GetMinAreaRect());//5188
                            g_BFlag = 3;
                        }
                        else
                        { //是短矩形
                            if (circles[0].Area >= 300000)
                            {
                                Console.WriteLine("B相机中的圆形");
                                g_BFlag = 1;
                            }
                            else {//区别短矩形和圆形
                                Console.WriteLine("B相机中的短矩形");
                                boxList.Add(currentContour.GetMinAreaRect());
                                g_BFlag = 2;
                            }
                           
                        }
                    }
                    else
                    { //图像中多余的小矩形
                        continue;
                       // Console.WriteLine("图像中未能捕捉到矩形!");
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
~~~





























































































~~~C#
  #region 相机A的处理过程
        /* ***
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
          
            Console.WriteLine("A相机中的矩形的个数：" + boxList.Count());
            #region 对A中形状数量的判断
            #region Situation 1 ：A相机中既没有圆也没有矩形
            if (boxList.Count() == 0 && (circles.Count() == 0 || (circles[0].Area >= 4600 || circles[0].Area <= 3000)))
            {
                Console.WriteLine("In Camera A,The Rectangle and the Circle are no found!");
                return 11;
            }
            #endregion

            #region Situation 2 :只有圆形
            else if (boxList.Count() == 0)
            {
                /*输出圆的圆心*/
                Console.WriteLine("圆形的个数：" + circles.Count());
                if (circles.Count() == 0 || (circles[0].Area >= 4600 || circles[0].Area <= 3000))
                {
                    Console.WriteLine("In Camera A,The Rectangle and the Circle are no found!");
                    return 11;
                }
                else 
                {
                    ACircleX = (double)(circles[0].Center.X);
                    ACircleY = (double)(circles[0].Center.Y);
                    Console.WriteLine("圆形的重心：" + circles[0].Center);
                    //circle_location
                    double[,] image_pix_circle = new double[3, 1] { { ACircleX }, { ACircleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_circle, ref world_cor);

                    world_X_circle = (world_cor[0, 0] / s) * 1000;
                    world_Y_circle = (world_cor[1, 0] / s) * 1000;

                    g_AWCircleX = world_X_circle;
                    g_AWCircleY = world_Y_circle;
                    Console.WriteLine("A相机中的圆");
                    Console.WriteLine("g_AWCircleX:" + g_AWCircleX);
                    Console.WriteLine("g_AWCircleY:" + g_AWCircleY);
                }  
            }
            #endregion

            #region Situation 3 :只有矩形
            else if (circles.Count() == 0 || (circles[0].Area >= 4600 || circles[0].Area <= 3000))
            {
                /*
                 * 如果圆不存在，则判断矩形，矩形的数量
                 */
                if (boxList.Count() == 0)
                {
                    Console.WriteLine("In Camera A,The Rectangle and the Circle are no found!");
                    return 11;
                }
                else if (boxList.Count() == 1)
                {
                    //图像中x,y的坐标位置
                    ARectangleX = (double)(boxList[0].center.X);
                    ARectangleY = (double)(boxList[0].center.Y);
                    //Console.WriteLine("矩形的重心：" + boxList[0].center);
                    Console.WriteLine("ARectangleX[0]:" + boxList[0].center.X);
                    Console.WriteLine("ARectangleY[0]:" + boxList[0].center.Y);
                    Console.WriteLine("ARectangleY[0]:" + boxList[0].angle);

                    //rectangle_location
                    double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                    Matrix.MatrixMultiply(a, b, ref c);
                    Matrix.MatrixOpp(c, ref c_);
                    Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                    world_X_c = (world_cor[0, 0] / s) * 1000;
                    world_Y_c = (world_cor[1, 0] / s) * 1000;

                    g_AWRectX = world_X_c;
                    g_AWRectY = world_Y_c;
                    Console.WriteLine("g_AWRectX[0]" + g_AWRectX);
                    Console.WriteLine("g_AWRectY[0]" + g_AWRectY);
                }
                else
                {
                    //此时的矩形是两个,选择x轴坐标大的那一个
                    if (boxList[0].center.X >= (boxList[1].center.X + 5))
                    {
                        //图像中x,y的坐标位置
                        ARectangleX = (double)(boxList[0].center.X);
                        ARectangleY = (double)(boxList[0].center.Y);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("ARectangleX[0]:" + boxList[0].center.X);
                        Console.WriteLine("ARectangleY[0]:" + boxList[0].center.Y);
                        Console.WriteLine("ARectangleY[0]:" + boxList[0].angle);

                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        world_X_c = (world_cor[0, 0] / s) * 1000;
                        world_Y_c = (world_cor[1, 0] / s) * 1000;

                        g_AWRectX = world_X_c;
                        g_AWRectY = world_Y_c;
                        Console.WriteLine("g_AWRectX[0]" + g_AWRectX);
                        Console.WriteLine("g_AWRectY[0]" + g_AWRectY);

                    }
                    else
                    {
                        //图像中x,y的坐标位置
                        ARectangleX = (double)(boxList[1].center.X);
                        ARectangleY = (double)(boxList[1].center.Y);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("ARectangleX[1]:" + boxList[1].center.X);
                        Console.WriteLine("ARectangleY[1]:" + boxList[1].center.Y);
                        Console.WriteLine("ARectangleY[1]:" + boxList[1].angle);
                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        world_X_c = (world_cor[0, 0] / s) * 1000;
                        world_Y_c = (world_cor[1, 0] / s) * 1000;

                        g_AWRectX = world_X_c;
                        g_AWRectY = world_Y_c;
                        Console.WriteLine("g_AWRectX[1]:" + g_AWRectX);
                        Console.WriteLine("g_AWRectY[1]:" + g_AWRectY);
                    }
                }
            }
            #endregion
           
            #region Situation 4 :既有圆形又有矩形
            else 
            {
                double tmpCircle = circles[0].Center.X;
                double tmpRect1 = boxList[0].center.X;
                double tmpRect2 = boxList[1].center.X;

                if (tmpCircle >= tmpRect1)
                {
                    if (tmpCircle >= tmpRect2)
                    {
                        ACircleX = (double)(circles[0].Center.X);
                        ACircleY = (double)(circles[0].Center.Y);
                        Console.WriteLine("圆形的重心：" + circles[0].Center);
                        //circle_location
                        double[,] image_pix_circle = new double[3, 1] { { ACircleX }, { ACircleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_circle, ref world_cor);

                        world_X_circle = (world_cor[0, 0] / s) * 1000;
                        world_Y_circle = (world_cor[1, 0] / s) * 1000;

                        g_AWCircleX = world_X_circle;
                        g_AWCircleY = world_Y_circle;
                        Console.WriteLine("A相机中的圆");
                        Console.WriteLine("g_AWCircleX:" + g_AWCircleX);
                        Console.WriteLine("g_AWCircleY:" + g_AWCircleY);
                    }
                    else
                    {
                        //图像中x,y的坐标位置
                        ARectangleX = (double)(boxList[1].center.X);
                        ARectangleY = (double)(boxList[1].center.Y);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("ARectangleX[1]:" + boxList[1].center.X);
                        Console.WriteLine("ARectangleY[1]:" + boxList[1].center.Y);
                        Console.WriteLine("ARectangleY[1]:" + boxList[1].angle);
                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        world_X_c = (world_cor[0, 0] / s) * 1000;
                        world_Y_c = (world_cor[1, 0] / s) * 1000;

                        g_AWRectX = world_X_c;
                        g_AWRectY = world_Y_c;
                        Console.WriteLine("g_AWRectX[1]:" + g_AWRectX);
                        Console.WriteLine("g_AWRectY[1]:" + g_AWRectY);
                    }
                }
                else {
                    if (tmpRect1 >= tmpRect2)
                    {
                        //图像中x,y的坐标位置
                        ARectangleX = (double)(boxList[0].center.X);
                        ARectangleY = (double)(boxList[0].center.Y);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("ARectangleX[0]:" + boxList[0].center.X);
                        Console.WriteLine("ARectangleY[0]:" + boxList[0].center.Y);
                        Console.WriteLine("ARectangleY[0]:" + boxList[0].angle);

                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        world_X_c = (world_cor[0, 0] / s) * 1000;
                        world_Y_c = (world_cor[1, 0] / s) * 1000;

                        g_AWRectX = world_X_c;
                        g_AWRectY = world_Y_c;
                        Console.WriteLine("g_AWRectX[0]" + g_AWRectX);
                        Console.WriteLine("g_AWRectY[0]" + g_AWRectY);
                    }
                    else{
                        //图像中x,y的坐标位置
                        ARectangleX = (double)(boxList[1].center.X);
                        ARectangleY = (double)(boxList[1].center.Y);
                        //Console.WriteLine("矩形的重心：" + boxList[0].center);
                        Console.WriteLine("ARectangleX[1]:" + boxList[1].center.X);
                        Console.WriteLine("ARectangleY[1]:" + boxList[1].center.Y);
                        Console.WriteLine("ARectangleY[1]:" + boxList[1].angle);
                        //rectangle_location
                        double[,] image_pix_rect = new double[3, 1] { { ARectangleX }, { ARectangleY }, { 1 } };

                        Matrix.MatrixMultiply(a, b, ref c);
                        Matrix.MatrixOpp(c, ref c_);
                        Matrix.MatrixMultiply(c_, image_pix_rect, ref world_cor);

                        world_X_c = (world_cor[0, 0] / s) * 1000;
                        world_Y_c = (world_cor[1, 0] / s) * 1000;

                        g_AWRectX = world_X_c;
                        g_AWRectY = world_Y_c;
                        Console.WriteLine("g_AWRectX[1]:" + g_AWRectX);
                        Console.WriteLine("g_AWRectY[1]:" + g_AWRectY);
                    
                    }
                }
            }
            #endregion
            #endregion



            #endregion
            return 10;
        }
~~~

~~~抓手程序 
--Start To Write RL
--Modbus寄存器地址DW:0x1000保存的是视觉图像坐标的X值
--Modbus寄存器地址DW:0x1002保存的是视觉图像坐标的Y值
--Modbus寄存器地址DW:0x1004保存的是视觉图像坐标的Z值
--Modbus寄存器地址DW:0x1006:写入Modbus中的值如果为1,则表示第一个图像坐标传递成功

--A相机的原点位置(374.436,-318.182,-82.411,36.246)
--优化之后（382.436，-316.482）
--A相机的世界坐标系是与机械手的XY坐标系一致
--B相机的位置(241.231,180.950,-43.268,63.937)

--C相机的原点位置(401.335,77.665,-98.149,36.246)
--优化之后（412.835,85.665）
--初始化需要用到的数据
	CDataX = 0
	CDataY = 0
	ADataX = 0
	ADataX = 0
	ADataZ = 0
	ADataX_tmp = 0
	ADataY_tmp = 0
	ADataZ_tmp = 0
	CDataX_tmp = 0
	CDataY_tmp = 0
	CDataZ_tmp = 0

RobotServoOff()
RobotServoOn()
--初始化
WriteModbus(0x1000,"DW",0)
WriteModbus(0x1002,"DW",0)
WriteModbus(0x1004,"DW",0)
WriteModbus(0x1006,"DW",0)

--A相机中抓取物件的角度
A_RZ = 36.259
--C相机中的角度
C_RZ = 36.259
--Up_Z上升高度
Up_Z = -22.628

--test_up = -9.865

--WahtIsA：用来判断A中抓取的是什么物件
--WhatIsA = 1:圆；2：长矩形；3:短矩形
WhatIsA = 0;

--0.初始化机械手原点
SetGlobalPoint(11,"Initial_point_O",406.037,-375.132,-43.268,A_RZ,0,0,0)
--0.设置中间点位
--1:圆(A抓-172.015)；2：长矩形(-153.697)；3：短矩形(A抓-131.472)
SetGlobalPoint(1,"Middle_Point",236.483,205.192,Up_Z,A_RZ,0,0,0)
SetGlobalPoint(2,"Middle_Point",237.963,206.280,Up_Z,A_RZ,0,0,0)
SetGlobalPoint(3,"Middle_Point",238.684,210.819,Up_Z,A_RZ,0,0,0)
MovP(11,15,15,15)


while 1 do
										--###1###
	--判断A相机处理成功，11表示相机传值失败
		ReadNullA = 0
		repeat 
			ReadNullA = ReadModbus(0x1006,"DW")
			ReadNullA = ReadNullA /1000
		until ReadNullA == 10 
										--###2###
	repeat
	--读取Modbus中A料盘中的坐标,并移动到该点
			ADataX_tmp = ReadModbus(0x1000,"DW")
			ADataY_tmp = ReadModbus(0x1002,"DW")
			ADataZ_tmp = ReadModbus(0x1004,"DW")
			ADataX_tmp = ADataX_tmp/1000
			ADataY_tmp = ADataY_tmp/1000  
			ADataZ_tmp = ADataZ_tmp/1000 
			
			ADataX = 382.436 + ADataX_tmp
			ADataY = -316.482 + ADataY_tmp
			ADataZ = ADataZ_tmp
			
	--根据传值过来的Z轴的高度值判定A盘中是什么形状
	--1：圆；2：长矩形；3：短矩形
			if ADataZ == -153.697 then
				WhatIsA = 2
			elseif ADataZ == -132.202 then
				WhatIsA = 3
			else
				WhatIsA = 1
			end
		
			--1001点存储A相机的X&Y
			WritePoint(1001,"X",ADataX)
			WritePoint(1001,"Y",ADataY)
			--WritePoint(1001,"Z",ADataZ)
			WritePoint(1001,"RZ",A_RZ)
		until 	ADataX~=382.436
			MovP(1001,5,5,5)
			DELAY(2)
			--Down
			WritePoint(1001,"Z",ADataZ)
			MovP(1001,5,5,5)
			
			--告诉计算机需要抓取物件了
			WriteModbus(0x1006,"DW",5)
	
	
		--如果返回值是55，说明机械爪抓取操作成功
		repeat 	
			Flag_Grab_Done = ReadModbus(0x1006,"DW")
			Flag_Grab_Done = Flag_Grab_Done/1000
		until Flag_Grab_Done == 55 
	
		DELAY(1)
		--Up
		WritePoint(1001,"Z",Up_Z)
		MovP(1001,5,5,5)
	  
	  --DELAY(5)
		
		 --移到中间B点
	  if WhatIsA == 1 then
		 	MovP(1,15,15,15)
	  elseif WhatIsA == 2 then
		 	MovP(2,15,15,15)		
  	else
		   MovP(3,15,15,15)
 	 end
		
	 --判断机械手是否到达了B相机所在之处
 	B_Done = 0
	 RealA2B_X = 0
	 repeat
	 	RealA2B_X = RobotX()
			if RealA2B_X <= 240.000 then
				if RealA2B_X >= 225.500 then
					B_Done = 1
				end
			end
   until B_Done == 1
		
 	--告诉计算机到达了B点,给计算机一个13
		WriteModbus(0x1006,"DW",13)
		
		--读取C相机的处理结果，20表示C处理成功了
	 ReadNullC = 0
		repeat 
			ReadNullC = ReadModbus(0x1006,"DW")
			ReadNullC = ReadNullC/1000
		until ReadNullC == 20 
		
	
	--------第三阶段---------
	--3.读取Modbus中C料盘中的坐标,并移动到该点
		repeat
			CDataX_tmp = ReadModbus(0x1000,"DW")
			CDataY_tmp = ReadModbus(0x1002,"DW")
			CDataZ_tmp = ReadModbus(0x1004,"DW")
			CDataX_tmp = CDataX_tmp/1000
			CDataY_tmp = CDataY_tmp/1000  
			CDataZ_tmp = CDataZ_tmp/1000 
		
			--CDataZ = 36.246		
			--401.335,77.665
			--412.835,85.665
			CDataX = 412.735 + CDataX_tmp
			CDataY = 85.565 + CDataY_tmp
	
			WritePoint(1002,"X",CDataX)
			WritePoint(1002,"Y",CDataY)
			WritePoint(1002,"RZ",C_RZ)
	until CDataX ~= 412.735
			--DELAY(1)
			--1002点存储C相机的X&Y
	
		MovP(1002,5,5,5)
 	 RealB2C_X_up = 0
		RealB2C_X_up_done = 0
		repeat
			RealB2C_X_up = RobotX()
			if RealB2C_X_up >= CDataX-1 then
				if RealB2C_X_up <= CDataX + 1 then
						RealB2C_X_up_done = 1
						WriteModbus(0x1006,"DW",10)
				end
		  end
		until	RealB2C_X_up_done == 1

		
		--Down，这里需要分情况，下降（后期考虑）
		--圆形下降-151.011
	--长矩形下降-135.181
	--短矩形下降-115.409
	if WhatIsA == 1 then
		WritePoint(1002,"Z",-151.011)
		MovP(1002,5,5,5)
	elseif WhatIsA == 2 then 
		WritePoint(1002,"Z",-135.181)
		MovP(1002,5,5,5)
	else
		WritePoint(1002,"Z",-135.181)
		MovP(1002,5,5,5)
	end
			
		
		repeat
			RealB2C_Z_down = RobotZ()
		until RealB2C_Z_down == -151.011
	
		
		--给Modbus写入信号,告诉机械手要放开物件了
			WriteModbus(0x1006,"DW",4)


		repeat 
		--如果返回值是44，说明放开操作成功
			Flag_Grab_Done = ReadModbus(0x1006,"DW")
		Flag_Grab_Done = Flag_Grab_Done/1000
		until Flag_Grab_Done == 44
end


~~~

