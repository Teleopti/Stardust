namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	partial class OutlookBar
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
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.contextMenuStripEx1 = new Syncfusion.Windows.Forms.Tools.ContextMenuStripEx();
			this.navigationOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuStripEx1.SuspendLayout();
			this.SuspendLayout();
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(150, 36);
			this.flowLayoutPanel1.TabIndex = 0;
			// 
			// contextMenuStripEx1
			// 
			this.contextMenuStripEx1.DropShadowEnabled = false;
			this.contextMenuStripEx1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.navigationOptionsToolStripMenuItem,
            this.toolStripMenuItem1});
			this.contextMenuStripEx1.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(236)))), ((int)(((byte)(249)))));
			this.contextMenuStripEx1.Name = "contextMenuStripEx1";
			this.contextMenuStripEx1.Size = new System.Drawing.Size(159, 32);
			this.contextMenuStripEx1.Style = Syncfusion.Windows.Forms.Tools.ContextMenuStripEx.ContextMenuStyle.Metro;
			// 
			// navigationOptionsToolStripMenuItem
			// 
			this.navigationOptionsToolStripMenuItem.CheckOnClick = true;
			this.navigationOptionsToolStripMenuItem.Name = "navigationOptionsToolStripMenuItem";
			this.navigationOptionsToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.navigationOptionsToolStripMenuItem.Text = "xxCompactView";
			this.navigationOptionsToolStripMenuItem.CheckedChanged += new System.EventHandler(this.navigationOptionsToolStripMenuItemCheckedChanged);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(155, 6);
			// 
			// OutlookBar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.flowLayoutPanel1);
			this.MinimumSize = new System.Drawing.Size(150, 36);
			this.Name = "OutlookBar";
			this.Size = new System.Drawing.Size(150, 36);
			this.contextMenuStripEx1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private Syncfusion.Windows.Forms.Tools.ContextMenuStripEx contextMenuStripEx1;
		private System.Windows.Forms.ToolStripMenuItem navigationOptionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
	}
}
