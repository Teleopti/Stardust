namespace Teleopti.Support.LicTool
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
			this.txtbxCustomerName = new System.Windows.Forms.TextBox();
			this.dtpkrExpirationDate = new System.Windows.Forms.DateTimePicker();
			this.numMaxActiveAgents = new System.Windows.Forms.NumericUpDown();
			this.numMaxActiveAgentsGrace = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.numExpirationGracePeriodDays = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.btnLoadLicenseFile = new System.Windows.Forms.Button();
			this.btnClearSettings = new System.Windows.Forms.Button();
			this.btnDemoSettings = new System.Windows.Forms.Button();
			this.btnCreateAndSave = new System.Windows.Forms.Button();
			this.chkBase = new System.Windows.Forms.CheckBox();
			this.chkAgentSelfService = new System.Windows.Forms.CheckBox();
			this.chkShiftTrades = new System.Windows.Forms.CheckBox();
			this.chkAgentScheduleMessenger = new System.Windows.Forms.CheckBox();
			this.chkHolidayPlanner = new System.Windows.Forms.CheckBox();
			this.chkRealtimeAdherence = new System.Windows.Forms.CheckBox();
			this.chkPerformanceManager = new System.Windows.Forms.CheckBox();
			this.chkPayrollIntegration = new System.Windows.Forms.CheckBox();
			this.grpBoxModules = new System.Windows.Forms.GroupBox();
			this.checkBoxCalendar = new System.Windows.Forms.CheckBox();
			this.checkBoxSMS = new System.Windows.Forms.CheckBox();
			this.chkMobileReports = new System.Windows.Forms.CheckBox();
			this.chkMyTimeWeb = new System.Windows.Forms.CheckBox();
			this.chkDeveloper = new System.Windows.Forms.CheckBox();
			this.btnAdd2Yrs = new System.Windows.Forms.Button();
			this.btnAdd3Mon = new System.Windows.Forms.Button();
			this.grpbxPresets = new System.Windows.Forms.GroupBox();
			this.label7 = new System.Windows.Forms.Label();
			this.ExpirationGracePeriodHours = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.comboBoxAgreement = new System.Windows.Forms.ComboBox();
			this.menuTopmenu = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadLicenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.createAndSaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearAllFieldsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.demoSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.comboBoxAgentsOrSeats = new System.Windows.Forms.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			this.numericUpDownCountRatio = new System.Windows.Forms.NumericUpDown();
			this.labelRatio = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.chkFreemium = new System.Windows.Forms.CheckBox();
			this.groupBoxVersion8 = new System.Windows.Forms.GroupBox();
			this.checkBoxVersion8 = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numMaxActiveAgents)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaxActiveAgentsGrace)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numExpirationGracePeriodDays)).BeginInit();
			this.grpBoxModules.SuspendLayout();
			this.grpbxPresets.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExpirationGracePeriodHours)).BeginInit();
			this.menuTopmenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownCountRatio)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.groupBoxVersion8.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtbxCustomerName
			// 
			this.txtbxCustomerName.Location = new System.Drawing.Point(118, 81);
			this.txtbxCustomerName.Name = "txtbxCustomerName";
			this.txtbxCustomerName.Size = new System.Drawing.Size(370, 20);
			this.txtbxCustomerName.TabIndex = 4;
			// 
			// dtpkrExpirationDate
			// 
			this.dtpkrExpirationDate.Location = new System.Drawing.Point(118, 162);
			this.dtpkrExpirationDate.Name = "dtpkrExpirationDate";
			this.dtpkrExpirationDate.Size = new System.Drawing.Size(147, 20);
			this.dtpkrExpirationDate.TabIndex = 6;
			// 
			// numMaxActiveAgents
			// 
			this.numMaxActiveAgents.Increment = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.numMaxActiveAgents.Location = new System.Drawing.Point(119, 259);
			this.numMaxActiveAgents.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
			this.numMaxActiveAgents.Name = "numMaxActiveAgents";
			this.numMaxActiveAgents.Size = new System.Drawing.Size(51, 20);
			this.numMaxActiveAgents.TabIndex = 10;
			// 
			// numMaxActiveAgentsGrace
			// 
			this.numMaxActiveAgentsGrace.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numMaxActiveAgentsGrace.Location = new System.Drawing.Point(119, 294);
			this.numMaxActiveAgentsGrace.Name = "numMaxActiveAgentsGrace";
			this.numMaxActiveAgentsGrace.Size = new System.Drawing.Size(51, 20);
			this.numMaxActiveAgentsGrace.TabIndex = 13;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 85);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 13);
			this.label1.TabIndex = 22;
			this.label1.Text = "Customer name";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 169);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(77, 13);
			this.label2.TabIndex = 7;
			this.label2.Text = "Expiration date";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(15, 268);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(62, 13);
			this.label3.TabIndex = 8;
			this.label3.Text = "Max agents";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(16, 301);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(92, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Max agents grace";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(15, 226);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(83, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "Expiration grace";
			// 
			// numExpirationGracePeriodDays
			// 
			this.numExpirationGracePeriodDays.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
			this.numExpirationGracePeriodDays.Location = new System.Drawing.Point(118, 219);
			this.numExpirationGracePeriodDays.Maximum = new decimal(new int[] {
            365,
            0,
            0,
            0});
			this.numExpirationGracePeriodDays.Name = "numExpirationGracePeriodDays";
			this.numExpirationGracePeriodDays.Size = new System.Drawing.Size(51, 20);
			this.numExpirationGracePeriodDays.TabIndex = 8;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(175, 226);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(29, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "days";
			// 
			// btnLoadLicenseFile
			// 
			this.btnLoadLicenseFile.Location = new System.Drawing.Point(19, 39);
			this.btnLoadLicenseFile.Name = "btnLoadLicenseFile";
			this.btnLoadLicenseFile.Size = new System.Drawing.Size(97, 26);
			this.btnLoadLicenseFile.TabIndex = 1;
			this.btnLoadLicenseFile.Text = "Load license";
			this.btnLoadLicenseFile.UseVisualStyleBackColor = true;
			this.btnLoadLicenseFile.Click += new System.EventHandler(this.LoadLicenseFile_Click);
			// 
			// btnClearSettings
			// 
			this.btnClearSettings.Location = new System.Drawing.Point(299, 40);
			this.btnClearSettings.Name = "btnClearSettings";
			this.btnClearSettings.Size = new System.Drawing.Size(87, 25);
			this.btnClearSettings.TabIndex = 3;
			this.btnClearSettings.Text = "Clear";
			this.btnClearSettings.UseVisualStyleBackColor = true;
			this.btnClearSettings.Click += new System.EventHandler(this.ClearSettings_Click);
			// 
			// btnDemoSettings
			// 
			this.btnDemoSettings.Location = new System.Drawing.Point(159, 39);
			this.btnDemoSettings.Name = "btnDemoSettings";
			this.btnDemoSettings.Size = new System.Drawing.Size(93, 26);
			this.btnDemoSettings.TabIndex = 2;
			this.btnDemoSettings.Text = "Demo settings";
			this.btnDemoSettings.UseVisualStyleBackColor = true;
			this.btnDemoSettings.Click += new System.EventHandler(this.DemoSettings_Click);
			// 
			// btnCreateAndSave
			// 
			this.btnCreateAndSave.Location = new System.Drawing.Point(453, 636);
			this.btnCreateAndSave.Name = "btnCreateAndSave";
			this.btnCreateAndSave.Size = new System.Drawing.Size(99, 27);
			this.btnCreateAndSave.TabIndex = 18;
			this.btnCreateAndSave.Text = "Create and save";
			this.btnCreateAndSave.UseVisualStyleBackColor = true;
			this.btnCreateAndSave.Click += new System.EventHandler(this.CreateAndSave_Click);
			// 
			// chkBase
			// 
			this.chkBase.AutoSize = true;
			this.chkBase.Location = new System.Drawing.Point(6, 19);
			this.chkBase.Name = "chkBase";
			this.chkBase.Size = new System.Drawing.Size(50, 17);
			this.chkBase.TabIndex = 6;
			this.chkBase.Text = "Base";
			this.chkBase.UseVisualStyleBackColor = true;
			// 
			// chkAgentSelfService
			// 
			this.chkAgentSelfService.AutoSize = true;
			this.chkAgentSelfService.Location = new System.Drawing.Point(6, 41);
			this.chkAgentSelfService.Name = "chkAgentSelfService";
			this.chkAgentSelfService.Size = new System.Drawing.Size(132, 17);
			this.chkAgentSelfService.TabIndex = 7;
			this.chkAgentSelfService.Text = "Employee Self Service";
			this.chkAgentSelfService.UseVisualStyleBackColor = true;
			this.chkAgentSelfService.CheckedChanged += new System.EventHandler(this.chkAgentSelfService_CheckedChanged);
			// 
			// chkShiftTrades
			// 
			this.chkShiftTrades.AutoSize = true;
			this.chkShiftTrades.Location = new System.Drawing.Point(33, 63);
			this.chkShiftTrades.Name = "chkShiftTrades";
			this.chkShiftTrades.Size = new System.Drawing.Size(83, 17);
			this.chkShiftTrades.TabIndex = 8;
			this.chkShiftTrades.Text = "Shift Trades";
			this.chkShiftTrades.UseVisualStyleBackColor = true;
			// 
			// chkAgentScheduleMessenger
			// 
			this.chkAgentScheduleMessenger.AutoSize = true;
			this.chkAgentScheduleMessenger.Location = new System.Drawing.Point(33, 85);
			this.chkAgentScheduleMessenger.Name = "chkAgentScheduleMessenger";
			this.chkAgentScheduleMessenger.Size = new System.Drawing.Size(157, 17);
			this.chkAgentScheduleMessenger.TabIndex = 9;
			this.chkAgentScheduleMessenger.Text = "Agent Schedule Messenger";
			this.chkAgentScheduleMessenger.UseVisualStyleBackColor = true;
			// 
			// chkHolidayPlanner
			// 
			this.chkHolidayPlanner.AutoSize = true;
			this.chkHolidayPlanner.Location = new System.Drawing.Point(33, 107);
			this.chkHolidayPlanner.Name = "chkHolidayPlanner";
			this.chkHolidayPlanner.Size = new System.Drawing.Size(100, 17);
			this.chkHolidayPlanner.TabIndex = 10;
			this.chkHolidayPlanner.Text = "Holiday Planner";
			this.chkHolidayPlanner.UseVisualStyleBackColor = true;
			// 
			// chkRealtimeAdherence
			// 
			this.chkRealtimeAdherence.AutoSize = true;
			this.chkRealtimeAdherence.Location = new System.Drawing.Point(6, 129);
			this.chkRealtimeAdherence.Name = "chkRealtimeAdherence";
			this.chkRealtimeAdherence.Size = new System.Drawing.Size(122, 17);
			this.chkRealtimeAdherence.TabIndex = 11;
			this.chkRealtimeAdherence.Text = "Realtime Adherence";
			this.chkRealtimeAdherence.UseVisualStyleBackColor = true;
			// 
			// chkPerformanceManager
			// 
			this.chkPerformanceManager.AutoSize = true;
			this.chkPerformanceManager.Location = new System.Drawing.Point(6, 152);
			this.chkPerformanceManager.Name = "chkPerformanceManager";
			this.chkPerformanceManager.Size = new System.Drawing.Size(131, 17);
			this.chkPerformanceManager.TabIndex = 12;
			this.chkPerformanceManager.Text = "Performance Manager";
			this.chkPerformanceManager.UseVisualStyleBackColor = true;
			// 
			// chkPayrollIntegration
			// 
			this.chkPayrollIntegration.AutoSize = true;
			this.chkPayrollIntegration.Location = new System.Drawing.Point(6, 174);
			this.chkPayrollIntegration.Name = "chkPayrollIntegration";
			this.chkPayrollIntegration.Size = new System.Drawing.Size(110, 17);
			this.chkPayrollIntegration.TabIndex = 13;
			this.chkPayrollIntegration.Text = "Payroll Integration";
			this.chkPayrollIntegration.UseVisualStyleBackColor = true;
			// 
			// grpBoxModules
			// 
			this.grpBoxModules.Controls.Add(this.checkBoxCalendar);
			this.grpBoxModules.Controls.Add(this.checkBoxSMS);
			this.grpBoxModules.Controls.Add(this.chkMobileReports);
			this.grpBoxModules.Controls.Add(this.chkMyTimeWeb);
			this.grpBoxModules.Controls.Add(this.chkPayrollIntegration);
			this.grpBoxModules.Controls.Add(this.chkPerformanceManager);
			this.grpBoxModules.Controls.Add(this.chkRealtimeAdherence);
			this.grpBoxModules.Controls.Add(this.chkHolidayPlanner);
			this.grpBoxModules.Controls.Add(this.chkAgentScheduleMessenger);
			this.grpBoxModules.Controls.Add(this.chkShiftTrades);
			this.grpBoxModules.Controls.Add(this.chkAgentSelfService);
			this.grpBoxModules.Controls.Add(this.chkBase);
			this.grpBoxModules.Location = new System.Drawing.Point(70, 338);
			this.grpBoxModules.Name = "grpBoxModules";
			this.grpBoxModules.Size = new System.Drawing.Size(211, 280);
			this.grpBoxModules.TabIndex = 14;
			this.grpBoxModules.TabStop = false;
			this.grpBoxModules.Text = "TeleoptiCCC options";
			// 
			// checkBoxCalendar
			// 
			this.checkBoxCalendar.AutoSize = true;
			this.checkBoxCalendar.Location = new System.Drawing.Point(6, 258);
			this.checkBoxCalendar.Name = "checkBoxCalendar";
			this.checkBoxCalendar.Size = new System.Drawing.Size(91, 17);
			this.checkBoxCalendar.TabIndex = 18;
			this.checkBoxCalendar.Text = "Calendar Link";
			this.checkBoxCalendar.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.checkBoxCalendar.UseVisualStyleBackColor = true;
			// 
			// checkBoxSMS
			// 
			this.checkBoxSMS.AutoSize = true;
			this.checkBoxSMS.Location = new System.Drawing.Point(6, 237);
			this.checkBoxSMS.Name = "checkBoxSMS";
			this.checkBoxSMS.Size = new System.Drawing.Size(72, 17);
			this.checkBoxSMS.TabIndex = 17;
			this.checkBoxSMS.Text = "SMS Link";
			this.checkBoxSMS.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.checkBoxSMS.UseVisualStyleBackColor = true;
			// 
			// chkMobileReports
			// 
			this.chkMobileReports.AutoSize = true;
			this.chkMobileReports.Location = new System.Drawing.Point(6, 216);
			this.chkMobileReports.Name = "chkMobileReports";
			this.chkMobileReports.Size = new System.Drawing.Size(97, 17);
			this.chkMobileReports.TabIndex = 16;
			this.chkMobileReports.Text = "Mobile Reports";
			this.chkMobileReports.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.chkMobileReports.UseVisualStyleBackColor = true;
			// 
			// chkMyTimeWeb
			// 
			this.chkMyTimeWeb.AutoSize = true;
			this.chkMyTimeWeb.Location = new System.Drawing.Point(6, 196);
			this.chkMyTimeWeb.Name = "chkMyTimeWeb";
			this.chkMyTimeWeb.Size = new System.Drawing.Size(89, 17);
			this.chkMyTimeWeb.TabIndex = 15;
			this.chkMyTimeWeb.Text = "MyTime Web";
			this.chkMyTimeWeb.UseVisualStyleBackColor = true;
			// 
			// chkDeveloper
			// 
			this.chkDeveloper.AutoSize = true;
			this.chkDeveloper.Location = new System.Drawing.Point(76, 624);
			this.chkDeveloper.Name = "chkDeveloper";
			this.chkDeveloper.Size = new System.Drawing.Size(75, 17);
			this.chkDeveloper.TabIndex = 15;
			this.chkDeveloper.Text = "Developer";
			this.chkDeveloper.UseVisualStyleBackColor = true;
			// 
			// btnAdd2Yrs
			// 
			this.btnAdd2Yrs.Location = new System.Drawing.Point(179, 22);
			this.btnAdd2Yrs.Name = "btnAdd2Yrs";
			this.btnAdd2Yrs.Size = new System.Drawing.Size(121, 23);
			this.btnAdd2Yrs.TabIndex = 1;
			this.btnAdd2Yrs.Text = "Extend 2 years";
			this.btnAdd2Yrs.UseVisualStyleBackColor = true;
			this.btnAdd2Yrs.Click += new System.EventHandler(this.btnAdd2Yrs_Click);
			// 
			// btnAdd3Mon
			// 
			this.btnAdd3Mon.Location = new System.Drawing.Point(28, 22);
			this.btnAdd3Mon.Name = "btnAdd3Mon";
			this.btnAdd3Mon.Size = new System.Drawing.Size(132, 23);
			this.btnAdd3Mon.TabIndex = 1;
			this.btnAdd3Mon.Text = "Extend 3 months";
			this.btnAdd3Mon.UseVisualStyleBackColor = true;
			this.btnAdd3Mon.Click += new System.EventHandler(this.btnAdd3Mon_Click);
			// 
			// grpbxPresets
			// 
			this.grpbxPresets.Controls.Add(this.btnAdd3Mon);
			this.grpbxPresets.Controls.Add(this.btnAdd2Yrs);
			this.grpbxPresets.Location = new System.Drawing.Point(271, 141);
			this.grpbxPresets.Name = "grpbxPresets";
			this.grpbxPresets.Size = new System.Drawing.Size(336, 66);
			this.grpbxPresets.TabIndex = 7;
			this.grpbxPresets.TabStop = false;
			this.grpbxPresets.Text = "Preset expiration dates";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(176, 301);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(43, 13);
			this.label7.TabIndex = 31;
			this.label7.Text = "percent";
			// 
			// ExpirationGracePeriodHours
			// 
			this.ExpirationGracePeriodHours.Location = new System.Drawing.Point(210, 219);
			this.ExpirationGracePeriodHours.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
			this.ExpirationGracePeriodHours.Name = "ExpirationGracePeriodHours";
			this.ExpirationGracePeriodHours.Size = new System.Drawing.Size(42, 20);
			this.ExpirationGracePeriodHours.TabIndex = 9;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(258, 226);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(33, 13);
			this.label8.TabIndex = 33;
			this.label8.Text = "hours";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(15, 113);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(97, 13);
			this.label9.TabIndex = 34;
			this.label9.Text = "License agreement";
			// 
			// comboBoxAgreement
			// 
			this.comboBoxAgreement.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAgreement.FormattingEnabled = true;
			this.comboBoxAgreement.Location = new System.Drawing.Point(119, 108);
			this.comboBoxAgreement.Name = "comboBoxAgreement";
			this.comboBoxAgreement.Size = new System.Drawing.Size(369, 21);
			this.comboBoxAgreement.TabIndex = 5;
			// 
			// menuTopmenu
			// 
			this.menuTopmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.menuTopmenu.Location = new System.Drawing.Point(0, 0);
			this.menuTopmenu.Name = "menuTopmenu";
			this.menuTopmenu.Size = new System.Drawing.Size(639, 24);
			this.menuTopmenu.TabIndex = 36;
			this.menuTopmenu.Text = "Top Menu";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadLicenseToolStripMenuItem,
            this.createAndSaveToolStripMenuItem,
            this.clearAllFieldsToolStripMenuItem,
            this.quitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// loadLicenseToolStripMenuItem
			// 
			this.loadLicenseToolStripMenuItem.Name = "loadLicenseToolStripMenuItem";
			this.loadLicenseToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.loadLicenseToolStripMenuItem.Text = "Load license";
			this.loadLicenseToolStripMenuItem.Click += new System.EventHandler(this.loadLicenseToolStripMenuItem_Click);
			// 
			// createAndSaveToolStripMenuItem
			// 
			this.createAndSaveToolStripMenuItem.Name = "createAndSaveToolStripMenuItem";
			this.createAndSaveToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.createAndSaveToolStripMenuItem.Text = "Create and save";
			this.createAndSaveToolStripMenuItem.Click += new System.EventHandler(this.createAndSaveToolStripMenuItem_Click);
			// 
			// clearAllFieldsToolStripMenuItem
			// 
			this.clearAllFieldsToolStripMenuItem.Name = "clearAllFieldsToolStripMenuItem";
			this.clearAllFieldsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.clearAllFieldsToolStripMenuItem.Text = "Clear all settings";
			this.clearAllFieldsToolStripMenuItem.Click += new System.EventHandler(this.clearAllFieldsToolStripMenuItem_Click);
			// 
			// quitToolStripMenuItem
			// 
			this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
			this.quitToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
			this.quitToolStripMenuItem.Text = "Quit";
			this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.demoSettingsToolStripMenuItem});
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
			this.editToolStripMenuItem.Text = "Presets";
			// 
			// demoSettingsToolStripMenuItem
			// 
			this.demoSettingsToolStripMenuItem.Name = "demoSettingsToolStripMenuItem";
			this.demoSettingsToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
			this.demoSettingsToolStripMenuItem.Text = "Demo settings";
			this.demoSettingsToolStripMenuItem.Click += new System.EventHandler(this.demoSettingsToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			this.helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			this.aboutToolStripMenuItem.Text = "About";
			// 
			// comboBoxAgentsOrSeats
			// 
			this.comboBoxAgentsOrSeats.FormattingEnabled = true;
			this.comboBoxAgentsOrSeats.Items.AddRange(new object[] {
            "Agents",
            "Seats"});
			this.comboBoxAgentsOrSeats.Location = new System.Drawing.Point(291, 260);
			this.comboBoxAgentsOrSeats.Name = "comboBoxAgentsOrSeats";
			this.comboBoxAgentsOrSeats.Size = new System.Drawing.Size(121, 21);
			this.comboBoxAgentsOrSeats.TabIndex = 11;
			this.comboBoxAgentsOrSeats.SelectedIndexChanged += new System.EventHandler(this.comboBoxAgentsOrSeats_SelectedIndexChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(188, 267);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(97, 13);
			this.label10.TabIndex = 38;
			this.label10.Text = "License count type";
			// 
			// numericUpDownCountRatio
			// 
			this.numericUpDownCountRatio.DecimalPlaces = 2;
			this.numericUpDownCountRatio.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDownCountRatio.Location = new System.Drawing.Point(462, 260);
			this.numericUpDownCountRatio.Maximum = new decimal(new int[] {
            499,
            0,
            0,
            131072});
			this.numericUpDownCountRatio.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownCountRatio.Name = "numericUpDownCountRatio";
			this.numericUpDownCountRatio.Size = new System.Drawing.Size(51, 20);
			this.numericUpDownCountRatio.TabIndex = 12;
			this.numericUpDownCountRatio.Value = new decimal(new int[] {
            15,
            0,
            0,
            65536});
			// 
			// labelRatio
			// 
			this.labelRatio.AutoSize = true;
			this.labelRatio.Location = new System.Drawing.Point(424, 267);
			this.labelRatio.Name = "labelRatio";
			this.labelRatio.Size = new System.Drawing.Size(32, 13);
			this.labelRatio.TabIndex = 40;
			this.labelRatio.Text = "Ratio";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.chkFreemium);
			this.groupBox1.Location = new System.Drawing.Point(352, 338);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(200, 130);
			this.groupBox1.TabIndex = 16;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Freemium";
			// 
			// chkFreemium
			// 
			this.chkFreemium.AutoSize = true;
			this.chkFreemium.Location = new System.Drawing.Point(8, 19);
			this.chkFreemium.Name = "chkFreemium";
			this.chkFreemium.Size = new System.Drawing.Size(71, 17);
			this.chkFreemium.TabIndex = 7;
			this.chkFreemium.Text = "Freemium";
			this.chkFreemium.UseVisualStyleBackColor = true;
			this.chkFreemium.CheckedChanged += new System.EventHandler(this.chkFreemium_CheckedChanged);
			// 
			// groupBoxVersion8
			// 
			this.groupBoxVersion8.Controls.Add(this.checkBoxVersion8);
			this.groupBoxVersion8.Location = new System.Drawing.Point(352, 484);
			this.groupBoxVersion8.Name = "groupBoxVersion8";
			this.groupBoxVersion8.Size = new System.Drawing.Size(200, 130);
			this.groupBoxVersion8.TabIndex = 17;
			this.groupBoxVersion8.TabStop = false;
			this.groupBoxVersion8.Text = "Version 8";
			// 
			// checkBoxVersion8
			// 
			this.checkBoxVersion8.AutoSize = true;
			this.checkBoxVersion8.Location = new System.Drawing.Point(8, 19);
			this.checkBoxVersion8.Name = "checkBoxVersion8";
			this.checkBoxVersion8.Size = new System.Drawing.Size(70, 17);
			this.checkBoxVersion8.TabIndex = 7;
			this.checkBoxVersion8.Text = "Version 8";
			this.checkBoxVersion8.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(639, 682);
			this.Controls.Add(this.groupBoxVersion8);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelRatio);
			this.Controls.Add(this.numericUpDownCountRatio);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.comboBoxAgentsOrSeats);
			this.Controls.Add(this.chkDeveloper);
			this.Controls.Add(this.comboBoxAgreement);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.ExpirationGracePeriodHours);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.grpbxPresets);
			this.Controls.Add(this.grpBoxModules);
			this.Controls.Add(this.btnCreateAndSave);
			this.Controls.Add(this.btnDemoSettings);
			this.Controls.Add(this.btnClearSettings);
			this.Controls.Add(this.btnLoadLicenseFile);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.numExpirationGracePeriodDays);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numMaxActiveAgentsGrace);
			this.Controls.Add(this.numMaxActiveAgents);
			this.Controls.Add(this.dtpkrExpirationDate);
			this.Controls.Add(this.txtbxCustomerName);
			this.Controls.Add(this.menuTopmenu);
			this.MainMenuStrip = this.menuTopmenu;
			this.Name = "MainForm";
			this.Text = "CCC v.7 License Tool";
			((System.ComponentModel.ISupportInitialize)(this.numMaxActiveAgents)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numMaxActiveAgentsGrace)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numExpirationGracePeriodDays)).EndInit();
			this.grpBoxModules.ResumeLayout(false);
			this.grpBoxModules.PerformLayout();
			this.grpbxPresets.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ExpirationGracePeriodHours)).EndInit();
			this.menuTopmenu.ResumeLayout(false);
			this.menuTopmenu.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownCountRatio)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBoxVersion8.ResumeLayout(false);
			this.groupBoxVersion8.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtbxCustomerName;
		private System.Windows.Forms.DateTimePicker dtpkrExpirationDate;
		private System.Windows.Forms.NumericUpDown numMaxActiveAgents;
		private System.Windows.Forms.NumericUpDown numMaxActiveAgentsGrace;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numExpirationGracePeriodDays;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnLoadLicenseFile;
		private System.Windows.Forms.Button btnClearSettings;
		private System.Windows.Forms.Button btnDemoSettings;
		private System.Windows.Forms.Button btnCreateAndSave;
		private System.Windows.Forms.CheckBox chkBase;
		private System.Windows.Forms.CheckBox chkAgentSelfService;
		private System.Windows.Forms.CheckBox chkShiftTrades;
		private System.Windows.Forms.CheckBox chkAgentScheduleMessenger;
		private System.Windows.Forms.CheckBox chkHolidayPlanner;
		private System.Windows.Forms.CheckBox chkRealtimeAdherence;
		private System.Windows.Forms.CheckBox chkPerformanceManager;
		private System.Windows.Forms.CheckBox chkPayrollIntegration;
		private System.Windows.Forms.GroupBox grpBoxModules;
		private System.Windows.Forms.Button btnAdd2Yrs;
		private System.Windows.Forms.Button btnAdd3Mon;
		private System.Windows.Forms.GroupBox grpbxPresets;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown ExpirationGracePeriodHours;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox chkDeveloper;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox comboBoxAgreement;
		private System.Windows.Forms.CheckBox chkMyTimeWeb;
		private System.Windows.Forms.MenuStrip menuTopmenu;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadLicenseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem createAndSaveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearAllFieldsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem demoSettingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ComboBox comboBoxAgentsOrSeats;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.NumericUpDown numericUpDownCountRatio;
		private System.Windows.Forms.Label labelRatio;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox chkFreemium;
		private System.Windows.Forms.CheckBox chkMobileReports;
		private System.Windows.Forms.CheckBox checkBoxSMS;
		private System.Windows.Forms.GroupBox groupBoxVersion8;
		private System.Windows.Forms.CheckBox checkBoxVersion8;
		private System.Windows.Forms.CheckBox checkBoxCalendar;
	}
}

