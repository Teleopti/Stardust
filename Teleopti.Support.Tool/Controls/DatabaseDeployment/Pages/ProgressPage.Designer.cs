namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
	partial class ProgressPage
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
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.smoothLabel1 = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.SuspendLayout();
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.textBoxOutput.Font = new System.Drawing.Font("Lucida Console", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxOutput.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.textBoxOutput.Location = new System.Drawing.Point(0, 38);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxOutput.Size = new System.Drawing.Size(660, 202);
            this.textBoxOutput.TabIndex = 38;
            // 
            // smoothLabel1
            // 
            this.smoothLabel1.AutoSize = true;
            this.smoothLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabel1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.smoothLabel1.Location = new System.Drawing.Point(3, 15);
            this.smoothLabel1.Name = "smoothLabel1";
            this.smoothLabel1.Size = new System.Drawing.Size(158, 16);
            this.smoothLabel1.TabIndex = 39;
            this.smoothLabel1.Text = "Deployment Progress";
            this.smoothLabel1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // ProgressPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.smoothLabel1);
            this.Controls.Add(this.textBoxOutput);
            this.Name = "ProgressPage";
            this.Size = new System.Drawing.Size(660, 240);
            this.Load += new System.EventHandler(this.progressPage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxOutput;
		private General.SmoothLabel smoothLabel1;
	}
}
