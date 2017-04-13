namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
	partial class SkillResultHighlightGridControl
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
			this.components = new System.ComponentModel.Container();
			this.contextMenuStripHighlightGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.toolStripMenuItemGoToDate = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripHighlightGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			this.SuspendLayout();
			// 
			// contextMenuStripHighlightGrid
			// 
			this.contextMenuStripHighlightGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemGoToDate});
			this.contextMenuStripHighlightGrid.Name = "contextMenuStripHighlightGrid";
			this.contextMenuStripHighlightGrid.Size = new System.Drawing.Size(144, 26);
			// 
			// toolStripMenuItemGoToDate
			// 
			this.toolStripMenuItemGoToDate.Name = "toolStripMenuItemGoToDate";
			this.toolStripMenuItemGoToDate.Size = new System.Drawing.Size(143, 22);
			this.toolStripMenuItemGoToDate.Text = "xxGo To Date";
			this.toolStripMenuItemGoToDate.Click += new System.EventHandler(this.toolStripMenuItemGoToDate_Click);
			// 
			// SkillResultHighlightGridControl
			// 
			this.ContextMenuStrip = this.contextMenuStripHighlightGrid;
			this.contextMenuStripHighlightGrid.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ContextMenuStrip contextMenuStripHighlightGrid;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemGoToDate;
	}
}
