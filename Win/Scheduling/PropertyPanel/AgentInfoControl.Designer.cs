namespace Teleopti.Ccc.Win.Scheduling.PropertyPanel
{
	partial class AgentInfoControl
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu")]
        private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgentInfoControl));
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "Hej : hej",
            ""}, -1);
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
            "Hej : hej",
            ""}, -1);
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
            "Hej : hej",
            ""}, -1);
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
            "Hej : hej",
            ""}, -1);
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.tabControlAgentInfo = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageAdvSchedulePeriod = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.listViewSchedulePeriod = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageAdvRestrictions = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.listViewRestrictions = new System.Windows.Forms.ListView();
			this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageAdvPersonPeriod = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.listViewPersonPeriod = new System.Windows.Forms.ListView();
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageAdvPerson = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.listViewPerson = new System.Windows.Forms.ListView();
			this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.tabPageFairness = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.comboBoxAgentGrouping = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.label1 = new System.Windows.Forms.Label();
			this.perPersonAndGroupListView = new System.Windows.Forms.ListView();
			this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.timerRefresh = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.tabControlAgentInfo)).BeginInit();
			this.tabControlAgentInfo.SuspendLayout();
			this.tabPageAdvSchedulePeriod.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.tabPageAdvRestrictions.SuspendLayout();
			this.tabPageAdvPersonPeriod.SuspendLayout();
			this.tabPageAdvPerson.SuspendLayout();
			this.tabPageFairness.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAgentGrouping)).BeginInit();
			this.SuspendLayout();
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "ikon.16.png");
			// 
			// tabControlAgentInfo
			// 
			this.tabControlAgentInfo.ActiveTabFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabControlAgentInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvSchedulePeriod);
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvRestrictions);
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvPersonPeriod);
			this.tabControlAgentInfo.Controls.Add(this.tabPageAdvPerson);
			this.tabControlAgentInfo.Controls.Add(this.tabPageFairness);
			this.tabControlAgentInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlAgentInfo.Location = new System.Drawing.Point(0, 0);
			this.tabControlAgentInfo.Name = "tabControlAgentInfo";
			this.tabControlAgentInfo.Size = new System.Drawing.Size(453, 462);
			this.tabControlAgentInfo.TabGap = 10;
			this.tabControlAgentInfo.TabIndex = 1;
			this.tabControlAgentInfo.TabPanelBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(199)))), ((int)(((byte)(216)))), ((int)(((byte)(237)))));
			this.tabControlAgentInfo.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererOffice2007);
			this.tabControlAgentInfo.SelectedIndexChanged += new System.EventHandler(this.tabControlAgentInfo_SelectedIndexChanged);
			// 
			// tabPageAdvSchedulePeriod
			// 
			this.tabPageAdvSchedulePeriod.AutoSize = true;
			this.tabPageAdvSchedulePeriod.Controls.Add(this.toolStrip1);
			this.tabPageAdvSchedulePeriod.Controls.Add(this.listViewSchedulePeriod);
			this.tabPageAdvSchedulePeriod.Image = null;
			this.tabPageAdvSchedulePeriod.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvSchedulePeriod.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvSchedulePeriod.Name = "tabPageAdvSchedulePeriod";
			this.tabPageAdvSchedulePeriod.ShowCloseButton = true;
			this.tabPageAdvSchedulePeriod.Size = new System.Drawing.Size(453, 441);
			this.tabPageAdvSchedulePeriod.TabIndex = 2;
			this.tabPageAdvSchedulePeriod.Text = "xxSchedulePeriod";
			this.tabPageAdvSchedulePeriod.ThemesEnabled = false;
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(453, 25);
			this.toolStrip1.Stretch = true;
			this.toolStrip1.TabIndex = 2;
			this.toolStrip1.Text = "toolStrip1";
			this.toolStrip1.Visible = false;
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.AutoToolTip = false;
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(124, 22);
			this.toolStripButton1.Text = "xxCalculateLegalState";
			this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// listViewSchedulePeriod
			// 
			this.listViewSchedulePeriod.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listViewSchedulePeriod.BackColor = System.Drawing.Color.White;
			this.listViewSchedulePeriod.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listViewSchedulePeriod.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.listViewSchedulePeriod.FullRowSelect = true;
			this.listViewSchedulePeriod.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			listViewItem1.IndentCount = 25;
			this.listViewSchedulePeriod.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
			this.listViewSchedulePeriod.Location = new System.Drawing.Point(0, 0);
			this.listViewSchedulePeriod.Name = "listViewSchedulePeriod";
			this.listViewSchedulePeriod.Size = new System.Drawing.Size(453, 441);
			this.listViewSchedulePeriod.SmallImageList = this.imageList1;
			this.listViewSchedulePeriod.TabIndex = 1;
			this.listViewSchedulePeriod.UseCompatibleStateImageBehavior = false;
			this.listViewSchedulePeriod.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "xxStatistics";
			this.columnHeader1.Width = 250;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Width = 200;
			// 
			// tabPageAdvRestrictions
			// 
			this.tabPageAdvRestrictions.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageAdvRestrictions.Controls.Add(this.listViewRestrictions);
			this.tabPageAdvRestrictions.Image = null;
			this.tabPageAdvRestrictions.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvRestrictions.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvRestrictions.Name = "tabPageAdvRestrictions";
			this.tabPageAdvRestrictions.ShowCloseButton = true;
			this.tabPageAdvRestrictions.Size = new System.Drawing.Size(453, 441);
			this.tabPageAdvRestrictions.TabIndex = 4;
			this.tabPageAdvRestrictions.Text = "xxRestrictions";
			this.tabPageAdvRestrictions.ThemesEnabled = false;
			// 
			// listViewRestrictions
			// 
			this.listViewRestrictions.BackColor = System.Drawing.Color.White;
			this.listViewRestrictions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6});
			this.listViewRestrictions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewRestrictions.FullRowSelect = true;
			this.listViewRestrictions.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			listViewItem2.IndentCount = 25;
			this.listViewRestrictions.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem2});
			this.listViewRestrictions.Location = new System.Drawing.Point(0, 0);
			this.listViewRestrictions.Name = "listViewRestrictions";
			this.listViewRestrictions.Size = new System.Drawing.Size(453, 441);
			this.listViewRestrictions.SmallImageList = this.imageList1;
			this.listViewRestrictions.TabIndex = 2;
			this.listViewRestrictions.UseCompatibleStateImageBehavior = false;
			this.listViewRestrictions.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "xxStatistics";
			this.columnHeader5.Width = 250;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Width = 200;
			// 
			// tabPageAdvPersonPeriod
			// 
			this.tabPageAdvPersonPeriod.Controls.Add(this.listViewPersonPeriod);
			this.tabPageAdvPersonPeriod.Image = null;
			this.tabPageAdvPersonPeriod.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvPersonPeriod.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvPersonPeriod.Name = "tabPageAdvPersonPeriod";
			this.tabPageAdvPersonPeriod.ShowCloseButton = true;
			this.tabPageAdvPersonPeriod.Size = new System.Drawing.Size(453, 441);
			this.tabPageAdvPersonPeriod.TabIndex = 3;
			this.tabPageAdvPersonPeriod.Text = "xxPersonPeriod";
			this.tabPageAdvPersonPeriod.ThemesEnabled = false;
			// 
			// listViewPersonPeriod
			// 
			this.listViewPersonPeriod.BackColor = System.Drawing.Color.White;
			this.listViewPersonPeriod.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
			this.listViewPersonPeriod.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewPersonPeriod.FullRowSelect = true;
			this.listViewPersonPeriod.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			listViewItem3.IndentCount = 25;
			this.listViewPersonPeriod.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem3});
			this.listViewPersonPeriod.Location = new System.Drawing.Point(0, 0);
			this.listViewPersonPeriod.Name = "listViewPersonPeriod";
			this.listViewPersonPeriod.Size = new System.Drawing.Size(453, 441);
			this.listViewPersonPeriod.SmallImageList = this.imageList1;
			this.listViewPersonPeriod.TabIndex = 2;
			this.listViewPersonPeriod.UseCompatibleStateImageBehavior = false;
			this.listViewPersonPeriod.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "xxStatistics";
			this.columnHeader3.Width = 250;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Width = 200;
			// 
			// tabPageAdvPerson
			// 
			this.tabPageAdvPerson.Controls.Add(this.listViewPerson);
			this.tabPageAdvPerson.Image = null;
			this.tabPageAdvPerson.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageAdvPerson.Location = new System.Drawing.Point(0, 21);
			this.tabPageAdvPerson.Name = "tabPageAdvPerson";
			this.tabPageAdvPerson.ShowCloseButton = true;
			this.tabPageAdvPerson.Size = new System.Drawing.Size(453, 441);
			this.tabPageAdvPerson.TabIndex = 5;
			this.tabPageAdvPerson.Text = "xxPerson";
			this.tabPageAdvPerson.ThemesEnabled = false;
			// 
			// listViewPerson
			// 
			this.listViewPerson.BackColor = System.Drawing.Color.White;
			this.listViewPerson.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8});
			this.listViewPerson.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewPerson.FullRowSelect = true;
			this.listViewPerson.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			listViewItem4.IndentCount = 25;
			this.listViewPerson.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem4});
			this.listViewPerson.Location = new System.Drawing.Point(0, 0);
			this.listViewPerson.Name = "listViewPerson";
			this.listViewPerson.Size = new System.Drawing.Size(453, 441);
			this.listViewPerson.SmallImageList = this.imageList1;
			this.listViewPerson.TabIndex = 3;
			this.listViewPerson.UseCompatibleStateImageBehavior = false;
			this.listViewPerson.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader7
			// 
			this.columnHeader7.Text = "xxStatistics";
			this.columnHeader7.Width = 250;
			// 
			// columnHeader8
			// 
			this.columnHeader8.Width = 249;
			// 
			// tabPageFairness
			// 
			this.tabPageFairness.Controls.Add(this.tableLayoutPanel1);
			this.tabPageFairness.Image = null;
			this.tabPageFairness.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageFairness.Location = new System.Drawing.Point(0, 21);
			this.tabPageFairness.Name = "tabPageFairness";
			this.tabPageFairness.ShowCloseButton = true;
			this.tabPageFairness.Size = new System.Drawing.Size(453, 441);
			this.tabPageFairness.TabIndex = 6;
			this.tabPageFairness.Text = "xxFairness";
			this.tabPageFairness.ThemesEnabled = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.perPersonAndGroupListView, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(453, 441);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.BackColor = System.Drawing.Color.White;
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.comboBoxAgentGrouping, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 3F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 97F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(447, 31);
			this.tableLayoutPanel2.TabIndex = 5;
			// 
			// comboBoxAgentGrouping
			// 
			this.comboBoxAgentGrouping.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.comboBoxAgentGrouping.Dock = System.Windows.Forms.DockStyle.Fill;
			this.comboBoxAgentGrouping.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAgentGrouping.Location = new System.Drawing.Point(203, 3);
			this.comboBoxAgentGrouping.Name = "comboBoxAgentGrouping";
			this.comboBoxAgentGrouping.Size = new System.Drawing.Size(241, 19);
			this.comboBoxAgentGrouping.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.comboBoxAgentGrouping.TabIndex = 1;
			this.comboBoxAgentGrouping.SelectedIndexChanged += new System.EventHandler(this.comboBoxAgentGrouping_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(194, 31);
			this.label1.TabIndex = 2;
			this.label1.Text = "xxCompareWithTeamsFrom";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// perPersonAndGroupListView
			// 
			this.perPersonAndGroupListView.BackColor = System.Drawing.Color.White;
			this.perPersonAndGroupListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.perPersonAndGroupListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader9,
            this.columnHeader10});
			this.perPersonAndGroupListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.perPersonAndGroupListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.perPersonAndGroupListView.Location = new System.Drawing.Point(3, 40);
			this.perPersonAndGroupListView.Name = "perPersonAndGroupListView";
			this.perPersonAndGroupListView.Size = new System.Drawing.Size(447, 398);
			this.perPersonAndGroupListView.StateImageList = this.imageList1;
			this.perPersonAndGroupListView.TabIndex = 0;
			this.perPersonAndGroupListView.UseCompatibleStateImageBehavior = false;
			this.perPersonAndGroupListView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader11
			// 
			this.columnHeader11.Text = "xxShiftCategory";
			this.columnHeader11.Width = 167;
			// 
			// columnHeader12
			// 
			this.columnHeader12.Text = "xxAgent";
			this.columnHeader12.Width = 90;
			// 
			// columnHeader9
			// 
			this.columnHeader9.Text = "xxTeam";
			this.columnHeader9.Width = 90;
			// 
			// columnHeader10
			// 
			this.columnHeader10.Text = "xxAll";
			this.columnHeader10.Width = 90;
			// 
			// timerRefresh
			// 
			this.timerRefresh.Interval = 20000;
			this.timerRefresh.Tick += new System.EventHandler(this.timerRefreshTick);
			// 
			// AgentInfoControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.tabControlAgentInfo);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "AgentInfoControl";
			this.Size = new System.Drawing.Size(453, 462);
			this.Load += new System.EventHandler(this.AgentInfo_FromLoad);
			((System.ComponentModel.ISupportInitialize)(this.tabControlAgentInfo)).EndInit();
			this.tabControlAgentInfo.ResumeLayout(false);
			this.tabControlAgentInfo.PerformLayout();
			this.tabPageAdvSchedulePeriod.ResumeLayout(false);
			this.tabPageAdvSchedulePeriod.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.tabPageAdvRestrictions.ResumeLayout(false);
			this.tabPageAdvPersonPeriod.ResumeLayout(false);
			this.tabPageAdvPerson.ResumeLayout(false);
			this.tabPageFairness.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAgentGrouping)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlAgentInfo;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvSchedulePeriod;
        private System.Windows.Forms.ListView listViewSchedulePeriod;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ImageList imageList1;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvPersonPeriod;
        private System.Windows.Forms.ListView listViewPersonPeriod;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvRestrictions;
        private System.Windows.Forms.ListView listViewRestrictions;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageAdvPerson;
        private System.Windows.Forms.ListView listViewPerson;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageFairness;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAgentGrouping;
        private System.Windows.Forms.ListView perPersonAndGroupListView;
        private System.Windows.Forms.ColumnHeader columnHeader11;
		private System.Windows.Forms.ColumnHeader columnHeader12;
		private System.Windows.Forms.ColumnHeader columnHeader9;
		private System.Windows.Forms.ColumnHeader columnHeader10;
		private System.Windows.Forms.Timer timerRefresh;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
        
	}
}