namespace CheckPreRequisites
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
			this.btnCheck2008 = new System.Windows.Forms.Button();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.button1 = new System.Windows.Forms.Button();
			this.buttonDBconnect = new System.Windows.Forms.Button();
			this.groupBoxDBConnectivity = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SQLPwd = new System.Windows.Forms.TextBox();
			this.SQLLogin = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.radioButtonSQLLogin = new System.Windows.Forms.RadioButton();
			this.radioButtonWinAuth = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxSQLServerName = new System.Windows.Forms.TextBox();
			this.groupBoxNumberOfAgents = new System.Windows.Forms.GroupBox();
			this.numericUpDownAgents = new System.Windows.Forms.NumericUpDown();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.comboBoxServerSetup = new System.Windows.Forms.ComboBox();
			this.groupBoxSQLInstance = new System.Windows.Forms.GroupBox();
			this.comboBoxSQLInstance = new System.Windows.Forms.ComboBox();
			this.groupBoxDatabase = new System.Windows.Forms.GroupBox();
			this.textBoxDBName = new System.Windows.Forms.TextBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.listView1 = new System.Windows.Forms.ListView();
			this.cArea = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.cType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.cValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.cMinRequirement = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.cIsOK = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.labelInfo = new System.Windows.Forms.Label();
			this.contextMenuStrip1.SuspendLayout();
			this.groupBoxDBConnectivity.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBoxNumberOfAgents.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownAgents)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.groupBoxSQLInstance.SuspendLayout();
			this.groupBoxDatabase.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnCheck2008
			// 
			this.btnCheck2008.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.btnCheck2008.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnCheck2008.ForeColor = System.Drawing.Color.White;
			this.btnCheck2008.Location = new System.Drawing.Point(439, 107);
			this.btnCheck2008.Name = "btnCheck2008";
			this.btnCheck2008.Size = new System.Drawing.Size(66, 31);
			this.btnCheck2008.TabIndex = 0;
			this.btnCheck2008.Text = "Check";
			this.btnCheck2008.UseVisualStyleBackColor = false;
			this.btnCheck2008.Click += new System.EventHandler(this.button1_Click);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCopy});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(145, 26);
			// 
			// menuCopy
			// 
			this.menuCopy.Name = "menuCopy";
			this.menuCopy.ShortcutKeyDisplayString = "";
			this.menuCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.menuCopy.Size = new System.Drawing.Size(144, 22);
			this.menuCopy.Text = "Copy";
			this.menuCopy.Click += new System.EventHandler(this.contextMenuStrip1_Click);
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.button1.ForeColor = System.Drawing.Color.White;
			this.button1.Location = new System.Drawing.Point(844, 740);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(140, 50);
			this.button1.TabIndex = 6;
			this.button1.Text = "Copy";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.contextMenuStrip1_Click);
			// 
			// buttonDBconnect
			// 
			this.buttonDBconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonDBconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonDBconnect.ForeColor = System.Drawing.Color.White;
			this.buttonDBconnect.Location = new System.Drawing.Point(301, 107);
			this.buttonDBconnect.Name = "buttonDBconnect";
			this.buttonDBconnect.Size = new System.Drawing.Size(64, 32);
			this.buttonDBconnect.TabIndex = 8;
			this.buttonDBconnect.Text = "Connect";
			this.buttonDBconnect.UseVisualStyleBackColor = false;
			this.buttonDBconnect.Click += new System.EventHandler(this.CheckDbConnection_Click);
			// 
			// groupBoxDBConnectivity
			// 
			this.groupBoxDBConnectivity.Controls.Add(this.groupBox2);
			this.groupBoxDBConnectivity.Controls.Add(this.label2);
			this.groupBoxDBConnectivity.Controls.Add(this.radioButtonSQLLogin);
			this.groupBoxDBConnectivity.Controls.Add(this.radioButtonWinAuth);
			this.groupBoxDBConnectivity.Controls.Add(this.label1);
			this.groupBoxDBConnectivity.Controls.Add(this.textBoxSQLServerName);
			this.groupBoxDBConnectivity.Controls.Add(this.buttonDBconnect);
			this.groupBoxDBConnectivity.Location = new System.Drawing.Point(610, 11);
			this.groupBoxDBConnectivity.Name = "groupBoxDBConnectivity";
			this.groupBoxDBConnectivity.Size = new System.Drawing.Size(378, 153);
			this.groupBoxDBConnectivity.TabIndex = 9;
			this.groupBoxDBConnectivity.TabStop = false;
			this.groupBoxDBConnectivity.Text = "Web to DB Connectivity";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.SQLPwd);
			this.groupBox2.Controls.Add(this.SQLLogin);
			this.groupBox2.Location = new System.Drawing.Point(14, 73);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(258, 74);
			this.groupBox2.TabIndex = 18;
			this.groupBox2.TabStop = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(14, 46);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 15);
			this.label4.TabIndex = 17;
			this.label4.Text = "Password:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(14, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 15);
			this.label3.TabIndex = 16;
			this.label3.Text = "Login:";
			// 
			// SQLPwd
			// 
			this.SQLPwd.Enabled = false;
			this.SQLPwd.Location = new System.Drawing.Point(80, 43);
			this.SQLPwd.Name = "SQLPwd";
			this.SQLPwd.PasswordChar = '*';
			this.SQLPwd.Size = new System.Drawing.Size(115, 23);
			this.SQLPwd.TabIndex = 15;
			this.SQLPwd.Text = "cadadi";
			// 
			// SQLLogin
			// 
			this.SQLLogin.Enabled = false;
			this.SQLLogin.Location = new System.Drawing.Point(80, 13);
			this.SQLLogin.Name = "SQLLogin";
			this.SQLLogin.Size = new System.Drawing.Size(115, 23);
			this.SQLLogin.TabIndex = 14;
			this.SQLLogin.Text = "sa";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(89, 15);
			this.label2.TabIndex = 13;
			this.label2.Text = "Authentication:";
			// 
			// radioButtonSQLLogin
			// 
			this.radioButtonSQLLogin.AutoSize = true;
			this.radioButtonSQLLogin.Location = new System.Drawing.Point(205, 52);
			this.radioButtonSQLLogin.Name = "radioButtonSQLLogin";
			this.radioButtonSQLLogin.Size = new System.Drawing.Size(79, 19);
			this.radioButtonSQLLogin.TabIndex = 12;
			this.radioButtonSQLLogin.Text = "SQL Login";
			this.radioButtonSQLLogin.UseVisualStyleBackColor = true;
			this.radioButtonSQLLogin.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
			// 
			// radioButtonWinAuth
			// 
			this.radioButtonWinAuth.AutoSize = true;
			this.radioButtonWinAuth.Checked = true;
			this.radioButtonWinAuth.Location = new System.Drawing.Point(112, 52);
			this.radioButtonWinAuth.Name = "radioButtonWinAuth";
			this.radioButtonWinAuth.Size = new System.Drawing.Size(72, 19);
			this.radioButtonWinAuth.TabIndex = 11;
			this.radioButtonWinAuth.TabStop = true;
			this.radioButtonWinAuth.Text = "WinAuth";
			this.radioButtonWinAuth.UseVisualStyleBackColor = true;
			this.radioButtonWinAuth.CheckedChanged += new System.EventHandler(this.radioButtonWniAuth);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 15);
			this.label1.TabIndex = 10;
			this.label1.Text = "Server name:";
			// 
			// textBoxSQLServerName
			// 
			this.textBoxSQLServerName.Location = new System.Drawing.Point(110, 22);
			this.textBoxSQLServerName.Name = "textBoxSQLServerName";
			this.textBoxSQLServerName.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
			this.textBoxSQLServerName.Size = new System.Drawing.Size(160, 23);
			this.textBoxSQLServerName.TabIndex = 9;
			// 
			// groupBoxNumberOfAgents
			// 
			this.groupBoxNumberOfAgents.Controls.Add(this.numericUpDownAgents);
			this.groupBoxNumberOfAgents.Location = new System.Drawing.Point(5, 29);
			this.groupBoxNumberOfAgents.Name = "groupBoxNumberOfAgents";
			this.groupBoxNumberOfAgents.Size = new System.Drawing.Size(177, 60);
			this.groupBoxNumberOfAgents.TabIndex = 4;
			this.groupBoxNumberOfAgents.TabStop = false;
			this.groupBoxNumberOfAgents.Text = "Number of Agents";
			// 
			// numericUpDownAgents
			// 
			this.numericUpDownAgents.AccessibleDescription = "Number of Agents";
			this.numericUpDownAgents.AccessibleName = "Agents";
			this.numericUpDownAgents.Location = new System.Drawing.Point(10, 23);
			this.numericUpDownAgents.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
			this.numericUpDownAgents.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownAgents.Name = "numericUpDownAgents";
			this.numericUpDownAgents.Size = new System.Drawing.Size(147, 23);
			this.numericUpDownAgents.TabIndex = 3;
			this.numericUpDownAgents.Value = new decimal(new int[] {
            4000,
            0,
            0,
            0});
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.comboBoxServerSetup);
			this.groupBox3.Location = new System.Drawing.Point(191, 30);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(157, 59);
			this.groupBox3.TabIndex = 12;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Server Setup";
			// 
			// comboBoxServerSetup
			// 
			this.comboBoxServerSetup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxServerSetup.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.comboBoxServerSetup.FormattingEnabled = true;
			this.comboBoxServerSetup.Items.AddRange(new object[] {
            "Web",
            "DB",
            "Web+DB",
			"Worker"});
			this.comboBoxServerSetup.Location = new System.Drawing.Point(7, 22);
			this.comboBoxServerSetup.Name = "comboBoxServerSetup";
			this.comboBoxServerSetup.Size = new System.Drawing.Size(140, 23);
			this.comboBoxServerSetup.TabIndex = 10;
			this.comboBoxServerSetup.SelectedIndexChanged += new System.EventHandler(this.comboBoxServerSetup_SelectedIndexChanged);
			// 
			// groupBoxSQLInstance
			// 
			this.groupBoxSQLInstance.Controls.Add(this.comboBoxSQLInstance);
			this.groupBoxSQLInstance.Location = new System.Drawing.Point(7, 92);
			this.groupBoxSQLInstance.Name = "groupBoxSQLInstance";
			this.groupBoxSQLInstance.Size = new System.Drawing.Size(341, 48);
			this.groupBoxSQLInstance.TabIndex = 13;
			this.groupBoxSQLInstance.TabStop = false;
			this.groupBoxSQLInstance.Text = "SQL Instance";
			// 
			// comboBoxSQLInstance
			// 
			this.comboBoxSQLInstance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSQLInstance.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.comboBoxSQLInstance.FormattingEnabled = true;
			this.comboBoxSQLInstance.Location = new System.Drawing.Point(5, 18);
			this.comboBoxSQLInstance.Name = "comboBoxSQLInstance";
			this.comboBoxSQLInstance.Size = new System.Drawing.Size(320, 23);
			this.comboBoxSQLInstance.TabIndex = 11;
			this.comboBoxSQLInstance.SelectedIndexChanged += new System.EventHandler(this.comboBoxSQLInstance_SelectedIndexChanged);
			// 
			// groupBoxDatabase
			// 
			this.groupBoxDatabase.Controls.Add(this.textBoxDBName);
			this.groupBoxDatabase.Location = new System.Drawing.Point(7, 92);
			this.groupBoxDatabase.Name = "groupBoxDatabase";
			this.groupBoxDatabase.Size = new System.Drawing.Size(341, 48);
			this.groupBoxDatabase.TabIndex = 16;
			this.groupBoxDatabase.TabStop = false;
			this.groupBoxDatabase.Text = "Database name";
			// 
			// textBoxDBName
			// 
			this.textBoxDBName.Location = new System.Drawing.Point(7, 20);
			this.textBoxDBName.Name = "textBoxDBName";
			this.textBoxDBName.Size = new System.Drawing.Size(322, 23);
			this.textBoxDBName.TabIndex = 0;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.groupBoxDatabase);
			this.groupBox5.Controls.Add(this.groupBoxSQLInstance);
			this.groupBox5.Controls.Add(this.groupBox3);
			this.groupBox5.Controls.Add(this.groupBoxNumberOfAgents);
			this.groupBox5.Controls.Add(this.btnCheck2008);
			this.groupBox5.Location = new System.Drawing.Point(14, 11);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(527, 153);
			this.groupBox5.TabIndex = 14;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Server hardware and software";
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Location = new System.Drawing.Point(14, 183);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(974, 548);
			this.tabControl1.TabIndex = 16;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.listView1);
			this.tabPage1.Location = new System.Drawing.Point(4, 24);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(966, 520);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Prerequisites";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// listView1
			// 
			this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cArea,
            this.cType,
            this.cValue,
            this.cMinRequirement,
            this.cIsOK});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.Name = "listView1";
			this.listView1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.listView1.ShowItemToolTips = true;
			this.listView1.Size = new System.Drawing.Size(966, 520);
			this.listView1.TabIndex = 6;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// cArea
			// 
			this.cArea.Text = "Area";
			this.cArea.Width = 124;
			// 
			// cType
			// 
			this.cType.Text = "Type";
			this.cType.Width = 158;
			// 
			// cValue
			// 
			this.cValue.Text = "Value";
			this.cValue.Width = 278;
			// 
			// cMinRequirement
			// 
			this.cMinRequirement.Text = "Minimum requirement";
			this.cMinRequirement.Width = 300;
			// 
			// cIsOK
			// 
			this.cIsOK.Text = "Is Ok";
			this.cIsOK.Width = 82;
			// 
			// labelInfo
			// 
			this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelInfo.AutoSize = true;
			this.labelInfo.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInfo.Location = new System.Drawing.Point(11, 740);
			this.labelInfo.Name = "labelInfo";
			this.labelInfo.Size = new System.Drawing.Size(0, 20);
			this.labelInfo.TabIndex = 17;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(997, 802);
			this.Controls.Add(this.labelInfo);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBoxDBConnectivity);
			this.Controls.Add(this.button1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "Pre requisites";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.contextMenuStrip1.ResumeLayout(false);
			this.groupBoxDBConnectivity.ResumeLayout(false);
			this.groupBoxDBConnectivity.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBoxNumberOfAgents.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownAgents)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBoxSQLInstance.ResumeLayout(false);
			this.groupBoxDatabase.ResumeLayout(false);
			this.groupBoxDatabase.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCheck2008;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuCopy;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonDBconnect;
        private System.Windows.Forms.GroupBox groupBoxDBConnectivity;
        private System.Windows.Forms.RadioButton radioButtonWinAuth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSQLServerName;
        private System.Windows.Forms.RadioButton radioButtonSQLLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox SQLPwd;
        private System.Windows.Forms.TextBox SQLLogin;
        private System.Windows.Forms.GroupBox groupBoxNumberOfAgents;
        private System.Windows.Forms.NumericUpDown numericUpDownAgents;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox comboBoxServerSetup;
        private System.Windows.Forms.GroupBox groupBoxSQLInstance;
        private System.Windows.Forms.ComboBox comboBoxSQLInstance;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBoxDatabase;
        private System.Windows.Forms.TextBox textBoxDBName;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader cArea;
        private System.Windows.Forms.ColumnHeader cType;
        private System.Windows.Forms.ColumnHeader cValue;
        private System.Windows.Forms.ColumnHeader cMinRequirement;
        private System.Windows.Forms.ColumnHeader cIsOK;
		private System.Windows.Forms.Label labelInfo;
    }
}

