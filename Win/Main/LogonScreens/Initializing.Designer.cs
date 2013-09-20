namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	partial class Initializing
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Initializing));
			this.pictureBoxStep1 = new System.Windows.Forms.PictureBox();
			this.labelStatusText = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBoxStep1
			// 
			this.pictureBoxStep1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBoxStep1.Image")));
			this.pictureBoxStep1.Location = new System.Drawing.Point(0, 0);
			this.pictureBoxStep1.Name = "pictureBoxStep1";
			this.pictureBoxStep1.Size = new System.Drawing.Size(490, 297);
			this.pictureBoxStep1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBoxStep1.TabIndex = 3;
			this.pictureBoxStep1.TabStop = false;
			// 
			// labelStatusText
			// 
			this.labelStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelStatusText.ForeColor = System.Drawing.Color.Orange;
			this.labelStatusText.Location = new System.Drawing.Point(3, 295);
			this.labelStatusText.Name = "labelStatusText";
			this.labelStatusText.Size = new System.Drawing.Size(478, 25);
			this.labelStatusText.TabIndex = 41;
			this.labelStatusText.Text = "xxSearching for data sources...";
			this.labelStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Initializing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.labelStatusText);
			this.Controls.Add(this.pictureBoxStep1);
			this.Name = "Initializing";
			this.Size = new System.Drawing.Size(490, 337);
			((System.ComponentModel.ISupportInitialize)(this.pictureBoxStep1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBoxStep1;
		private System.Windows.Forms.Label labelStatusText;
	}
}
