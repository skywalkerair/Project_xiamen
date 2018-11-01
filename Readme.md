# 文件说明

* 2018.10.23：zsx_PC=>备份实验室PC端的厦门科技局项目文件
* 总结和整理整个项目，理清楚整个项目的流程，并规划项目的下一步

## 项目总体要达到的目标：

1. 精度的保证：实现三个图像的精准定位，提高精度：

   （1）精度包括三个精度：机械手端精度，定标精度，图像处理之后的物体精度

2. 论文成果转化：把论文中的超分辨率重建的方法引入到实际的系统中

3. 速度问题：在课题要求的速度下，实现运行



***20181025-任务***：

1. 备份当前目录项目的文件（DONE）
2. 简化代码，只显示矩形和圆形（DONE）
3. 将三个相机的图像处理都加进去（）
4. 两个相机的位置定标问题，位置坐标解决方案,两套定标程序
5. 是否需要在通讯端增加反馈
6. 速度问题，提速



***20181031代码更新***

* 将所有的模块就精简了（DONE）

* 加入了C相机的定标模块（***未完成***：还没有测试）

* A盘中的长方形，正方形和圆形的区分要与B盘中的长方形，正方形和圆形对应

  ***11月1号需要做的***：

  * 将B相机的程序完成，查找代码，保存图像，处理图像
  * 将C相机的定标程序测试完成
  * Modbus的双工通讯程序要完成

***20181101代码更新***

* 将C相机的图像显示出来了，标定还没进行，今天优化了图像处理，将圆形更好的显示了出来

  ***问题：***在边缘提取之后有很多的边缘，需要选一个，只能是最优的那个，怎么选，是个问题


























---

***Git的使用说明***

***Operation:***
​	git add .
​	git commit -m "XXXXXXX"
​	git push 

~~~git
***Quick setup — if you’ve done this kind of thing before***
******************************************************
（1)…or create a new repository on the command line
 echo "# Project_xiamen" >> README.md
git init
git add README.md
git commit -m "first commit"
git remote add origin https://github.com/skywalkerair/Project_xiamen.git
git push -u origin master
（2）…or push an existing repository from the command line
 git remote add origin https://github.com/skywalkerair/Project_xiamen.git
git push -u origin master
（3）…or import code from another repository
You can initialize this repository with code from a Subversion, Mercurial, or TFS project.


~~~

