namespace SimpleImageDisplaySample
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.camListBox = new System.Windows.Forms.ListBox();
            this.SearchButton = new System.Windows.Forms.Button();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.pictureBox_A = new System.Windows.Forms.PictureBox();
            this.pictureBox_B = new System.Windows.Forms.PictureBox();
            this.pictureBox_C = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCameraControl = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenuItem1 = new System.Windows.Forms.Button();
            this.tbxHistory = new System.Windows.Forms.RichTextBox();
            this.pictureBox_A_processed = new System.Windows.Forms.PictureBox();
            this.ZoomInbutton = new System.Windows.Forms.Button();
            this.ZoomResetbutton = new System.Windows.Forms.Button();
            this.ZoomOutbutton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.toolStripMenuItem_C = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_B)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_C)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A_processed)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.camListBox);
            this.groupBox1.Controls.Add(this.SearchButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 11);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(523, 66);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "ID of the camera found";
            // 
            // camListBox
            // 
            this.camListBox.FormattingEnabled = true;
            this.camListBox.ItemHeight = 12;
            this.camListBox.Location = new System.Drawing.Point(9, 16);
            this.camListBox.Name = "camListBox";
            this.camListBox.Size = new System.Drawing.Size(473, 40);
            this.camListBox.TabIndex = 7;
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(488, 16);
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
            this.StartButton.Location = new System.Drawing.Point(35, 83);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 21);
            this.StartButton.TabIndex = 2;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(144, 83);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 21);
            this.StopButton.TabIndex = 3;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // pictureBox_A
            // 
            this.pictureBox_A.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_A.Location = new System.Drawing.Point(12, 142);
            this.pictureBox_A.Name = "pictureBox_A";
            this.pictureBox_A.Size = new System.Drawing.Size(342, 343);
            this.pictureBox_A.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_A.TabIndex = 6;
            this.pictureBox_A.TabStop = false;
            // 
            // pictureBox_B
            // 
            this.pictureBox_B.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_B.Location = new System.Drawing.Point(12, 507);
            this.pictureBox_B.Name = "pictureBox_B";
            this.pictureBox_B.Size = new System.Drawing.Size(342, 303);
            this.pictureBox_B.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_B.TabIndex = 10;
            this.pictureBox_B.TabStop = false;
            // 
            // pictureBox_C
            // 
            this.pictureBox_C.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_C.Location = new System.Drawing.Point(770, 142);
            this.pictureBox_C.Name = "pictureBox_C";
            this.pictureBox_C.Size = new System.Drawing.Size(342, 343);
            this.pictureBox_C.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_C.TabIndex = 11;
            this.pictureBox_C.TabStop = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripButtonStart,
            this.toolStripButtonStop,
            this.toolStripButtonCameraControl});
            this.toolStrip1.Location = new System.Drawing.Point(552, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(230, 43);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 12;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.AutoSize = false;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(69, 27);
            this.toolStripLabel1.Text = "FlyCapture";
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.AutoSize = false;
            this.toolStripButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStart.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStart.Image")));
            this.toolStripButtonStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(30, 32);
            this.toolStripButtonStart.Text = "Play";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonStart_Click);
            // 
            // toolStripButtonStop
            // 
            this.toolStripButtonStop.AutoSize = false;
            this.toolStripButtonStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonStop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonStop.Image")));
            this.toolStripButtonStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonStop.Name = "toolStripButtonStop";
            this.toolStripButtonStop.Size = new System.Drawing.Size(30, 32);
            this.toolStripButtonStop.Text = "Stop";
            // 
            // toolStripButtonCameraControl
            // 
            this.toolStripButtonCameraControl.AutoSize = false;
            this.toolStripButtonCameraControl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCameraControl.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCameraControl.Image")));
            this.toolStripButtonCameraControl.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCameraControl.Name = "toolStripButtonCameraControl";
            this.toolStripButtonCameraControl.Size = new System.Drawing.Size(30, 32);
            this.toolStripButtonCameraControl.Text = "Controls";
            this.toolStripButtonCameraControl.Click += new System.EventHandler(this.toolStripButtonCameraControl_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Location = new System.Drawing.Point(35, 110);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(75, 23);
            this.toolStripMenuItem1.TabIndex = 13;
            this.toolStripMenuItem1.Text = "标定A";
            this.toolStripMenuItem1.UseVisualStyleBackColor = true;
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click_A);
            // 
            // tbxHistory
            // 
            this.tbxHistory.Location = new System.Drawing.Point(243, 73);
            this.tbxHistory.Name = "tbxHistory";
            this.tbxHistory.Size = new System.Drawing.Size(524, 63);
            this.tbxHistory.TabIndex = 17;
            this.tbxHistory.Text = "";
            // 
            // pictureBox_A_processed
            // 
            this.pictureBox_A_processed.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.pictureBox_A_processed.Location = new System.Drawing.Point(410, 142);
            this.pictureBox_A_processed.Name = "pictureBox_A_processed";
            this.pictureBox_A_processed.Size = new System.Drawing.Size(342, 343);
            this.pictureBox_A_processed.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_A_processed.TabIndex = 18;
            this.pictureBox_A_processed.TabStop = false;
            // 
            // ZoomInbutton
            // 
            this.ZoomInbutton.Image = ((System.Drawing.Image)(resources.GetObject("ZoomInbutton.Image")));
            this.ZoomInbutton.Location = new System.Drawing.Point(360, 219);
            this.ZoomInbutton.Name = "ZoomInbutton";
            this.ZoomInbutton.Size = new System.Drawing.Size(44, 41);
            this.ZoomInbutton.TabIndex = 20;
            this.ZoomInbutton.UseVisualStyleBackColor = true;
            this.ZoomInbutton.Click += new System.EventHandler(this.ZoomInbutton_Click);
            // 
            // ZoomResetbutton
            // 
            this.ZoomResetbutton.Image = ((System.Drawing.Image)(resources.GetObject("ZoomResetbutton.Image")));
            this.ZoomResetbutton.Location = new System.Drawing.Point(360, 291);
            this.ZoomResetbutton.Name = "ZoomResetbutton";
            this.ZoomResetbutton.Size = new System.Drawing.Size(44, 41);
            this.ZoomResetbutton.TabIndex = 20;
            this.ZoomResetbutton.UseVisualStyleBackColor = true;
            this.ZoomResetbutton.Click += new System.EventHandler(this.ZoomResetbutton_Click);
            // 
            // ZoomOutbutton
            // 
            this.ZoomOutbutton.Image = ((System.Drawing.Image)(resources.GetObject("ZoomOutbutton.Image")));
            this.ZoomOutbutton.Location = new System.Drawing.Point(360, 363);
            this.ZoomOutbutton.Name = "ZoomOutbutton";
            this.ZoomOutbutton.Size = new System.Drawing.Size(44, 41);
            this.ZoomOutbutton.TabIndex = 20;
            this.ZoomOutbutton.UseVisualStyleBackColor = true;
            this.ZoomOutbutton.Click += new System.EventHandler(this.ZoomOutbutton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(123, 488);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 16);
            this.label1.TabIndex = 30;
            this.label1.Text = "Camera_A";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(106, 813);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 16);
            this.label2.TabIndex = 30;
            this.label2.Text = "Camera_B";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(510, 488);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 16);
            this.label3.TabIndex = 30;
            this.label3.Text = "Camera_A_processed";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("黑体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(909, 488);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 16);
            this.label4.TabIndex = 30;
            this.label4.Text = "Camera_C";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            // 
            // timer2
            // 
            this.timer2.Enabled = true;
            this.timer2.Interval = 2000;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick_1);
            // 
            // toolStripMenuItem_C
            // 
            this.toolStripMenuItem_C.Location = new System.Drawing.Point(144, 110);
            this.toolStripMenuItem_C.Name = "toolStripMenuItem_C";
            this.toolStripMenuItem_C.Size = new System.Drawing.Size(75, 23);
            this.toolStripMenuItem_C.TabIndex = 31;
            this.toolStripMenuItem_C.Text = "定标C";
            this.toolStripMenuItem_C.UseVisualStyleBackColor = true;
            this.toolStripMenuItem_C.Click += new System.EventHandler(this.toolStripMenuItem_C_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1376, 865);
            this.Controls.Add(this.toolStripMenuItem_C);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ZoomOutbutton);
            this.Controls.Add(this.ZoomResetbutton);
            this.Controls.Add(this.ZoomInbutton);
            this.Controls.Add(this.pictureBox_A_processed);
            this.Controls.Add(this.tbxHistory);
            this.Controls.Add(this.toolStripMenuItem1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pictureBox_C);
            this.Controls.Add(this.pictureBox_B);
            this.Controls.Add(this.pictureBox_A);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Simple Image Display Sample";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestModBus_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_B)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_C)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_A_processed)).EndInit();
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
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButtonStart;
        private System.Windows.Forms.ToolStripButton toolStripButtonStop;
        private System.Windows.Forms.ToolStripButton toolStripButtonCameraControl;
        private System.Windows.Forms.Button toolStripMenuItem1;
        private System.Windows.Forms.RichTextBox tbxHistory;
        private System.Windows.Forms.PictureBox pictureBox_A_processed;
        private System.Windows.Forms.Button ZoomInbutton;
        private System.Windows.Forms.Button ZoomResetbutton;
        private System.Windows.Forms.Button ZoomOutbutton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button toolStripMenuItem_C;
       
    }
}

