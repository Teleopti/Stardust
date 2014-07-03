namespace Teleopti.Ccc.SmartClientPortal.Shell.Controls
{
	partial class ModulePanelMenu
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
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.SuspendLayout();
			// 
			// autoLabel1
			// 
			this.autoLabel1.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.autoLabel1.Location = new System.Drawing.Point(0, -10);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(38, 37);
			this.autoLabel1.TabIndex = 0;
			this.autoLabel1.Text = "...";
			this.autoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.autoLabel1.Click += new System.EventHandler(this.onClick);
			this.autoLabel1.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.autoLabel1.MouseLeave += new System.EventHandler(this.onMouseLeave);
			// 
			// ModulePanelMenu
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.autoLabel1);
			this.Name = "ModulePanelMenu";
			this.Size = new System.Drawing.Size(36, 36);
			this.Click += new System.EventHandler(this.onClick);
			this.MouseEnter += new System.EventHandler(this.onMouseEnter);
			this.MouseLeave += new System.EventHandler(this.onMouseLeave);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
	}
}
