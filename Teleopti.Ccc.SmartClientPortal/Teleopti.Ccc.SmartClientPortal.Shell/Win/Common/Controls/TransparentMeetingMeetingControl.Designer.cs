namespace Teleopti.Ccc.Win.Common.Controls
{
	partial class TransparentMeetingMeetingControl
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
			this.panelWest = new System.Windows.Forms.Panel();
			this.panelEast = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// panelWest
			// 
			this.panelWest.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.panelWest.Cursor = System.Windows.Forms.Cursors.SizeWE;
			this.panelWest.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelWest.Location = new System.Drawing.Point(0, 0);
			this.panelWest.Name = "panelWest";
			this.panelWest.Size = new System.Drawing.Size(10, 150);
			this.panelWest.TabIndex = 0;
			this.panelWest.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelWestMouseDown);
			this.panelWest.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelWestMouseMove);
			this.panelWest.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelWestMouseUp);
			// 
			// panelEast
			// 
			this.panelEast.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.panelEast.Cursor = System.Windows.Forms.Cursors.SizeWE;
			this.panelEast.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelEast.Location = new System.Drawing.Point(140, 0);
			this.panelEast.Name = "panelEast";
			this.panelEast.Size = new System.Drawing.Size(10, 150);
			this.panelEast.TabIndex = 1;
			this.panelEast.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PanelEastMouseDown);
			this.panelEast.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PanelEastMouseMove);
			this.panelEast.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PanelEastMouseUp);
			// 
			// TransparentMeetingMeetingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.panelEast);
			this.Controls.Add(this.panelWest);
			this.Name = "TransparentMeetingMeetingControl";
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TransparentControlMouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TransparentControlMouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TransparentControlMouseUp);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelWest;
		private System.Windows.Forms.Panel panelEast;
	}
}
