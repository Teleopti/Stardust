namespace Teleopti.Ccc.Win.ExceptionHandling
{
	partial class ExceptionHandlerView
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.buttonPopEmail = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonCloseApplication = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelInformationText = new System.Windows.Forms.Label();
			this.linkLabelCopy = new System.Windows.Forms.LinkLabel();
			this.checkBoxIncludeScreenShot = new System.Windows.Forms.CheckBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonPopEmail
			// 
			this.buttonPopEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPopEmail.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonPopEmail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonPopEmail.BeforeTouchSize = new System.Drawing.Size(119, 27);
			this.buttonPopEmail.ForeColor = System.Drawing.Color.White;
			this.buttonPopEmail.IsBackStageButton = false;
			this.buttonPopEmail.Location = new System.Drawing.Point(308, 467);
			this.buttonPopEmail.Name = "buttonPopEmail";
			this.buttonPopEmail.Size = new System.Drawing.Size(119, 27);
			this.buttonPopEmail.TabIndex = 0;
			this.buttonPopEmail.Text = "xxSendEmailReport";
			this.buttonPopEmail.UseVisualStyle = true;
			this.buttonPopEmail.UseVisualStyleBackColor = true;
			this.buttonPopEmail.Click += new System.EventHandler(this.buttonPopEmailClick);
			// 
			// buttonCloseApplication
			// 
			this.buttonCloseApplication.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCloseApplication.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCloseApplication.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCloseApplication.BeforeTouchSize = new System.Drawing.Size(124, 27);
			this.buttonCloseApplication.ForeColor = System.Drawing.Color.White;
			this.buttonCloseApplication.IsBackStageButton = false;
			this.buttonCloseApplication.Location = new System.Drawing.Point(449, 467);
			this.buttonCloseApplication.Name = "buttonCloseApplication";
			this.buttonCloseApplication.Size = new System.Drawing.Size(124, 27);
			this.buttonCloseApplication.TabIndex = 1;
			this.buttonCloseApplication.Text = "xxClose";
			this.buttonCloseApplication.UseVisualStyle = true;
			this.buttonCloseApplication.UseVisualStyleBackColor = true;
			this.buttonCloseApplication.Click += new System.EventHandler(this.buttonCloseApplicationClick);
			// 
			// labelInformationText
			// 
			this.labelInformationText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelInformationText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInformationText.Location = new System.Drawing.Point(56, 42);
			this.labelInformationText.Name = "labelInformationText";
			this.labelInformationText.Size = new System.Drawing.Size(486, 358);
			this.labelInformationText.TabIndex = 4;
			this.labelInformationText.Text = "xxInformationText";
			this.labelInformationText.UseMnemonic = false;
			// 
			// linkLabelCopy
			// 
			this.linkLabelCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.linkLabelCopy.AutoSize = true;
			this.linkLabelCopy.Location = new System.Drawing.Point(56, 418);
			this.linkLabelCopy.Name = "linkLabelCopy";
			this.linkLabelCopy.Size = new System.Drawing.Size(182, 15);
			this.linkLabelCopy.TabIndex = 3;
			this.linkLabelCopy.TabStop = true;
			this.linkLabelCopy.Text = "xxCopyErrorMessageToClipboard";
			this.linkLabelCopy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelCopyLinkClicked);
			// 
			// checkBoxIncludeScreenShot
			// 
			this.checkBoxIncludeScreenShot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxIncludeScreenShot.AutoSize = true;
			this.checkBoxIncludeScreenShot.Checked = true;
			this.checkBoxIncludeScreenShot.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxIncludeScreenShot.Location = new System.Drawing.Point(91, 460);
			this.checkBoxIncludeScreenShot.Name = "checkBoxIncludeScreenShot";
			this.checkBoxIncludeScreenShot.Size = new System.Drawing.Size(134, 19);
			this.checkBoxIncludeScreenShot.TabIndex = 2;
			this.checkBoxIncludeScreenShot.Text = "xxIncludeScreenShot";
			this.checkBoxIncludeScreenShot.UseVisualStyleBackColor = true;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.ccc_Cancel_32x32;
			this.pictureBox1.Location = new System.Drawing.Point(12, 42);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(42, 37);
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			// 
			// ExceptionHandlerView
			// 
			this.AcceptButton = this.buttonCloseApplication;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ClientSize = new System.Drawing.Size(585, 506);
			this.ControlBox = false;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.checkBoxIncludeScreenShot);
			this.Controls.Add(this.linkLabelCopy);
			this.Controls.Add(this.labelInformationText);
			this.Controls.Add(this.buttonCloseApplication);
			this.Controls.Add(this.buttonPopEmail);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "ExceptionHandlerView";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "xxTeleoptiCCC";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonPopEmail;
		private Syncfusion.Windows.Forms.ButtonAdv buttonCloseApplication;
		private System.Windows.Forms.Label labelInformationText;
		private System.Windows.Forms.LinkLabel linkLabelCopy;
		private System.Windows.Forms.CheckBox checkBoxIncludeScreenShot;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}