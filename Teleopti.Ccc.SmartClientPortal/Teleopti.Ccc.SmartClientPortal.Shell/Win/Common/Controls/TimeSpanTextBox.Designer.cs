namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
	partial class TimeSpanTextBox
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
            this.TimeSpanBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TimeSpanBox
            // 
            this.TimeSpanBox.Location = new System.Drawing.Point(0, 1);
            this.TimeSpanBox.Name = "TimeSpanBox";
            this.TimeSpanBox.Size = new System.Drawing.Size(33, 20);
            this.TimeSpanBox.TabIndex = 0;
            // 
            // TimeSpanTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TimeSpanBox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "TimeSpanTextBox";
            this.Size = new System.Drawing.Size(74, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.TextBox TimeSpanBox;
	}
}
