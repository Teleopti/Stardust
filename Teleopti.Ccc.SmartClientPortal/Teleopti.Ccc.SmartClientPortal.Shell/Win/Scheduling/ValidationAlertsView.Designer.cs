namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	partial class ValidationAlertsView
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
			this.toolStripButtonFind = new System.Windows.Forms.ToolStripButton();
			this.toolStripDropDownButtonFilter = new System.Windows.Forms.ToolStripDropDownButton();
			this.xxAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeaderDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.columnHeaderAlert = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonFind,
            this.toolStripDropDownButtonFilter});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(340, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButtonFind
			// 
			this.toolStripButtonFind.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_FindAgent;
			this.toolStripButtonFind.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonFind.Name = "toolStripButtonFind";
			this.toolStripButtonFind.Size = new System.Drawing.Size(60, 22);
			this.toolStripButtonFind.Text = "xxFind";
			this.toolStripButtonFind.Click += new System.EventHandler(this.toolStripButtonFindClick);
			// 
			// toolStripDropDownButtonFilter
			// 
			this.toolStripDropDownButtonFilter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xxAllToolStripMenuItem});
			this.toolStripDropDownButtonFilter.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Filter;
			this.toolStripDropDownButtonFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownButtonFilter.Name = "toolStripDropDownButtonFilter";
			this.toolStripDropDownButtonFilter.Size = new System.Drawing.Size(72, 22);
			this.toolStripDropDownButtonFilter.Text = "xxFilter";
			// 
			// xxAllToolStripMenuItem
			// 
			this.xxAllToolStripMenuItem.Name = "xxAllToolStripMenuItem";
			this.xxAllToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
			this.xxAllToolStripMenuItem.Text = "xxAll";
			this.xxAllToolStripMenuItem.Click += new System.EventHandler(this.allToolStripMenuItemClick);
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderDate,
            this.columnHeaderName,
            this.columnHeaderAlert});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, 25);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.ShowItemToolTips = true;
			this.listView1.Size = new System.Drawing.Size(340, 494);
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1ColumnClick);
			this.listView1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView1MouseDoubleClick);
			// 
			// columnHeaderDate
			// 
			this.columnHeaderDate.Text = "xxDate";
			this.columnHeaderDate.Width = 76;
			// 
			// columnHeaderName
			// 
			this.columnHeaderName.Text = "xxName";
			this.columnHeaderName.Width = 139;
			// 
			// columnHeaderAlert
			// 
			this.columnHeaderAlert.Text = "xxAlert";
			this.columnHeaderAlert.Width = 279;
			// 
			// ValidationAlertsView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "ValidationAlertsView";
			this.Size = new System.Drawing.Size(340, 519);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeaderDate;
		private System.Windows.Forms.ColumnHeader columnHeaderName;
		private System.Windows.Forms.ColumnHeader columnHeaderAlert;
		private System.Windows.Forms.ToolStripButton toolStripButtonFind;
		private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonFilter;
		private System.Windows.Forms.ToolStripMenuItem xxAllToolStripMenuItem;
	}
}
