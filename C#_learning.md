# C#语法学习

---

*20181026*：

[c# 语法网址](https://www.cnblogs.com/xdzy/p/9457848.html)

~~~C#
public Form1()
{
    InitializeComponent();
    //调用初始化方法，其代码再Form1.Designer.cs文件中
}
~~~



20181118：

（需要自己敲一遍代码）[我的OpenCV学习笔记（19）：检测轮廓，直线，圆以及直线拟合](https://blog.csdn.net/thefutureisour/article/details/7599537)

~~~markdown

~~~

[[转载+原创\]Emgu CV on C# （六） —— Emgu CV on Canny边缘检测](https://www.cnblogs.com/MobileBo/p/3947128.html)]

~~~markdown
1. 关键函数
public static void cvCanny(
	IntPtr image,
	IntPtr edges,
	double threshold1,
	double threshold2,
	int apertureSize
)
第一个参数：image,表示输入图像，必须是单通道灰度图。
第二个参数：edges,表示输出边缘图像，为单通道黑白图。
第三个参数：threshold1,the first threshold。
第四个参数：threshold2,the sencond threshold。
***这两个阈值中小的阈值用来控制边缘连接，大的阈值用来控制边缘的初始分割，即如果一个图像的梯度大于上限值，则被认为时边缘像素，如果小于下线阈值，则被抛弃。如果该点的梯度在两者之间则当这个点与高于上限值的像素点连接时我们才保留，否则删除。***
第五个参数：aperture,表示Sobel算子的大小，默认是3，即3*3的矩阵；Sobel算子与高斯拉普拉斯算子都是常用的边缘算子。

**说明：**
* 定义：IntPtr类型称为”平台特定的整数类型“，用于本机资源，如窗口句柄。资源大小取决于使用的硬件和操作系统，但其大小总是足以包含系统的指针。
* 所用到的地方：（1）c# 调用WINAPI时； （2）c#调用c/c++写的DLL时（其实和（1）相同，只是这个一般是我们在和他人合作开发时经常用到）

~~~

