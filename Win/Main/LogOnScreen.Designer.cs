namespace Teleopti.Ccc.Win.Main
{
    partial class LogOnScreen
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogOnScreen));
            this.panelLogin = new System.Windows.Forms.Panel();
            this.labelLogOn = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxLogOnName = new System.Windows.Forms.TextBox();
            this.labelLoginName = new System.Windows.Forms.Label();
            this.buttonLogOnCancel = new System.Windows.Forms.Button();
            this.buttonLogOnOK = new System.Windows.Forms.Button();
            this.pictureBoxStep1 = new System.Windows.Forms.PictureBox();
            this.panelChooseDataSource = new System.Windows.Forms.Panel();
            this.tabControlChooseDataSource = new System.Windows.Forms.TabControl();
            this.tabPageWindowsDataSources = new System.Windows.Forms.TabPage();
            this.listBoxWindowsDataSources = new System.Windows.Forms.ListBox();
            this.tabPageApplicationDataSources = new System.Windows.Forms.TabPage();
            this.listBoxApplicationDataSources = new System.Windows.Forms.ListBox();
            this.labelChooseDataSource = new System.Windows.Forms.Label();
            this.buttonDataSourcesListCancel = new System.Windows.Forms.Button();
            this.buttonDataSourceListOK = new System.Windows.Forms.Button();
            this.panelChooseBusinessUnit = new System.Windows.Forms.Panel();
            this.listBoxBusinessUnits = new System.Windows.Forms.ListBox();
            this.labelChooseBusinessUnit = new System.Windows.Forms.Label();
            this.buttonBusinessUnitsCancel = new System.Windows.Forms.Button();
            this.buttonBusinessUnitsOK = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelStatusText = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panelPicture = new System.Windows.Forms.Panel();
            this.pictureBoxStep3 = new System.Windows.Forms.PictureBox();
            this.pictureBoxStep2 = new System.Windows.Forms.PictureBox();
            this.panelLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep1)).BeginInit();
            this.panelChooseDataSource.SuspendLayout();
            this.tabControlChooseDataSource.SuspendLayout();
            this.tabPageWindowsDataSources.SuspendLayout();
            this.tabPageApplicationDataSources.SuspendLayout();
            this.panelChooseBusinessUnit.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panelPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep2)).BeginInit();
            this.SuspendLayout();
            // 
            // panelLogin
            // 
            this.panelLogin.BackColor = System.Drawing.Color.White;
            this.panelLogin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLogin.Controls.Add(this.labelLogOn);
            this.panelLogin.Controls.Add(this.textBoxPassword);
            this.panelLogin.Controls.Add(this.labelPassword);
            this.panelLogin.Controls.Add(this.textBoxLogOnName);
            this.panelLogin.Controls.Add(this.labelLoginName);
            this.panelLogin.Controls.Add(this.buttonLogOnCancel);
            this.panelLogin.Controls.Add(this.buttonLogOnOK);
            this.panelLogin.Location = new System.Drawing.Point(63, 31);
            this.panelLogin.Name = "panelLogin";
            this.panelLogin.Size = new System.Drawing.Size(249, 199);
            this.panelLogin.TabIndex = 31;
            this.panelLogin.Visible = false;
            // 
            // labelLogOn
            // 
            this.labelLogOn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLogOn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLogOn.Location = new System.Drawing.Point(31, 25);
            this.labelLogOn.Name = "labelLogOn";
            this.labelLogOn.Size = new System.Drawing.Size(175, 23);
            this.labelLogOn.TabIndex = 29;
            this.labelLogOn.Text = "xxPlease enter your logon credentials";
            this.labelLogOn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassword.Location = new System.Drawing.Point(151, 105);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(29, 20);
            this.textBoxPassword.TabIndex = 1;
            // 
            // labelPassword
            // 
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(60, 108);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(66, 13);
            this.labelPassword.TabIndex = 27;
            this.labelPassword.Text = "xxPassword:";
            // 
            // textBoxLogOnName
            // 
            this.textBoxLogOnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLogOnName.Location = new System.Drawing.Point(151, 79);
            this.textBoxLogOnName.Name = "textBoxLogOnName";
            this.textBoxLogOnName.Size = new System.Drawing.Size(29, 20);
            this.textBoxLogOnName.TabIndex = 0;
            // 
            // labelLoginName
            // 
            this.labelLoginName.AutoSize = true;
            this.labelLoginName.Location = new System.Drawing.Point(60, 82);
            this.labelLoginName.Name = "labelLoginName";
            this.labelLoginName.Size = new System.Drawing.Size(75, 13);
            this.labelLoginName.TabIndex = 25;
            this.labelLoginName.Text = "xxLogin name:";
            // 
            // buttonLogOnCancel
            // 
            this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonLogOnCancel.Location = new System.Drawing.Point(105, 130);
            this.buttonLogOnCancel.Name = "buttonLogOnCancel";
            this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonLogOnCancel.TabIndex = 3;
            this.buttonLogOnCancel.Text = "xxCancel";
            this.buttonLogOnCancel.UseVisualStyleBackColor = true;
            this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
            // 
            // buttonLogOnOK
            // 
            this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogOnOK.Location = new System.Drawing.Point(24, 130);
            this.buttonLogOnOK.Name = "buttonLogOnOK";
            this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
            this.buttonLogOnOK.TabIndex = 2;
            this.buttonLogOnOK.Text = "xxOK";
            this.buttonLogOnOK.UseVisualStyleBackColor = true;
            this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
            // 
            // pictureBoxStep1
            // 
            this.pictureBoxStep1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxStep1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStep1.Image")));
            this.pictureBoxStep1.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxStep1.Name = "pictureBoxStep1";
            this.pictureBoxStep1.Size = new System.Drawing.Size(247, 141);
            this.pictureBoxStep1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxStep1.TabIndex = 2;
            this.pictureBoxStep1.TabStop = false;
            // 
            // panelChooseDataSource
            // 
            this.panelChooseDataSource.BackColor = System.Drawing.Color.White;
            this.panelChooseDataSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelChooseDataSource.Controls.Add(this.tabControlChooseDataSource);
            this.panelChooseDataSource.Controls.Add(this.panelLogin);
            this.panelChooseDataSource.Controls.Add(this.labelChooseDataSource);
            this.panelChooseDataSource.Controls.Add(this.buttonDataSourcesListCancel);
            this.panelChooseDataSource.Controls.Add(this.buttonDataSourceListOK);
            this.panelChooseDataSource.Location = new System.Drawing.Point(6, 12);
            this.panelChooseDataSource.Name = "panelChooseDataSource";
            this.panelChooseDataSource.Size = new System.Drawing.Size(277, 199);
            this.panelChooseDataSource.TabIndex = 34;
            this.panelChooseDataSource.Visible = false;
            // 
            // tabControlChooseDataSource
            // 
            this.tabControlChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlChooseDataSource.Controls.Add(this.tabPageWindowsDataSources);
            this.tabControlChooseDataSource.Controls.Add(this.tabPageApplicationDataSources);
            this.tabControlChooseDataSource.Location = new System.Drawing.Point(63, 51);
            this.tabControlChooseDataSource.Name = "tabControlChooseDataSource";
            this.tabControlChooseDataSource.SelectedIndex = 0;
            this.tabControlChooseDataSource.Size = new System.Drawing.Size(150, 74);
            this.tabControlChooseDataSource.TabIndex = 33;
            this.tabControlChooseDataSource.SelectedIndexChanged += new System.EventHandler(this.tabControlChooseDataSource_SelectedIndexChanged);
            // 
            // tabPageWindowsDataSources
            // 
            this.tabPageWindowsDataSources.Controls.Add(this.listBoxWindowsDataSources);
            this.tabPageWindowsDataSources.Location = new System.Drawing.Point(4, 22);
            this.tabPageWindowsDataSources.Name = "tabPageWindowsDataSources";
            this.tabPageWindowsDataSources.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWindowsDataSources.Size = new System.Drawing.Size(142, 48);
            this.tabPageWindowsDataSources.TabIndex = 0;
            this.tabPageWindowsDataSources.Text = "xxWindows logon";
            this.tabPageWindowsDataSources.UseVisualStyleBackColor = true;
            // 
            // listBoxWindowsDataSources
            // 
            this.listBoxWindowsDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxWindowsDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxWindowsDataSources.FormattingEnabled = true;
            this.listBoxWindowsDataSources.Location = new System.Drawing.Point(3, 3);
            this.listBoxWindowsDataSources.Name = "listBoxWindowsDataSources";
            this.listBoxWindowsDataSources.Size = new System.Drawing.Size(136, 42);
            this.listBoxWindowsDataSources.TabIndex = 3;
            this.listBoxWindowsDataSources.DoubleClick += new System.EventHandler(this.listBoxWindowsDataSources_DoubleClick);
            // 
            // tabPageApplicationDataSources
            // 
            this.tabPageApplicationDataSources.Controls.Add(this.listBoxApplicationDataSources);
            this.tabPageApplicationDataSources.Location = new System.Drawing.Point(4, 22);
            this.tabPageApplicationDataSources.Name = "tabPageApplicationDataSources";
            this.tabPageApplicationDataSources.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageApplicationDataSources.Size = new System.Drawing.Size(142, 48);
            this.tabPageApplicationDataSources.TabIndex = 1;
            this.tabPageApplicationDataSources.Text = "xxApplication logon";
            this.tabPageApplicationDataSources.UseVisualStyleBackColor = true;
            // 
            // listBoxApplicationDataSources
            // 
            this.listBoxApplicationDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxApplicationDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxApplicationDataSources.FormattingEnabled = true;
            this.listBoxApplicationDataSources.Location = new System.Drawing.Point(3, 3);
            this.listBoxApplicationDataSources.Name = "listBoxApplicationDataSources";
            this.listBoxApplicationDataSources.Size = new System.Drawing.Size(136, 42);
            this.listBoxApplicationDataSources.TabIndex = 4;
            this.listBoxApplicationDataSources.DoubleClick += new System.EventHandler(this.listBoxApplicationDataSources_DoubleClick);
            // 
            // labelChooseDataSource
            // 
            this.labelChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelChooseDataSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelChooseDataSource.Location = new System.Drawing.Point(31, 25);
            this.labelChooseDataSource.Name = "labelChooseDataSource";
            this.labelChooseDataSource.Size = new System.Drawing.Size(205, 23);
            this.labelChooseDataSource.TabIndex = 29;
            this.labelChooseDataSource.Text = "xxPlease choose a datasource";
            this.labelChooseDataSource.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonDataSourcesListCancel
            // 
            this.buttonDataSourcesListCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDataSourcesListCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDataSourcesListCancel.Location = new System.Drawing.Point(133, 130);
            this.buttonDataSourcesListCancel.Name = "buttonDataSourcesListCancel";
            this.buttonDataSourcesListCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonDataSourcesListCancel.TabIndex = 2;
            this.buttonDataSourcesListCancel.Text = "xxCancel";
            this.buttonDataSourcesListCancel.UseVisualStyleBackColor = true;
            this.buttonDataSourcesListCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
            // 
            // buttonDataSourceListOK
            // 
            this.buttonDataSourceListOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDataSourceListOK.Location = new System.Drawing.Point(52, 130);
            this.buttonDataSourceListOK.Name = "buttonDataSourceListOK";
            this.buttonDataSourceListOK.Size = new System.Drawing.Size(75, 23);
            this.buttonDataSourceListOK.TabIndex = 1;
            this.buttonDataSourceListOK.Text = "xxOK";
            this.buttonDataSourceListOK.UseVisualStyleBackColor = true;
            this.buttonDataSourceListOK.Click += new System.EventHandler(this.buttonDataSourcesListOK_Click);
            // 
            // panelChooseBusinessUnit
            // 
            this.panelChooseBusinessUnit.BackColor = System.Drawing.Color.White;
            this.panelChooseBusinessUnit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelChooseBusinessUnit.Controls.Add(this.listBoxBusinessUnits);
            this.panelChooseBusinessUnit.Controls.Add(this.labelChooseBusinessUnit);
            this.panelChooseBusinessUnit.Controls.Add(this.buttonBusinessUnitsCancel);
            this.panelChooseBusinessUnit.Controls.Add(this.buttonBusinessUnitsOK);
            this.panelChooseBusinessUnit.Location = new System.Drawing.Point(393, 112);
            this.panelChooseBusinessUnit.Name = "panelChooseBusinessUnit";
            this.panelChooseBusinessUnit.Size = new System.Drawing.Size(216, 176);
            this.panelChooseBusinessUnit.TabIndex = 35;
            this.panelChooseBusinessUnit.Visible = false;
            // 
            // listBoxBusinessUnits
            // 
            this.listBoxBusinessUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxBusinessUnits.FormattingEnabled = true;
            this.listBoxBusinessUnits.Location = new System.Drawing.Point(72, 74);
            this.listBoxBusinessUnits.Name = "listBoxBusinessUnits";
            this.listBoxBusinessUnits.Size = new System.Drawing.Size(75, 17);
            this.listBoxBusinessUnits.TabIndex = 32;
            // 
            // labelChooseBusinessUnit
            // 
            this.labelChooseBusinessUnit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelChooseBusinessUnit.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelChooseBusinessUnit.Location = new System.Drawing.Point(31, 25);
            this.labelChooseBusinessUnit.Name = "labelChooseBusinessUnit";
            this.labelChooseBusinessUnit.Size = new System.Drawing.Size(148, 23);
            this.labelChooseBusinessUnit.TabIndex = 29;
            this.labelChooseBusinessUnit.Text = "xxPlease choose a business unit";
            this.labelChooseBusinessUnit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonBusinessUnitsCancel
            // 
            this.buttonBusinessUnitsCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBusinessUnitsCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonBusinessUnitsCancel.Location = new System.Drawing.Point(72, 107);
            this.buttonBusinessUnitsCancel.Name = "buttonBusinessUnitsCancel";
            this.buttonBusinessUnitsCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonBusinessUnitsCancel.TabIndex = 2;
            this.buttonBusinessUnitsCancel.Text = "xxCancel";
            this.buttonBusinessUnitsCancel.UseVisualStyleBackColor = true;
            this.buttonBusinessUnitsCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
            // 
            // buttonBusinessUnitsOK
            // 
            this.buttonBusinessUnitsOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBusinessUnitsOK.Location = new System.Drawing.Point(-9, 107);
            this.buttonBusinessUnitsOK.Name = "buttonBusinessUnitsOK";
            this.buttonBusinessUnitsOK.Size = new System.Drawing.Size(75, 23);
            this.buttonBusinessUnitsOK.TabIndex = 1;
            this.buttonBusinessUnitsOK.Text = "xxOK";
            this.buttonBusinessUnitsOK.UseVisualStyleBackColor = true;
            this.buttonBusinessUnitsOK.Click += new System.EventHandler(this.buttonBusinessUnitsOK_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 297F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(490, 337);
            this.tableLayoutPanel1.TabIndex = 36;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.labelStatusText);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 300);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(484, 34);
            this.panel2.TabIndex = 37;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Courier New", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(484, 10);
            this.label1.TabIndex = 39;
            this.label1.Text = "Build";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelStatusText
            // 
            this.labelStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusText.ForeColor = System.Drawing.Color.Orange;
            this.labelStatusText.Location = new System.Drawing.Point(3, 5);
            this.labelStatusText.Name = "labelStatusText";
            this.labelStatusText.Size = new System.Drawing.Size(478, 25);
            this.labelStatusText.TabIndex = 40;
            this.labelStatusText.Text = "xxSearching for data sources...";
            this.labelStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panelPicture);
            this.panel1.Controls.Add(this.panelChooseBusinessUnit);
            this.panel1.Controls.Add(this.panelChooseDataSource);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(484, 291);
            this.panel1.TabIndex = 0;
            // 
            // panelPicture
            // 
            this.panelPicture.Controls.Add(this.pictureBoxStep3);
            this.panelPicture.Controls.Add(this.pictureBoxStep2);
            this.panelPicture.Controls.Add(this.pictureBoxStep1);
            this.panelPicture.Location = new System.Drawing.Point(198, 12);
            this.panelPicture.Name = "panelPicture";
            this.panelPicture.Size = new System.Drawing.Size(247, 141);
            this.panelPicture.TabIndex = 36;
            // 
            // pictureBoxStep3
            // 
            this.pictureBoxStep3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStep3.Image")));
            this.pictureBoxStep3.Location = new System.Drawing.Point(222, 67);
            this.pictureBoxStep3.Name = "pictureBoxStep3";
            this.pictureBoxStep3.Size = new System.Drawing.Size(144, 74);
            this.pictureBoxStep3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxStep3.TabIndex = 38;
            this.pictureBoxStep3.TabStop = false;
            this.pictureBoxStep3.Visible = false;
            // 
            // pictureBoxStep2
            // 
            this.pictureBoxStep2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStep2.Image")));
            this.pictureBoxStep2.Location = new System.Drawing.Point(28, 20);
            this.pictureBoxStep2.Name = "pictureBoxStep2";
            this.pictureBoxStep2.Size = new System.Drawing.Size(144, 74);
            this.pictureBoxStep2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxStep2.TabIndex = 37;
            this.pictureBoxStep2.TabStop = false;
            this.pictureBoxStep2.Visible = false;
            // 
            // LogOnScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightBlue;
            this.ClientSize = new System.Drawing.Size(490, 337);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LogOnScreen";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LogOnScreen";
            this.panelLogin.ResumeLayout(false);
            this.panelLogin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep1)).EndInit();
            this.panelChooseDataSource.ResumeLayout(false);
            this.tabControlChooseDataSource.ResumeLayout(false);
            this.tabPageWindowsDataSources.ResumeLayout(false);
            this.tabPageApplicationDataSources.ResumeLayout(false);
            this.panelChooseBusinessUnit.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panelPicture.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLogin;
        private System.Windows.Forms.Label labelLogOn;
        internal System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        internal System.Windows.Forms.TextBox textBoxLogOnName;
        private System.Windows.Forms.Label labelLoginName;
        private System.Windows.Forms.Button buttonLogOnCancel;
        private System.Windows.Forms.Button buttonLogOnOK;
        private System.Windows.Forms.PictureBox pictureBoxStep1;
        private System.Windows.Forms.Panel panelChooseDataSource;
        private System.Windows.Forms.Label labelChooseDataSource;
        private System.Windows.Forms.Button buttonDataSourcesListCancel;
        private System.Windows.Forms.Button buttonDataSourceListOK;
        private System.Windows.Forms.Panel panelChooseBusinessUnit;
        private System.Windows.Forms.ListBox listBoxBusinessUnits;
        private System.Windows.Forms.Label labelChooseBusinessUnit;
        private System.Windows.Forms.Button buttonBusinessUnitsCancel;
        private System.Windows.Forms.Button buttonBusinessUnitsOK;
        private System.Windows.Forms.TabControl tabControlChooseDataSource;
        private System.Windows.Forms.TabPage tabPageWindowsDataSources;
        private System.Windows.Forms.ListBox listBoxWindowsDataSources;
        private System.Windows.Forms.TabPage tabPageApplicationDataSources;
        private System.Windows.Forms.ListBox listBoxApplicationDataSources;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelPicture;
        private System.Windows.Forms.PictureBox pictureBoxStep2;
        private System.Windows.Forms.PictureBox pictureBoxStep3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelStatusText;
        private System.Windows.Forms.Label label1;

    }
}