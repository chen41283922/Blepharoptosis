namespace eyes
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.比例校正ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.imageBox6 = new Emgu.CV.UI.ImageBox();
            this.imageBox5 = new Emgu.CV.UI.ImageBox();
            this.imageBox2 = new Emgu.CV.UI.ImageBox();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.imageBox3 = new Emgu.CV.UI.ImageBox();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.radioButtonOri = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton_OSA = new System.Windows.Forms.RadioButton();
            this.radioButton_PS = new System.Windows.Forms.RadioButton();
            this.radioButton_PFW4 = new System.Windows.Forms.RadioButton();
            this.radioButton_PFH3 = new System.Windows.Forms.RadioButton();
            this.radioButton_MRD2 = new System.Windows.Forms.RadioButton();
            this.radioButton_MRD1 = new System.Windows.Forms.RadioButton();
            this.imageBox7 = new Emgu.CV.UI.ImageBox();
            this.imageBox8 = new Emgu.CV.UI.ImageBox();
            this.imageBox9 = new Emgu.CV.UI.ImageBox();
            this.imageBox10 = new Emgu.CV.UI.ImageBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox3)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox10)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem19,
            this.toolStripMenuItem1,
            this.toolStripMenuItem18});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(6, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1467, 30);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem19
            // 
            this.toolStripMenuItem19.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem20,
            this.toolStripMenuItem2,
            this.比例校正ToolStripMenuItem});
            this.toolStripMenuItem19.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripMenuItem19.Name = "toolStripMenuItem19";
            this.toolStripMenuItem19.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.toolStripMenuItem19.Size = new System.Drawing.Size(71, 24);
            this.toolStripMenuItem19.Text = "檔案(&F)";
            // 
            // toolStripMenuItem20
            // 
            this.toolStripMenuItem20.Image = global::eyes.Properties.Resources.單張影像;
            this.toolStripMenuItem20.Name = "toolStripMenuItem20";
            this.toolStripMenuItem20.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.toolStripMenuItem20.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem20.Text = "開啟照片";
            this.toolStripMenuItem20.Click += new System.EventHandler(this.toolStripMenuItem20_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::eyes.Properties.Resources.擷取影像;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.toolStripMenuItem2.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem2.Text = "拍照";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click_1);
            // 
            // 比例校正ToolStripMenuItem
            // 
            this.比例校正ToolStripMenuItem.Name = "比例校正ToolStripMenuItem";
            this.比例校正ToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
            this.比例校正ToolStripMenuItem.Text = "比例校正";
            this.比例校正ToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItem8_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem8,
            this.toolStripMenuItem12});
            this.toolStripMenuItem1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.E)));
            this.toolStripMenuItem1.Size = new System.Drawing.Size(72, 24);
            this.toolStripMenuItem1.Text = "功能(&E)";
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Enabled = false;
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(169, 24);
            this.toolStripMenuItem8.Text = "比例校正";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.toolStripMenuItem8_Click);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Image = global::eyes.Properties.Resources.eye1;
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.toolStripMenuItem12.Size = new System.Drawing.Size(169, 24);
            this.toolStripMenuItem12.Text = "眼睛";
            this.toolStripMenuItem12.Click += new System.EventHandler(this.toolStripMenuItem12_Click);
            // 
            // toolStripMenuItem18
            // 
            this.toolStripMenuItem18.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3});
            this.toolStripMenuItem18.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripMenuItem18.Name = "toolStripMenuItem18";
            this.toolStripMenuItem18.ShortcutKeyDisplayString = "";
            this.toolStripMenuItem18.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this.toolStripMenuItem18.Size = new System.Drawing.Size(75, 24);
            this.toolStripMenuItem18.Text = "說明(&H)";
            
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Image = global::eyes.Properties.Resources.關於本程式;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.toolStripMenuItem3.Size = new System.Drawing.Size(218, 24);
            this.toolStripMenuItem3.Text = "關於本程式";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(874, 280);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 27);
            this.label1.TabIndex = 3;
            this.label1.Text = "半徑:";
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(874, 320);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 27);
            this.label2.TabIndex = 4;
            this.label2.Text = "硬幣直徑";
            this.label2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(874, 361);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 27);
            this.label3.TabIndex = 5;
            this.label3.Text = "硬幣直徑";
            this.label3.Visible = false;
            // 
            // timer2
            // 
            this.timer2.Interval = 1;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Enabled = false;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(16, 401);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(11, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "0";
            this.label4.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(849, 171);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(225, 34);
            this.label5.TabIndex = 7;
            this.label5.Text = "                              ";
            this.label5.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微軟正黑體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(1082, 171);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(197, 34);
            this.label6.TabIndex = 8;
            this.label6.Text = "                          ";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label6.Visible = false;
            // 
            // imageBox6
            // 
            this.imageBox6.Location = new System.Drawing.Point(1155, 38);
            this.imageBox6.Name = "imageBox6";
            this.imageBox6.Size = new System.Drawing.Size(243, 121);
            this.imageBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox6.TabIndex = 2;
            this.imageBox6.TabStop = false;
            this.imageBox6.Visible = false;
            // 
            // imageBox5
            // 
            this.imageBox5.Location = new System.Drawing.Point(890, 38);
            this.imageBox5.Name = "imageBox5";
            this.imageBox5.Size = new System.Drawing.Size(243, 121);
            this.imageBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox5.TabIndex = 2;
            this.imageBox5.TabStop = false;
            this.imageBox5.Visible = false;
            // 
            // imageBox2
            // 
            this.imageBox2.Enabled = false;
            this.imageBox2.Location = new System.Drawing.Point(12, 416);
            this.imageBox2.Name = "imageBox2";
            this.imageBox2.Size = new System.Drawing.Size(464, 357);
            this.imageBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox2.TabIndex = 2;
            this.imageBox2.TabStop = false;
            // 
            // imageBox1
            // 
            this.imageBox1.Enabled = false;
            this.imageBox1.Location = new System.Drawing.Point(12, 41);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(464, 357);
            this.imageBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 829);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1467, 22);
            this.statusStrip1.TabIndex = 149;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Value = 1;
            this.toolStripProgressBar1.Visible = false;
            // 
            // imageBox3
            // 
            this.imageBox3.Enabled = false;
            this.imageBox3.Location = new System.Drawing.Point(482, 41);
            this.imageBox3.Name = "imageBox3";
            this.imageBox3.Size = new System.Drawing.Size(464, 357);
            this.imageBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox3.TabIndex = 153;
            this.imageBox3.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微軟正黑體", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(1242, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(225, 34);
            this.label7.TabIndex = 7;
            this.label7.Text = "                              ";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.label7.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButtonOri);
            this.panel1.Controls.Add(this.radioButton7);
            this.panel1.Controls.Add(this.radioButton_OSA);
            this.panel1.Controls.Add(this.radioButton_PS);
            this.panel1.Controls.Add(this.radioButton_PFW4);
            this.panel1.Controls.Add(this.radioButton_PFH3);
            this.panel1.Controls.Add(this.radioButton_MRD2);
            this.panel1.Controls.Add(this.radioButton_MRD1);
            this.panel1.Location = new System.Drawing.Point(817, 196);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(247, 301);
            this.panel1.TabIndex = 154;
            this.panel1.Visible = false;
            // 
            // radioButtonOri
            // 
            this.radioButtonOri.AutoSize = true;
            this.radioButtonOri.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButtonOri.ForeColor = System.Drawing.Color.White;
            this.radioButtonOri.Location = new System.Drawing.Point(16, 12);
            this.radioButtonOri.Name = "radioButtonOri";
            this.radioButtonOri.Size = new System.Drawing.Size(226, 25);
            this.radioButtonOri.TabIndex = 4;
            this.radioButtonOri.TabStop = true;
            this.radioButtonOri.Text = "檢測結果(單位 : mm)";
            this.radioButtonOri.UseVisualStyleBackColor = true;
            this.radioButtonOri.Visible = false;
            this.radioButtonOri.CheckedChanged += new System.EventHandler(this.radioButtonOri_CheckedChanged);
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton7.ForeColor = System.Drawing.Color.White;
            this.radioButton7.Location = new System.Drawing.Point(16, 243);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(119, 25);
            this.radioButton7.TabIndex = 4;
            this.radioButton7.TabStop = true;
            this.radioButton7.Text = "LEVATOR";
            this.radioButton7.UseVisualStyleBackColor = true;
            this.radioButton7.CheckedChanged += new System.EventHandler(this.radioButton7_CheckedChanged);
            // 
            // radioButton_OSA
            // 
            this.radioButton_OSA.AutoSize = true;
            this.radioButton_OSA.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_OSA.ForeColor = System.Drawing.Color.White;
            this.radioButton_OSA.Location = new System.Drawing.Point(16, 211);
            this.radioButton_OSA.Name = "radioButton_OSA";
            this.radioButton_OSA.Size = new System.Drawing.Size(67, 25);
            this.radioButton_OSA.TabIndex = 4;
            this.radioButton_OSA.TabStop = true;
            this.radioButton_OSA.Text = "OSA";
            this.radioButton_OSA.UseVisualStyleBackColor = true;
            this.radioButton_OSA.CheckedChanged += new System.EventHandler(this.radioButton6_CheckedChanged);
            // 
            // radioButton_PS
            // 
            this.radioButton_PS.AutoSize = true;
            this.radioButton_PS.Font = new System.Drawing.Font("新細明體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_PS.ForeColor = System.Drawing.Color.White;
            this.radioButton_PS.Location = new System.Drawing.Point(16, 180);
            this.radioButton_PS.Name = "radioButton_PS";
            this.radioButton_PS.Size = new System.Drawing.Size(123, 25);
            this.radioButton_PS.TabIndex = 3;
            this.radioButton_PS.TabStop = true;
            this.radioButton_PS.Text = "SEVERITY";
            this.radioButton_PS.UseVisualStyleBackColor = true;
            this.radioButton_PS.CheckedChanged += new System.EventHandler(this.radioButton5_CheckedChanged);
            // 
            // radioButton_PFW4
            // 
            this.radioButton_PFW4.AutoSize = true;
            this.radioButton_PFW4.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_PFW4.ForeColor = System.Drawing.Color.White;
            this.radioButton_PFW4.Location = new System.Drawing.Point(16, 146);
            this.radioButton_PFW4.Name = "radioButton_PFW4";
            this.radioButton_PFW4.Size = new System.Drawing.Size(75, 28);
            this.radioButton_PFW4.TabIndex = 3;
            this.radioButton_PFW4.TabStop = true;
            this.radioButton_PFW4.Text = "PFW";
            this.radioButton_PFW4.UseVisualStyleBackColor = true;
            this.radioButton_PFW4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
            // 
            // radioButton_PFH3
            // 
            this.radioButton_PFH3.AutoSize = true;
            this.radioButton_PFH3.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_PFH3.ForeColor = System.Drawing.Color.White;
            this.radioButton_PFH3.Location = new System.Drawing.Point(16, 112);
            this.radioButton_PFH3.Name = "radioButton_PFH3";
            this.radioButton_PFH3.Size = new System.Drawing.Size(70, 28);
            this.radioButton_PFH3.TabIndex = 2;
            this.radioButton_PFH3.TabStop = true;
            this.radioButton_PFH3.Text = "PFH";
            this.radioButton_PFH3.UseVisualStyleBackColor = true;
            this.radioButton_PFH3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // radioButton_MRD2
            // 
            this.radioButton_MRD2.AutoSize = true;
            this.radioButton_MRD2.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_MRD2.ForeColor = System.Drawing.Color.White;
            this.radioButton_MRD2.Location = new System.Drawing.Point(16, 78);
            this.radioButton_MRD2.Name = "radioButton_MRD2";
            this.radioButton_MRD2.Size = new System.Drawing.Size(90, 28);
            this.radioButton_MRD2.TabIndex = 1;
            this.radioButton_MRD2.TabStop = true;
            this.radioButton_MRD2.Text = "MRD2";
            this.radioButton_MRD2.UseVisualStyleBackColor = true;
            this.radioButton_MRD2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton_MRD1
            // 
            this.radioButton_MRD1.AutoSize = true;
            this.radioButton_MRD1.Font = new System.Drawing.Font("新細明體", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radioButton_MRD1.ForeColor = System.Drawing.Color.White;
            this.radioButton_MRD1.Location = new System.Drawing.Point(16, 44);
            this.radioButton_MRD1.Name = "radioButton_MRD1";
            this.radioButton_MRD1.Size = new System.Drawing.Size(90, 28);
            this.radioButton_MRD1.TabIndex = 0;
            this.radioButton_MRD1.TabStop = true;
            this.radioButton_MRD1.Text = "MRD1";
            this.radioButton_MRD1.UseVisualStyleBackColor = true;
            this.radioButton_MRD1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // imageBox7
            // 
            this.imageBox7.Location = new System.Drawing.Point(890, 547);
            this.imageBox7.Name = "imageBox7";
            this.imageBox7.Size = new System.Drawing.Size(243, 121);
            this.imageBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox7.TabIndex = 2;
            this.imageBox7.TabStop = false;
            this.imageBox7.Visible = false;
            // 
            // imageBox8
            // 
            this.imageBox8.Location = new System.Drawing.Point(1155, 547);
            this.imageBox8.Name = "imageBox8";
            this.imageBox8.Size = new System.Drawing.Size(243, 121);
            this.imageBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox8.TabIndex = 2;
            this.imageBox8.TabStop = false;
            this.imageBox8.Visible = false;
            // 
            // imageBox9
            // 
            this.imageBox9.Location = new System.Drawing.Point(890, 708);
            this.imageBox9.Name = "imageBox9";
            this.imageBox9.Size = new System.Drawing.Size(243, 121);
            this.imageBox9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox9.TabIndex = 2;
            this.imageBox9.TabStop = false;
            this.imageBox9.Visible = false;
            // 
            // imageBox10
            // 
            this.imageBox10.Location = new System.Drawing.Point(1155, 708);
            this.imageBox10.Name = "imageBox10";
            this.imageBox10.Size = new System.Drawing.Size(243, 121);
            this.imageBox10.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox10.TabIndex = 2;
            this.imageBox10.TabStop = false;
            this.imageBox10.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(1370, 749);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.imageBox6);
            this.Controls.Add(this.imageBox10);
            this.Controls.Add(this.imageBox8);
            this.Controls.Add(this.imageBox9);
            this.Controls.Add(this.imageBox7);
            this.Controls.Add(this.imageBox5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.imageBox2);
            this.Controls.Add(this.imageBox1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.imageBox3);
            this.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "眼瞼下垂檢測系統";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox3)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox10)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private Emgu.CV.UI.ImageBox imageBox2;
        public Emgu.CV.UI.ImageBox imageBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Label label4;
        private Emgu.CV.UI.ImageBox imageBox5;
        private Emgu.CV.UI.ImageBox imageBox6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem18;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem19;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem20;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        public Emgu.CV.UI.ImageBox imageBox3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStripMenuItem 比例校正ToolStripMenuItem;
        public System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem12;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton radioButton_PS;
        private System.Windows.Forms.RadioButton radioButton_PFW4;
        private System.Windows.Forms.RadioButton radioButton_PFH3;
        private System.Windows.Forms.RadioButton radioButton_MRD2;
        private System.Windows.Forms.RadioButton radioButton_MRD1;
        private System.Windows.Forms.RadioButton radioButton_OSA;
        private System.Windows.Forms.RadioButton radioButtonOri;
        private Emgu.CV.UI.ImageBox imageBox7;
        private Emgu.CV.UI.ImageBox imageBox8;
        private Emgu.CV.UI.ImageBox imageBox9;
        private Emgu.CV.UI.ImageBox imageBox10;
        private System.Windows.Forms.RadioButton radioButton7;
    }
}

