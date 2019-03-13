using System.Windows.Forms;
using SdkTestClientWin.Sdk;

namespace SdkTestWinGui
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
			System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("All");
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
			this.backgroundWorkerLogon = new System.ComponentModel.BackgroundWorker();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.monthCalendar1 = new System.Windows.Forms.MonthCalendar();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageAgentInfo = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.listView2 = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderEmail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTeam = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderTimeZone = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderLanguage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderCulture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderIdentity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderApplicationLogOn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderWindowsUserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageSchedule = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.button5 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.button7 = new System.Windows.Forms.Button();
			this.button8 = new System.Windows.Forms.Button();
			this.labelScenario = new System.Windows.Forms.Label();
			this.comboBoxScenarios = new System.Windows.Forms.ComboBox();
			this.comboScheduleTag = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabPageSkillData = new System.Windows.Forms.TabPage();
			this.tabControl2 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tableLayoutPanelSkillData = new System.Windows.Forms.TableLayoutPanel();
			this.dataGridViewIntraday = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewSkillDay = new System.Windows.Forms.DataGridView();
			this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.tabPagePersonPeriod = new System.Windows.Forms.TabPage();
			this.listViewPersonPeriods = new System.Windows.Forms.ListView();
			this.columnHeaderPerson = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderStartDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colHeaderTeam = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderNote = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderContract = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderPartTimePercentage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderContractSchedule = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAcdLogOnId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAcdLogOnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageWriteProtect = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.listViewWriteProtect = new System.Windows.Forms.ListView();
			this.columnHeaderPersonName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderWriteProtectionDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panel1 = new System.Windows.Forms.Panel();
			this.buttonSetNewWriteProtectionDate = new System.Windows.Forms.Button();
			this.dateTimePickerSetProtectDate = new System.Windows.Forms.DateTimePicker();
			this.tabPagePermissions = new System.Windows.Forms.TabPage();
			this.listViewPermissionRole = new System.Windows.Forms.ListView();
			this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.listViewPermissionPerson = new System.Windows.Forms.ListView();
			this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panel2 = new System.Windows.Forms.Panel();
			this.comboBoxRoles = new System.Windows.Forms.ComboBox();
			this.buttonRevokeRole = new System.Windows.Forms.Button();
			this.buttonGrantRole = new System.Windows.Forms.Button();
			this.backgroundWorkerLoadTree = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorkerLoadSchedules = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorkerLoadSkillData = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorkerLoadPersonPeriods = new System.ComponentModel.BackgroundWorker();
			this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.personPeriodContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.cancelPersonPeriodAfterMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.button9 = new System.Windows.Forms.Button();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPageAgentInfo.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			this.tabPageSchedule.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.tableLayoutPanel3.SuspendLayout();
			this.tabPageSkillData.SuspendLayout();
			this.tabControl2.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tableLayoutPanelSkillData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewIntraday)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewSkillDay)).BeginInit();
			this.tabPagePersonPeriod.SuspendLayout();
			this.tabPageWriteProtect.SuspendLayout();
			this.tableLayoutPanel5.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPagePermissions.SuspendLayout();
			this.panel2.SuspendLayout();
			this.personPeriodContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// backgroundWorkerLogon
			// 
			this.backgroundWorkerLogon.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLogon_DoWork);
			this.backgroundWorkerLogon.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLogon_RunWorkerCompleted);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 603);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(1068, 22);
			this.statusStrip1.TabIndex = 0;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(39, 17);
			this.toolStripStatusLabel1.Text = "Ready";
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
			this.splitContainer1.Size = new System.Drawing.Size(1068, 603);
			this.splitContainer1.SplitterDistance = 236;
			this.splitContainer1.TabIndex = 1;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.treeView1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.monthCalendar1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 171F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(236, 603);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// treeView1
			// 
			this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.HideSelection = false;
			this.treeView1.Location = new System.Drawing.Point(3, 174);
			this.treeView1.Name = "treeView1";
			treeNode2.Name = "All";
			treeNode2.Text = "All";
			this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode2});
			this.treeView1.Size = new System.Drawing.Size(230, 426);
			this.treeView1.TabIndex = 1;
			this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(181, 26);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(180, 22);
			this.toolStripMenuItem2.Text = "Set Period worktime";
			this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
			// 
			// monthCalendar1
			// 
			this.monthCalendar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.monthCalendar1.Location = new System.Drawing.Point(9, 9);
			this.monthCalendar1.MaxSelectionCount = 1;
			this.monthCalendar1.Name = "monthCalendar1";
			this.monthCalendar1.TabIndex = 2;
			this.monthCalendar1.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateSelected);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPageAgentInfo);
			this.tabControl1.Controls.Add(this.tabPageSchedule);
			this.tabControl1.Controls.Add(this.tabPageSkillData);
			this.tabControl1.Controls.Add(this.tabPagePersonPeriod);
			this.tabControl1.Controls.Add(this.tabPageWriteProtect);
			this.tabControl1.Controls.Add(this.tabPagePermissions);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(828, 603);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPageAgentInfo
			// 
			this.tabPageAgentInfo.Controls.Add(this.tableLayoutPanel4);
			this.tabPageAgentInfo.Location = new System.Drawing.Point(4, 22);
			this.tabPageAgentInfo.Name = "tabPageAgentInfo";
			this.tabPageAgentInfo.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageAgentInfo.Size = new System.Drawing.Size(820, 577);
			this.tabPageAgentInfo.TabIndex = 0;
			this.tabPageAgentInfo.Text = "Agent Info";
			this.tabPageAgentInfo.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 1;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel4.Controls.Add(this.listView2, 0, 1);
			this.tableLayoutPanel4.Controls.Add(this.listView1, 0, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 3;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.54991F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.45009F));
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(814, 571);
			this.tableLayoutPanel4.TabIndex = 1;
			// 
			// listView2
			// 
			this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7});
			this.listView2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView2.FullRowSelect = true;
			this.listView2.GridLines = true;
			this.listView2.HideSelection = false;
			this.listView2.Location = new System.Drawing.Point(3, 369);
			this.listView2.Name = "listView2";
			this.listView2.Size = new System.Drawing.Size(808, 178);
			this.listView2.TabIndex = 2;
			this.listView2.UseCompatibleStateImageBehavior = false;
			this.listView2.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Tracker";
			this.columnHeader1.Width = 103;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Period";
			this.columnHeader2.Width = 140;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Balance In";
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Extra";
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Accrued";
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Used";
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "Balance Out";
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderTag,
            this.columnHeaderEmail,
            this.columnHeaderTeam,
            this.columnHeaderTimeZone,
            this.columnHeaderLanguage,
            this.columnHeaderCulture,
            this.columnHeaderIdentity,
            this.columnHeaderApplicationLogOn,
            this.columnHeaderWindowsUserName});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(3, 3);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(808, 360);
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "Name";
			// 
			// columnHeaderTag
			// 
			this.columnHeaderTag.Text = "Tag";
			// 
			// columnHeaderEmail
			// 
			this.columnHeaderEmail.Text = "Email";
			// 
			// columnHeaderTeam
			// 
			this.columnHeaderTeam.Text = "Team";
			// 
			// columnHeaderTimeZone
			// 
			this.columnHeaderTimeZone.Text = "Time zone";
			// 
			// columnHeaderLanguage
			// 
			this.columnHeaderLanguage.Text = "Language";
			// 
			// columnHeaderCulture
			// 
			this.columnHeaderCulture.Text = "Culture";
			// 
			// columnHeaderIdentity
			// 
			this.columnHeaderIdentity.Text = "Identity";
			// 
			// columnHeaderApplicationLogOn
			// 
			this.columnHeaderApplicationLogOn.Text = "App logon";
			// 
			// columnHeaderWindowsUserName
			// 
			this.columnHeaderWindowsUserName.Text = "Win logon";
			// 
			// tabPageSchedule
			// 
			this.tabPageSchedule.Controls.Add(this.tableLayoutPanel2);
			this.tabPageSchedule.Location = new System.Drawing.Point(4, 22);
			this.tabPageSchedule.Name = "tabPageSchedule";
			this.tabPageSchedule.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSchedule.Size = new System.Drawing.Size(820, 577);
			this.tabPageSchedule.TabIndex = 1;
			this.tabPageSchedule.Text = "Schedules";
			this.tabPageSchedule.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 749F));
			this.tableLayoutPanel2.Controls.Add(this.dataGridView1, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.labelScenario, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.comboBoxScenarios, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.comboScheduleTag, 1, 3);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 3);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 4;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(814, 571);
			this.tableLayoutPanel2.TabIndex = 5;
			// 
			// dataGridView1
			// 
			this.dataGridView1.AllowUserToAddRows = false;
			this.dataGridView1.AllowUserToDeleteRows = false;
			dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1});
			this.tableLayoutPanel2.SetColumnSpan(this.dataGridView1, 2);
			dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle11;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(3, 30);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.ReadOnly = true;
			dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
			this.dataGridView1.Size = new System.Drawing.Size(808, 428);
			this.dataGridView1.TabIndex = 5;
			// 
			// Column1
			// 
			this.Column1.HeaderText = "Column1";
			this.Column1.Name = "Column1";
			this.Column1.ReadOnly = true;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 3;
			this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel3, 2);
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.02167F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.97833F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 245F));
			this.tableLayoutPanel3.Controls.Add(this.button5, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.button4, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.button3, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.button2, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.button1, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.button6, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.button7, 2, 0);
			this.tableLayoutPanel3.Controls.Add(this.button8, 2, 1);
			this.tableLayoutPanel3.Controls.Add(this.button9, 2, 2);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 464);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 3;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(808, 84);
			this.tableLayoutPanel3.TabIndex = 7;
			// 
			// button5
			// 
			this.button5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button5.Location = new System.Drawing.Point(3, 59);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(224, 22);
			this.button5.TabIndex = 12;
			this.button5.Text = "Add public note for all agents on current day (* tooltip on agent name)";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// button4
			// 
			this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button4.Location = new System.Drawing.Point(3, 31);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(224, 22);
			this.button4.TabIndex = 11;
			this.button4.Text = "Add overtime where agents can have";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button3
			// 
			this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button3.Location = new System.Drawing.Point(3, 3);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(224, 22);
			this.button3.TabIndex = 10;
			this.button3.Text = "Remove all layers labeled \"Lunch\"";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button2
			// 
			this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button2.Location = new System.Drawing.Point(233, 31);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(326, 22);
			this.button2.TabIndex = 8;
			this.button2.Text = "Move all bottom layers 17 minutes earlier";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button1
			// 
			this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button1.Location = new System.Drawing.Point(233, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(326, 22);
			this.button1.TabIndex = 7;
			this.button1.Text = "Add a layer to all schedules";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button6
			// 
			this.button6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.button6.Location = new System.Drawing.Point(233, 59);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(326, 22);
			this.button6.TabIndex = 13;
			this.button6.Text = "Add day off for selected agents";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.Button6Click);
			// 
			// button7
			// 
			this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button7.Location = new System.Drawing.Point(565, 3);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(240, 22);
			this.button7.TabIndex = 14;
			this.button7.Text = "Remove personal activities with meeting Guid";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// button8
			// 
			this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button8.Location = new System.Drawing.Point(565, 31);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(240, 22);
			this.button8.TabIndex = 15;
			this.button8.Text = "Remove all personal activities";
			this.button8.UseVisualStyleBackColor = true;
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// labelScenario
			// 
			this.labelScenario.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelScenario.AutoSize = true;
			this.labelScenario.Location = new System.Drawing.Point(3, 7);
			this.labelScenario.Name = "labelScenario";
			this.labelScenario.Size = new System.Drawing.Size(52, 13);
			this.labelScenario.TabIndex = 8;
			this.labelScenario.Text = "Scenario:";
			// 
			// comboBoxScenarios
			// 
			this.comboBoxScenarios.FormattingEnabled = true;
			this.comboBoxScenarios.Location = new System.Drawing.Point(68, 3);
			this.comboBoxScenarios.Name = "comboBoxScenarios";
			this.comboBoxScenarios.Size = new System.Drawing.Size(121, 21);
			this.comboBoxScenarios.TabIndex = 9;
			this.comboBoxScenarios.SelectedIndexChanged += new System.EventHandler(this.comboBoxScenarios_SelectedIndexChanged);
			// 
			// comboScheduleTag
			// 
			this.comboScheduleTag.FormattingEnabled = true;
			this.comboScheduleTag.Location = new System.Drawing.Point(68, 554);
			this.comboScheduleTag.Name = "comboScheduleTag";
			this.comboScheduleTag.Size = new System.Drawing.Size(121, 21);
			this.comboScheduleTag.TabIndex = 10;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 551);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(51, 20);
			this.label1.TabIndex = 11;
			this.label1.Text = "Tag changes with:";
			// 
			// tabPageSkillData
			// 
			this.tabPageSkillData.Controls.Add(this.tabControl2);
			this.tabPageSkillData.Location = new System.Drawing.Point(4, 22);
			this.tabPageSkillData.Name = "tabPageSkillData";
			this.tabPageSkillData.Size = new System.Drawing.Size(844, 601);
			this.tabPageSkillData.TabIndex = 2;
			this.tabPageSkillData.Text = "SkillData";
			this.tabPageSkillData.UseVisualStyleBackColor = true;
			// 
			// tabControl2
			// 
			this.tabControl2.Controls.Add(this.tabPage1);
			this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl2.Location = new System.Drawing.Point(0, 0);
			this.tabControl2.Name = "tabControl2";
			this.tabControl2.SelectedIndex = 0;
			this.tabControl2.Size = new System.Drawing.Size(844, 601);
			this.tabControl2.TabIndex = 7;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.tableLayoutPanelSkillData);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(836, 575);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "tabPage1";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanelSkillData
			// 
			this.tableLayoutPanelSkillData.ColumnCount = 1;
			this.tableLayoutPanelSkillData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelSkillData.Controls.Add(this.dataGridViewIntraday, 0, 1);
			this.tableLayoutPanelSkillData.Controls.Add(this.dataGridViewSkillDay, 0, 0);
			this.tableLayoutPanelSkillData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelSkillData.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelSkillData.Name = "tableLayoutPanelSkillData";
			this.tableLayoutPanelSkillData.RowCount = 2;
			this.tableLayoutPanelSkillData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelSkillData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanelSkillData.Size = new System.Drawing.Size(830, 569);
			this.tableLayoutPanelSkillData.TabIndex = 8;
			this.tableLayoutPanelSkillData.Visible = false;
			// 
			// dataGridViewIntraday
			// 
			this.dataGridViewIntraday.AllowUserToAddRows = false;
			this.dataGridViewIntraday.AllowUserToDeleteRows = false;
			dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle13.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle13.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewIntraday.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle13;
			this.dataGridViewIntraday.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewIntraday.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn4});
			dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle14.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle14.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewIntraday.DefaultCellStyle = dataGridViewCellStyle14;
			this.dataGridViewIntraday.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewIntraday.Location = new System.Drawing.Point(3, 287);
			this.dataGridViewIntraday.Name = "dataGridViewIntraday";
			this.dataGridViewIntraday.ReadOnly = true;
			dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle15.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle15.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle15.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle15.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle15.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle15.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewIntraday.RowHeadersDefaultCellStyle = dataGridViewCellStyle15;
			this.dataGridViewIntraday.Size = new System.Drawing.Size(824, 279);
			this.dataGridViewIntraday.TabIndex = 9;
			// 
			// dataGridViewTextBoxColumn4
			// 
			this.dataGridViewTextBoxColumn4.HeaderText = "Column1";
			this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			this.dataGridViewTextBoxColumn4.ReadOnly = true;
			// 
			// dataGridViewSkillDay
			// 
			this.dataGridViewSkillDay.AllowUserToAddRows = false;
			this.dataGridViewSkillDay.AllowUserToDeleteRows = false;
			dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle16.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle16.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle16.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle16.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle16.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle16.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewSkillDay.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle16;
			this.dataGridViewSkillDay.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewSkillDay.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn3});
			dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle17.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle17.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle17.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle17.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridViewSkillDay.DefaultCellStyle = dataGridViewCellStyle17;
			this.dataGridViewSkillDay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridViewSkillDay.Location = new System.Drawing.Point(3, 3);
			this.dataGridViewSkillDay.Name = "dataGridViewSkillDay";
			this.dataGridViewSkillDay.ReadOnly = true;
			dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridViewSkillDay.RowHeadersDefaultCellStyle = dataGridViewCellStyle18;
			this.dataGridViewSkillDay.Size = new System.Drawing.Size(824, 278);
			this.dataGridViewSkillDay.TabIndex = 8;
			// 
			// dataGridViewTextBoxColumn3
			// 
			this.dataGridViewTextBoxColumn3.HeaderText = "Column1";
			this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			this.dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// tabPagePersonPeriod
			// 
			this.tabPagePersonPeriod.Controls.Add(this.listViewPersonPeriods);
			this.tabPagePersonPeriod.Location = new System.Drawing.Point(4, 22);
			this.tabPagePersonPeriod.Name = "tabPagePersonPeriod";
			this.tabPagePersonPeriod.Padding = new System.Windows.Forms.Padding(3);
			this.tabPagePersonPeriod.Size = new System.Drawing.Size(820, 577);
			this.tabPagePersonPeriod.TabIndex = 3;
			this.tabPagePersonPeriod.Text = "Person Periods";
			this.tabPagePersonPeriod.UseVisualStyleBackColor = true;
			// 
			// listViewPersonPeriods
			// 
			this.listViewPersonPeriods.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPerson,
            this.columnHeaderStartDate,
            this.colHeaderTeam,
            this.columnHeaderNote,
            this.columnHeaderContract,
            this.columnHeaderPartTimePercentage,
            this.columnHeaderContractSchedule,
            this.columnHeaderAcdLogOnId,
            this.columnHeaderAcdLogOnName});
			this.listViewPersonPeriods.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewPersonPeriods.GridLines = true;
			this.listViewPersonPeriods.Location = new System.Drawing.Point(3, 3);
			this.listViewPersonPeriods.MultiSelect = false;
			this.listViewPersonPeriods.Name = "listViewPersonPeriods";
			this.listViewPersonPeriods.Size = new System.Drawing.Size(814, 571);
			this.listViewPersonPeriods.TabIndex = 0;
			this.listViewPersonPeriods.UseCompatibleStateImageBehavior = false;
			this.listViewPersonPeriods.View = System.Windows.Forms.View.Details;
			this.listViewPersonPeriods.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listViewPersonPeriods_RightClick);
			// 
			// columnHeaderPerson
			// 
			this.columnHeaderPerson.Text = "Person";
			// 
			// columnHeaderStartDate
			// 
			this.columnHeaderStartDate.Text = "Start Date";
			// 
			// colHeaderTeam
			// 
			this.colHeaderTeam.Text = "Team";
			// 
			// columnHeaderNote
			// 
			this.columnHeaderNote.Text = "Note";
			// 
			// columnHeaderContract
			// 
			this.columnHeaderContract.Text = "Contract";
			// 
			// columnHeaderPartTimePercentage
			// 
			this.columnHeaderPartTimePercentage.Text = "Part Time Percentage";
			// 
			// columnHeaderContractSchedule
			// 
			this.columnHeaderContractSchedule.Text = "Contract Schedule";
			// 
			// columnHeaderAcdLogOnId
			// 
			this.columnHeaderAcdLogOnId.Text = "Acd Log On Id";
			// 
			// columnHeaderAcdLogOnName
			// 
			this.columnHeaderAcdLogOnName.Text = "Acd Log On Name";
			// 
			// tabPageWriteProtect
			// 
			this.tabPageWriteProtect.Controls.Add(this.tableLayoutPanel5);
			this.tabPageWriteProtect.Location = new System.Drawing.Point(4, 22);
			this.tabPageWriteProtect.Name = "tabPageWriteProtect";
			this.tabPageWriteProtect.Size = new System.Drawing.Size(820, 577);
			this.tabPageWriteProtect.TabIndex = 4;
			this.tabPageWriteProtect.Text = "WriteProtectSchedule";
			this.tabPageWriteProtect.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.ColumnCount = 1;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel5.Controls.Add(this.listViewWriteProtect, 0, 1);
			this.tableLayoutPanel5.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 2;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.2253F));
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.7747F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(820, 577);
			this.tableLayoutPanel5.TabIndex = 3;
			// 
			// listViewWriteProtect
			// 
			this.listViewWriteProtect.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderPersonName,
            this.columnHeaderWriteProtectionDate});
			this.listViewWriteProtect.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewWriteProtect.FullRowSelect = true;
			this.listViewWriteProtect.GridLines = true;
			this.listViewWriteProtect.HideSelection = false;
			this.listViewWriteProtect.Location = new System.Drawing.Point(3, 61);
			this.listViewWriteProtect.MultiSelect = false;
			this.listViewWriteProtect.Name = "listViewWriteProtect";
			this.listViewWriteProtect.Size = new System.Drawing.Size(814, 513);
			this.listViewWriteProtect.TabIndex = 3;
			this.listViewWriteProtect.UseCompatibleStateImageBehavior = false;
			this.listViewWriteProtect.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderPersonName
			// 
			this.columnHeaderPersonName.Text = "Name";
			this.columnHeaderPersonName.Width = 371;
			// 
			// columnHeaderWriteProtectionDate
			// 
			this.columnHeaderWriteProtectionDate.Text = "WriteProtectionDate";
			this.columnHeaderWriteProtectionDate.Width = 256;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonSetNewWriteProtectionDate);
			this.panel1.Controls.Add(this.dateTimePickerSetProtectDate);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(814, 52);
			this.panel1.TabIndex = 4;
			// 
			// buttonSetNewWriteProtectionDate
			// 
			this.buttonSetNewWriteProtectionDate.Location = new System.Drawing.Point(236, 18);
			this.buttonSetNewWriteProtectionDate.Name = "buttonSetNewWriteProtectionDate";
			this.buttonSetNewWriteProtectionDate.Size = new System.Drawing.Size(179, 23);
			this.buttonSetNewWriteProtectionDate.TabIndex = 1;
			this.buttonSetNewWriteProtectionDate.Text = "Set new date";
			this.buttonSetNewWriteProtectionDate.UseVisualStyleBackColor = true;
			this.buttonSetNewWriteProtectionDate.Click += new System.EventHandler(this.buttonSetNewWriteProtectionDate_Click);
			// 
			// dateTimePickerSetProtectDate
			// 
			this.dateTimePickerSetProtectDate.Location = new System.Drawing.Point(16, 18);
			this.dateTimePickerSetProtectDate.Name = "dateTimePickerSetProtectDate";
			this.dateTimePickerSetProtectDate.Size = new System.Drawing.Size(200, 20);
			this.dateTimePickerSetProtectDate.TabIndex = 0;
			// 
			// tabPagePermissions
			// 
			this.tabPagePermissions.Controls.Add(this.listViewPermissionRole);
			this.tabPagePermissions.Controls.Add(this.listViewPermissionPerson);
			this.tabPagePermissions.Controls.Add(this.panel2);
			this.tabPagePermissions.Location = new System.Drawing.Point(4, 22);
			this.tabPagePermissions.Name = "tabPagePermissions";
			this.tabPagePermissions.Size = new System.Drawing.Size(820, 577);
			this.tabPagePermissions.TabIndex = 5;
			this.tabPagePermissions.Text = "Permission";
			this.tabPagePermissions.UseVisualStyleBackColor = true;
			// 
			// listViewPermissionRole
			// 
			this.listViewPermissionRole.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader10});
			this.listViewPermissionRole.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewPermissionRole.Location = new System.Drawing.Point(484, 44);
			this.listViewPermissionRole.Name = "listViewPermissionRole";
			this.listViewPermissionRole.Size = new System.Drawing.Size(336, 533);
			this.listViewPermissionRole.TabIndex = 3;
			this.listViewPermissionRole.UseCompatibleStateImageBehavior = false;
			this.listViewPermissionRole.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "Roles granted to Person";
			this.columnHeader10.Width = 308;
			// 
			// listViewPermissionPerson
			// 
			this.listViewPermissionPerson.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader8,
            this.columnHeader9});
			this.listViewPermissionPerson.Dock = System.Windows.Forms.DockStyle.Left;
			this.listViewPermissionPerson.Location = new System.Drawing.Point(0, 44);
			this.listViewPermissionPerson.Name = "listViewPermissionPerson";
			this.listViewPermissionPerson.Size = new System.Drawing.Size(484, 533);
			this.listViewPermissionPerson.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listViewPermissionPerson.TabIndex = 2;
			this.listViewPermissionPerson.UseCompatibleStateImageBehavior = false;
			this.listViewPermissionPerson.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Text = "Name";
			this.columnHeader8.Width = 131;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "Team";
			this.columnHeader9.Width = 185;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.comboBoxRoles);
			this.panel2.Controls.Add(this.buttonRevokeRole);
			this.panel2.Controls.Add(this.buttonGrantRole);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(820, 44);
			this.panel2.TabIndex = 1;
			// 
			// comboBoxRoles
			// 
			this.comboBoxRoles.FormattingEnabled = true;
			this.comboBoxRoles.Location = new System.Drawing.Point(4, 4);
			this.comboBoxRoles.Name = "comboBoxRoles";
			this.comboBoxRoles.Size = new System.Drawing.Size(181, 21);
			this.comboBoxRoles.TabIndex = 2;
			// 
			// buttonRevokeRole
			// 
			this.buttonRevokeRole.Location = new System.Drawing.Point(272, 3);
			this.buttonRevokeRole.Name = "buttonRevokeRole";
			this.buttonRevokeRole.Size = new System.Drawing.Size(75, 23);
			this.buttonRevokeRole.TabIndex = 1;
			this.buttonRevokeRole.Text = "Revoke Role";
			this.buttonRevokeRole.UseVisualStyleBackColor = true;
			this.buttonRevokeRole.Click += new System.EventHandler(this.buttonRevokeRole_Click);
			// 
			// buttonGrantRole
			// 
			this.buttonGrantRole.Location = new System.Drawing.Point(191, 3);
			this.buttonGrantRole.Name = "buttonGrantRole";
			this.buttonGrantRole.Size = new System.Drawing.Size(75, 23);
			this.buttonGrantRole.TabIndex = 0;
			this.buttonGrantRole.Text = "Grant Role";
			this.buttonGrantRole.UseVisualStyleBackColor = true;
			this.buttonGrantRole.Click += new System.EventHandler(this.buttonGrantRole_Click);
			// 
			// backgroundWorkerLoadTree
			// 
			this.backgroundWorkerLoadTree.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadTree_DoWork);
			this.backgroundWorkerLoadTree.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadTree_RunWorkerCompleted);
			// 
			// backgroundWorkerLoadSchedules
			// 
			this.backgroundWorkerLoadSchedules.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadSchedules_DoWork);
			this.backgroundWorkerLoadSchedules.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadSchedules_RunWorkerCompleted);
			// 
			// backgroundWorkerLoadSkillData
			// 
			this.backgroundWorkerLoadSkillData.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadSkillData_DoWork);
			this.backgroundWorkerLoadSkillData.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadSkillData_RunWorkerCompleted);
			// 
			// backgroundWorkerLoadPersonPeriods
			// 
			this.backgroundWorkerLoadPersonPeriods.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoadPersonPeriods_DoWork);
			this.backgroundWorkerLoadPersonPeriods.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoadPersonPeriods_RunWorkerCompleted);
			// 
			// dataGridViewTextBoxColumn1
			// 
			this.dataGridViewTextBoxColumn1.Frozen = true;
			this.dataGridViewTextBoxColumn1.HeaderText = "xName";
			this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
			this.dataGridViewTextBoxColumn1.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn2
			// 
			this.dataGridViewTextBoxColumn2.HeaderText = "re";
			this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			// 
			// personPeriodContextMenuStrip
			// 
			this.personPeriodContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cancelPersonPeriodAfterMenuItem});
			this.personPeriodContextMenuStrip.Name = "personPeriodContextMenuStrip";
			this.personPeriodContextMenuStrip.Size = new System.Drawing.Size(109, 26);
			// 
			// cancelPersonPeriodAfterMenuItem
			// 
			this.cancelPersonPeriodAfterMenuItem.Name = "cancelPersonPeriodAfterMenuItem";
			this.cancelPersonPeriodAfterMenuItem.Size = new System.Drawing.Size(108, 22);
			this.cancelPersonPeriodAfterMenuItem.Text = "cancel";
			this.cancelPersonPeriodAfterMenuItem.Click += new System.EventHandler(this.cancelPersonPeriodAfterMenuItem_Click);
			// 
			// button9
			// 
			this.button9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button9.Location = new System.Drawing.Point(565, 59);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(240, 22);
			this.button9.TabIndex = 16;
			this.button9.Text = "Endpoint Test";
			this.button9.UseVisualStyleBackColor = true;
			this.button9.Click += new System.EventHandler(this.button9_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1068, 625);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.statusStrip1);
			this.Name = "Form1";
			this.Text = "TeleoptiCCC SDK [Demo application]";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.contextMenuStrip1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPageAgentInfo.ResumeLayout(false);
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tabPageSchedule.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tabPageSkillData.ResumeLayout(false);
			this.tabControl2.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tableLayoutPanelSkillData.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewIntraday)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewSkillDay)).EndInit();
			this.tabPagePersonPeriod.ResumeLayout(false);
			this.tabPageWriteProtect.ResumeLayout(false);
			this.tableLayoutPanel5.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.tabPagePermissions.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.personPeriodContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backgroundWorkerLogon;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TreeView treeView1;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadTree;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageAgentInfo;
        private System.Windows.Forms.TabPage tabPageSchedule;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadSchedules;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.TabPage tabPageSkillData;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSkillData;
        private System.Windows.Forms.DataGridView dataGridViewIntraday;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridView dataGridViewSkillDay;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoadSkillData;
        private System.Windows.Forms.MonthCalendar monthCalendar1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderTag;
        private System.Windows.Forms.ColumnHeader columnHeaderEmail;
        private System.Windows.Forms.ColumnHeader columnHeaderTeam;
        private System.Windows.Forms.ColumnHeader columnHeaderTimeZone;
        private System.Windows.Forms.ColumnHeader columnHeaderLanguage;
        private System.Windows.Forms.ColumnHeader columnHeaderCulture;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
		private System.Windows.Forms.TabPage tabPagePersonPeriod;
		private System.Windows.Forms.ListView listViewPersonPeriods;
		private System.Windows.Forms.ColumnHeader columnHeaderPerson;
		private System.Windows.Forms.ColumnHeader columnHeaderStartDate;
		private System.Windows.Forms.ColumnHeader colHeaderTeam;
		private System.Windows.Forms.ColumnHeader columnHeaderNote;
		private System.Windows.Forms.ColumnHeader columnHeaderPartTimePercentage;
		private System.Windows.Forms.ColumnHeader columnHeaderContractSchedule;
		private System.Windows.Forms.ColumnHeader columnHeaderAcdLogOnId;
		private System.Windows.Forms.ColumnHeader columnHeaderAcdLogOnName;
		private System.ComponentModel.BackgroundWorker backgroundWorkerLoadPersonPeriods;
		private System.Windows.Forms.ColumnHeader columnHeaderContract;
        private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TabPage tabPageWriteProtect;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.ListView listViewWriteProtect;
        private System.Windows.Forms.ColumnHeader columnHeaderPersonName;
        private System.Windows.Forms.ColumnHeader columnHeaderWriteProtectionDate;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonSetNewWriteProtectionDate;
        private System.Windows.Forms.DateTimePicker dateTimePickerSetProtectDate;
        private System.Windows.Forms.Label labelScenario;
        private System.Windows.Forms.ComboBox comboBoxScenarios;
		private System.Windows.Forms.ComboBox comboScheduleTag;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
		private System.Windows.Forms.TabPage tabPagePermissions;
		private System.Windows.Forms.ListView listViewPermissionRole;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.ListView listViewPermissionPerson;
		private System.Windows.Forms.ColumnHeader columnHeader8;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.ComboBox comboBoxRoles;
		private System.Windows.Forms.Button buttonRevokeRole;
		private System.Windows.Forms.Button buttonGrantRole;
		private System.Windows.Forms.ColumnHeader columnHeaderIdentity;
		private System.Windows.Forms.ColumnHeader columnHeaderApplicationLogOn;
		private System.Windows.Forms.ColumnHeader columnHeaderWindowsUserName;
		private ContextMenuStrip personPeriodContextMenuStrip;
		private ToolStripMenuItem cancelPersonPeriodAfterMenuItem;
		private Button button7;
		private Button button8;
		private Button button9;
	}
}