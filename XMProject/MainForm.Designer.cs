namespace SimpleImageDisplaySample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Button btnSetPorts;
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.camListBox = new System.Windows.Forms.ListBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.pictureBox_A = new System.Windows.Forms.PictureBox();
            this.pictureBox_B = new System.Windows.Forms.PictureBox();
            this.pictureBox_C = new System.Windows.Forms.PictureBox();
            this.btnCalib_A = new System.Windows.Forms.Button();
            this.tbxHistory = new System.Windows.Forms.RichTextBox();
            this.pictureBox_A_processed = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.btnCalib_C = new System.Windows.Forms.Button();
            this.pictureBox_C_processed = new System.Windows.Forms.PictureBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox_B_Processing = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnOpenSerial = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnGrabing = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.btnReleasing = new System.Windows.Forms.Button();
            this.textPower = new System.Windows.Forms.TextBox();
            this.textSpeed = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtShow_Recieved = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnImageProcess = new System.Windows.Forms.Button();
            btnSetPorts = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_B)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_C)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A_processed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_C_processed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_B_Processing)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSetPorts
            // 
            btnSetPorts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnSetPorts.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            btnSetPorts.Location = new System.Drawing.Point(1139, 343);
            btnSetPorts.Name = "btnSetPorts";
            btnSetPorts.Size = new System.Drawing.Size(100, 30);
            btnSetPorts.TabIndex = 36;
            btnSetPorts.Text = "SetPorts";
            btnSetPorts.UseVisualStyleBackColor = true;
            btnSetPorts.Click += new System.EventHandler(this.btnSetPorts_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.camListBox);
            this.groupBox1.Controls.Add(this.SearchButton);
            this.groupBox1.Font = new System.Drawing.Font("楷体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(1131, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(233, 89);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Camera ID ";
            // 
            // camListBox
            // 
            this.camListBox.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.camListBox.FormattingEnabled = true;
            this.camListBox.ItemHeight = 14;
            this.camListBox.Location = new System.Drawing.Point(6, 20);
            this.camListBox.Name = "camListBox";
            this.camListBox.Size = new System.Drawing.Size(167, 46);
            this.camListBox.TabIndex = 7;
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(188, 20);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(29, 21);
            this.SearchButton.TabIndex = 6;
            this.SearchButton.Text = "...";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // StartButton
            // 
            this.StartButton.Enabled = false;
            this.StartButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.StartButton.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StartButton.Location = new System.Drawing.Point(1137, 107);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(100, 30);
            this.StartButton.TabIndex = 2;
            this.StartButton.Text = "OpenCamera";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StopButton.Location = new System.Drawing.Point(1264, 107);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(100, 30);
            this.StopButton.TabIndex = 3;
            this.StopButton.Text = "StopCamera";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // pictureBox_A
            // 
            this.pictureBox_A.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_A.Location = new System.Drawing.Point(12, 11);
            this.pictureBox_A.Name = "pictureBox_A";
            this.pictureBox_A.Size = new System.Drawing.Size(367, 343);
            this.pictureBox_A.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_A.TabIndex = 6;
            this.pictureBox_A.TabStop = false;
            // 
            // pictureBox_B
            // 
            this.pictureBox_B.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_B.Location = new System.Drawing.Point(385, 11);
            this.pictureBox_B.Name = "pictureBox_B";
            this.pictureBox_B.Size = new System.Drawing.Size(367, 343);
            this.pictureBox_B.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_B.TabIndex = 10;
            this.pictureBox_B.TabStop = false;
            // 
            // pictureBox_C
            // 
            this.pictureBox_C.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_C.Location = new System.Drawing.Point(758, 11);
            this.pictureBox_C.Name = "pictureBox_C";
            this.pictureBox_C.Size = new System.Drawing.Size(367, 343);
            this.pictureBox_C.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_C.TabIndex = 11;
            this.pictureBox_C.TabStop = false;
            // 
            // btnCalib_A
            // 
            this.btnCalib_A.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCalib_A.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCalib_A.Location = new System.Drawing.Point(1139, 186);
            this.btnCalib_A.Name = "btnCalib_A";
            this.btnCalib_A.Size = new System.Drawing.Size(100, 30);
            this.btnCalib_A.TabIndex = 13;
            this.btnCalib_A.Text = "Calib_A";
            this.btnCalib_A.UseVisualStyleBackColor = true;
            this.btnCalib_A.Click += new System.EventHandler(this.btnCalib_A_Click);
            // 
            // tbxHistory
            // 
            this.tbxHistory.Location = new System.Drawing.Point(1139, 236);
            this.tbxHistory.Name = "tbxHistory";
            this.tbxHistory.Size = new System.Drawing.Size(225, 87);
            this.tbxHistory.TabIndex = 17;
            this.tbxHistory.Text = "";
            // 
            // pictureBox_A_processed
            // 
            this.pictureBox_A_processed.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_A_processed.Location = new System.Drawing.Point(12, 376);
            this.pictureBox_A_processed.Name = "pictureBox_A_processed";
            this.pictureBox_A_processed.Size = new System.Drawing.Size(367, 343);
            this.pictureBox_A_processed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_A_processed.TabIndex = 18;
            this.pictureBox_A_processed.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(136, 357);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 30;
            this.label1.Text = "Camera_A";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(521, 357);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 30;
            this.label2.Text = "Camera_B";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(93, 722);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 16);
            this.label3.TabIndex = 30;
            this.label3.Text = "Camera_A_processed";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(906, 357);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 16);
            this.label4.TabIndex = 30;
            this.label4.Text = "Camera_C";
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 1000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // btnCalib_C
            // 
            this.btnCalib_C.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCalib_C.Location = new System.Drawing.Point(1264, 186);
            this.btnCalib_C.Name = "btnCalib_C";
            this.btnCalib_C.Size = new System.Drawing.Size(100, 30);
            this.btnCalib_C.TabIndex = 31;
            this.btnCalib_C.Text = "Calib_C";
            this.btnCalib_C.UseVisualStyleBackColor = true;
            this.btnCalib_C.Click += new System.EventHandler(this.btnCalib_C_Click);
            // 
            // pictureBox_C_processed
            // 
            this.pictureBox_C_processed.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_C_processed.Location = new System.Drawing.Point(758, 376);
            this.pictureBox_C_processed.Name = "pictureBox_C_processed";
            this.pictureBox_C_processed.Size = new System.Drawing.Size(367, 343);
            this.pictureBox_C_processed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_C_processed.TabIndex = 32;
            this.pictureBox_C_processed.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(867, 722);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 16);
            this.label5.TabIndex = 33;
            this.label5.Text = "Camera_C_processed";
            // 
            // pictureBox_B_Processing
            // 
            this.pictureBox_B_Processing.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_B_Processing.Location = new System.Drawing.Point(385, 376);
            this.pictureBox_B_Processing.Name = "pictureBox_B_Processing";
            this.pictureBox_B_Processing.Size = new System.Drawing.Size(367, 343);
            this.pictureBox_B_Processing.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_B_Processing.TabIndex = 34;
            this.pictureBox_B_Processing.TabStop = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(505, 722);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 16);
            this.label6.TabIndex = 35;
            this.label6.Text = "processing";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(1136, 219);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 14);
            this.label7.TabIndex = 37;
            this.label7.Text = "Modbus数据";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel5});
            this.statusStrip1.Location = new System.Drawing.Point(0, 733);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1370, 22);
            this.statusStrip1.TabIndex = 38;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel2.Text = "toolStripStatusLabel2";
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel3.Text = "toolStripStatusLabel3";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel4.Text = "toolStripStatusLabel4";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel5.Text = "toolStripStatusLabel5";
            // 
            // btnOpenSerial
            // 
            this.btnOpenSerial.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOpenSerial.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOpenSerial.Location = new System.Drawing.Point(1264, 343);
            this.btnOpenSerial.Name = "btnOpenSerial";
            this.btnOpenSerial.Size = new System.Drawing.Size(100, 30);
            this.btnOpenSerial.TabIndex = 39;
            this.btnOpenSerial.Text = "OpenSerial";
            this.btnOpenSerial.UseVisualStyleBackColor = true;
            this.btnOpenSerial.Click += new System.EventHandler(this.btnOpenSerial_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnGrabing);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.btnReleasing);
            this.groupBox2.Controls.Add(this.textPower);
            this.groupBox2.Controls.Add(this.textSpeed);
            this.groupBox2.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(1139, 387);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(225, 147);
            this.groupBox2.TabIndex = 40;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Serial_Sending";
            // 
            // btnGrabing
            // 
            this.btnGrabing.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnGrabing.Location = new System.Drawing.Point(6, 104);
            this.btnGrabing.Name = "btnGrabing";
            this.btnGrabing.Size = new System.Drawing.Size(100, 30);
            this.btnGrabing.TabIndex = 42;
            this.btnGrabing.Text = "Grabing";
            this.btnGrabing.UseVisualStyleBackColor = true;
            this.btnGrabing.Click += new System.EventHandler(this.btnGrabing_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("楷体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(112, 35);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(107, 12);
            this.label9.TabIndex = 46;
            this.label9.Text = "Power(100<X<1500)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("楷体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(6, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 12);
            this.label8.TabIndex = 46;
            this.label8.Text = "Speed(1<X<255)";
            // 
            // btnReleasing
            // 
            this.btnReleasing.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnReleasing.Location = new System.Drawing.Point(119, 104);
            this.btnReleasing.Name = "btnReleasing";
            this.btnReleasing.Size = new System.Drawing.Size(100, 30);
            this.btnReleasing.TabIndex = 43;
            this.btnReleasing.Text = "Releasing";
            this.btnReleasing.UseVisualStyleBackColor = true;
            this.btnReleasing.Click += new System.EventHandler(this.btnReleaseMax_Click);
            // 
            // textPower
            // 
            this.textPower.Location = new System.Drawing.Point(119, 61);
            this.textPower.Name = "textPower";
            this.textPower.Size = new System.Drawing.Size(100, 23);
            this.textPower.TabIndex = 45;
            // 
            // textSpeed
            // 
            this.textSpeed.Location = new System.Drawing.Point(6, 61);
            this.textSpeed.Name = "textSpeed";
            this.textSpeed.Size = new System.Drawing.Size(100, 23);
            this.textSpeed.TabIndex = 44;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtShow_Recieved);
            this.groupBox3.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox3.Location = new System.Drawing.Point(1139, 540);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(219, 117);
            this.groupBox3.TabIndex = 41;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Serial_Recieving";
            // 
            // txtShow_Recieved
            // 
            this.txtShow_Recieved.Location = new System.Drawing.Point(6, 33);
            this.txtShow_Recieved.Multiline = true;
            this.txtShow_Recieved.Name = "txtShow_Recieved";
            this.txtShow_Recieved.Size = new System.Drawing.Size(207, 78);
            this.txtShow_Recieved.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            // 
            // btnImageProcess
            // 
            this.btnImageProcess.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnImageProcess.Font = new System.Drawing.Font("楷体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnImageProcess.Location = new System.Drawing.Point(1204, 143);
            this.btnImageProcess.Name = "btnImageProcess";
            this.btnImageProcess.Size = new System.Drawing.Size(100, 30);
            this.btnImageProcess.TabIndex = 42;
            this.btnImageProcess.Text = "ImageProcess";
            this.btnImageProcess.UseVisualStyleBackColor = false;
            this.btnImageProcess.Click += new System.EventHandler(this.btnImageProcess_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1370, 755);
            this.Controls.Add(this.btnImageProcess);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnOpenSerial);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label7);
            this.Controls.Add(btnSetPorts);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.pictureBox_B_Processing);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.pictureBox_C_processed);
            this.Controls.Add(this.btnCalib_C);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbxHistory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox_A_processed);
            this.Controls.Add(this.btnCalib_A);
            this.Controls.Add(this.pictureBox_C);
            this.Controls.Add(this.pictureBox_B);
            this.Controls.Add(this.pictureBox_A);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "MainForm";
            this.Text = "XMProject";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestModBus_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_B)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_C)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A_processed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_C_processed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_B_Processing)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.PictureBox pictureBox_A;
        private System.Windows.Forms.ListBox camListBox;
        //
        private System.Windows.Forms.PictureBox pictureBox_B;
        //
        private System.Windows.Forms.PictureBox pictureBox_C;
        private System.Windows.Forms.Button btnCalib_A;
        private System.Windows.Forms.RichTextBox tbxHistory;
        private System.Windows.Forms.PictureBox pictureBox_A_processed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button btnCalib_C;
        private System.Windows.Forms.PictureBox pictureBox_C_processed;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.PictureBox pictureBox_B_Processing;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.Button btnOpenSerial;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtShow_Recieved;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnReleasing;
        private System.Windows.Forms.TextBox textSpeed;
        private System.Windows.Forms.TextBox textPower;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btnGrabing;
        private System.Windows.Forms.Button btnImageProcess;
       
    }
}

