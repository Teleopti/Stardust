namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Backlog
{
	partial class OutboundView
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
			System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("testgroup", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("another", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("test");
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("anothe test");
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.viewStatusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addActualBacklogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addManualProductionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.replanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.changePeriodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStripContainer1
			// 
			this.toolStripContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.listView1);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(937, 536);
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 1);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(937, 536);
			this.toolStripContainer1.TabIndex = 1;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
			this.listView1.ContextMenuStrip = this.contextMenuStrip1;
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			listViewGroup1.Header = "testgroup";
			listViewGroup1.Name = "testgroup";
			listViewGroup2.Header = "another";
			listViewGroup2.Name = "listViewGroup1";
			this.listView1.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
			listViewItem1.Group = listViewGroup1;
			listViewItem2.Group = listViewGroup2;
			this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2});
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(937, 536);
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Campaign";
			this.columnHeader1.Width = 394;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Period";
			this.columnHeader2.Width = 160;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 92;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewStatusToolStripMenuItem,
            this.addActualBacklogToolStripMenuItem,
            this.addManualProductionToolStripMenuItem,
            this.replanToolStripMenuItem,
            this.changePeriodToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(211, 114);
			// 
			// viewStatusToolStripMenuItem
			// 
			this.viewStatusToolStripMenuItem.Name = "viewStatusToolStripMenuItem";
			this.viewStatusToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.viewStatusToolStripMenuItem.Text = "View status...";
			this.viewStatusToolStripMenuItem.Click += new System.EventHandler(this.viewStatusToolStripMenuItem_Click);
			// 
			// addActualBacklogToolStripMenuItem
			// 
			this.addActualBacklogToolStripMenuItem.Name = "addActualBacklogToolStripMenuItem";
			this.addActualBacklogToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.addActualBacklogToolStripMenuItem.Text = "Add actual backlog...";
			this.addActualBacklogToolStripMenuItem.Click += new System.EventHandler(this.addActualBacklogToolStripMenuItem_Click);
			// 
			// addManualProductionToolStripMenuItem
			// 
			this.addManualProductionToolStripMenuItem.Name = "addManualProductionToolStripMenuItem";
			this.addManualProductionToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.addManualProductionToolStripMenuItem.Text = "Add manual production...";
			this.addManualProductionToolStripMenuItem.Click += new System.EventHandler(this.addManualProductionToolStripMenuItem_Click);
			// 
			// replanToolStripMenuItem
			// 
			this.replanToolStripMenuItem.Name = "replanToolStripMenuItem";
			this.replanToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.replanToolStripMenuItem.Text = "Replan";
			this.replanToolStripMenuItem.Click += new System.EventHandler(this.replanToolStripMenuItem_Click);
			// 
			// changePeriodToolStripMenuItem
			// 
			this.changePeriodToolStripMenuItem.Name = "changePeriodToolStripMenuItem";
			this.changePeriodToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.changePeriodToolStripMenuItem.Text = "Change period...";
			this.changePeriodToolStripMenuItem.Click += new System.EventHandler(this.changePeriodToolStripMenuItem_Click);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 540);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(937, 22);
			this.statusStrip1.TabIndex = 2;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// OutboundView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(937, 562);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStripContainer1);
			this.Name = "OutboundView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "OutboundView";
			this.Load += new System.EventHandler(this.outboundViewLoad);
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem viewStatusToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addActualBacklogToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addManualProductionToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem replanToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem changePeriodToolStripMenuItem;

	}
}