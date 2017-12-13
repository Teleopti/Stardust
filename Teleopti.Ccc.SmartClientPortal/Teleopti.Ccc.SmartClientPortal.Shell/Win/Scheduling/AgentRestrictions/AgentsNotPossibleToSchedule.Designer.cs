﻿namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.AgentRestrictions
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgentsNotPossibleToSchedule));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
			this.listViewResult = new System.Windows.Forms.ListView();
			this.columnHeaderAgent = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderReason = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonRefresh});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(1050, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButtonRefresh
			// 
			this.toolStripButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonRefresh.Image")));
			this.toolStripButtonRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
			this.toolStripButtonRefresh.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonRefresh.Text = "xxRefresh";
			this.toolStripButtonRefresh.Click += new System.EventHandler(this.toolStripButtonRefreshClick);
			// 
			// listViewResult
			// 
			this.listViewResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderAgent,
            this.columnHeaderReason});
			this.listViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewResult.Location = new System.Drawing.Point(0, 25);
			this.listViewResult.Name = "listViewResult";
			this.listViewResult.Size = new System.Drawing.Size(1050, 327);
			this.listViewResult.TabIndex = 1;
			this.listViewResult.UseCompatibleStateImageBehavior = false;
			this.listViewResult.View = System.Windows.Forms.View.Details;
			// 
			// columnHeaderAgent
			// 
			this.columnHeaderAgent.Text = "xxAgent";
			this.columnHeaderAgent.Width = 300;
			// 
			// columnHeaderReason
			// 
			this.columnHeaderReason.Text = "xxReason";
			this.columnHeaderReason.Width = 300;
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
	}
}
