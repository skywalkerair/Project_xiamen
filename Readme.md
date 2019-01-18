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

