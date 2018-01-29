namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
{
	partial class AgentsNotPossibleToSchedule
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
			this.toolStripLabelManySelected = new System.Windows.Forms.ToolStripLabel();
			this.listViewResult = new System.Windows.Forms.ListView();
			this.columnHeaderAgent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderPeriod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRefresh,
            this.toolStripLabelManySelected});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(1050, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButtonRefresh
			// 
			this.toolStripButtonRefresh.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Refresh_16x16;
			this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
			this.toolStripButtonRefresh.Size = new System.Drawing.Size(76, 22);
			this.toolStripButtonRefresh.Text = "xxRefresh";
			this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefreshClick);
			// 
			// toolStripLabelManySelected
			// 
			this.toolStripLabelManySelected.Name = "toolStripLabelManySelected";
			this.toolStripLabelManySelected.Size = new System.Drawing.Size(109, 22);
			this.toolStripLabelManySelected.Text = "xxManyAgentsAlert";
			// 
			// listViewResult
			// 
			this.listViewResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAgent,
            this.columnHeaderReason,
            this.columnHeaderPeriod});
			this.listViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewResult.FullRowSelect = true;
			this.listViewResult.HideSelection = false;
			this.listViewResult.Location = new System.Drawing.Point(0, 25);
			this.listViewResult.Name = "listViewResult";
			this.listViewResult.Size = new System.Drawing.Size(1050, 327);
			this.listViewResult.TabIndex = 1;
			this.listViewResult.UseCompatibleStateImageBehavior = false;
			this.listViewResult.View = System.Windows.Forms.View.Details;
			this.listViewResult.SelectedIndexChanged += new System.EventHandler(this.listViewResultSelectedIndexChanged);
			this.listViewResult.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewResultKeyDown);
			// 
			// columnHeaderAgent
			// 
			this.columnHeaderAgent.Text = "xxAgentsThatFailedInspection";
			this.columnHeaderAgent.Width = 300;
			// 
			// columnHeaderReason
			// 
			this.columnHeaderReason.Text = "xxPrimaryReason";
			this.columnHeaderReason.Width = 300;
			// 
			// columnHeaderPeriod
			// 
			this.columnHeaderPeriod.Text = "xxPeriod";
			this.columnHeaderPeriod.Width = 250;
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.WorkerSupportsCancellation = true;
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1DoWork);
			this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1RunWorkerCompleted);
			// 
			// AgentsNotPossibleToSchedule
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listViewResult);
			this.Controls.Add(this.toolStrip1);
			this.Name = "AgentsNotPossibleToSchedule";
			this.Size = new System.Drawing.Size(1050, 352);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
		private System.Windows.Forms.ListView listViewResult;
		private System.Windows.Forms.ColumnHeader columnHeaderAgent;
		private System.Windows.Forms.ColumnHeader columnHeaderReason;
		private System.Windows.Forms.ColumnHeader columnHeaderPeriod;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private System.Windows.Forms.ToolStripLabel toolStripLabelManySelected;
	}
}
