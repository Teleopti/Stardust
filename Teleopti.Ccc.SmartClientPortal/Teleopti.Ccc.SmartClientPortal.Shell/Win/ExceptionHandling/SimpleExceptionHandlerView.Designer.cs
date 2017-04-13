
namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling
{
	partial class SimpleExceptionHandlerView
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
			this.buttonCopyErrorDetails = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelInformationText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// buttonCopyErrorDetails
			// 
			this.buttonCopyErrorDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCopyErrorDetails.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonCopyErrorDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonCopyErrorDetails.BeforeTouchSize = new System.Drawing.Size(119, 27);
			this.buttonCopyErrorDetails.ForeColor = System.Drawing.Color.White;
			this.buttonCopyErrorDetails.IsBackStageButton = false;
			this.buttonCopyErrorDetails.Location = new System.Drawing.Point(226, 176);
			this.buttonCopyErrorDetails.Name = "buttonCopyErrorDetails";
			this.buttonCopyErrorDetails.Size = new System.Drawing.Size(119, 27);
			this.buttonCopyErrorDetails.TabIndex = 0;
			this.buttonCopyErrorDetails.Text = "xxCopyDetails";
			this.buttonCopyErrorDetails.UseVisualStyle = true;
			this.buttonCopyErrorDetails.UseVisualStyleBackColor = true;
			this.buttonCopyErrorDetails.Click += new System.EventHandler(this.buttonCopyErrorDetailsClick);
			// 
			// buttonOk
			// 
			this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.ForeColor = System.Drawing.Color.White;
			this.buttonOk.IsBackStageButton = false;
			this.buttonOk.Location = new System.Drawing.Point(401, 176);
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.Size = new System.Drawing.Size(87, 27);
			this.buttonOk.TabIndex = 1;
			this.buttonOk.Text = "xxOK";
			this.buttonOk.UseVisualStyle = true;
			this.buttonOk.UseVisualStyleBackColor = true;
			// 
			// labelInformationText
			// 
			this.labelInformationText.BackColor = System.Drawing.Color.White;
			this.labelInformationText.Dock = System.Windows.Forms.DockStyle.Top;
			this.labelInformationText.Location = new System.Drawing.Point(0, 0);
			this.labelInformationText.Multiline = true;
			this.labelInformationText.Name = "labelInformationText";
			this.labelInformationText.ReadOnly = true;
			this.labelInformationText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.labelInformationText.Size = new System.Drawing.Size(488, 165);
			this.labelInformationText.TabIndex = 4;
			this.labelInformationText.Text = "xxInformationText";
			// 
			// SimpleExceptionHandlerView
			// 
			this.AcceptButton = this.buttonOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(488, 210);
			this.ControlBox = false;
			this.Controls.Add(this.labelInformationText);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.buttonCopyErrorDetails);
			this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.Name = "SimpleExceptionHandlerView";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxTeleoptiCCC";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonCopyErrorDetails;
		private Syncfusion.Windows.Forms.ButtonAdv buttonOk;
		private System.Windows.Forms.TextBox labelInformationText;
	}
}