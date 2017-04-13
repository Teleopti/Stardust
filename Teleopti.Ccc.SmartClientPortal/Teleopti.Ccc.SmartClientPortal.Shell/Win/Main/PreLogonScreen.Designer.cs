namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
	partial class PreLogonScreen
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreLogonScreen));
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel1 = new System.Windows.Forms.Panel();
			this.comboBoxAdvSDKList = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.labelChooseSDK = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvSDKList)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(15, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(104, 93);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.LoginHeaderText;
			this.pictureBox2.Location = new System.Drawing.Point(136, 12);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(235, 54);
			this.pictureBox2.TabIndex = 5;
			this.pictureBox2.TabStop = false;
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnOK.BackColor = System.Drawing.Color.White;
			this.buttonLogOnOK.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonLogOnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnOK.Image = global::Teleopti.Ccc.SmartClientPortal.Shell.Properties.Resources.right_round_32;
			this.buttonLogOnOK.IsBackStageButton = false;
			this.buttonLogOnOK.Location = new System.Drawing.Point(250, 113);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.TabIndex = 6;
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.comboBoxAdvSDKList);
			this.panel1.Controls.Add(this.labelChooseSDK);
			this.panel1.Controls.Add(this.buttonLogOnOK);
			this.panel1.Location = new System.Drawing.Point(136, 73);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(285, 148);
			this.panel1.TabIndex = 7;
			// 
			// comboBoxAdvSDKList
			// 
			this.comboBoxAdvSDKList.BackColor = System.Drawing.Color.White;
			this.comboBoxAdvSDKList.BeforeTouchSize = new System.Drawing.Size(223, 23);
			this.comboBoxAdvSDKList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxAdvSDKList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.comboBoxAdvSDKList.Location = new System.Drawing.Point(3, 28);
			this.comboBoxAdvSDKList.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.comboBoxAdvSDKList.Name = "comboBoxAdvSDKList";
			this.comboBoxAdvSDKList.Size = new System.Drawing.Size(223, 23);
			this.comboBoxAdvSDKList.TabIndex = 38;
			// 
			// labelChooseSDK
			// 
			this.labelChooseSDK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelChooseSDK.AutoSize = true;
			this.labelChooseSDK.BackColor = System.Drawing.Color.Transparent;
			this.labelChooseSDK.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelChooseSDK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.labelChooseSDK.Location = new System.Drawing.Point(0, 0);
			this.labelChooseSDK.Name = "labelChooseSDK";
			this.labelChooseSDK.Size = new System.Drawing.Size(184, 25);
			this.labelChooseSDK.TabIndex = 37;
			this.labelChooseSDK.Text = "Select Configuration";
			// 
			// PreLogonScreen
			// 
			this.AcceptButton = this.buttonLogOnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(425, 220);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.pictureBox1);
			this.DropShadow = true;
			this.Font = new System.Drawing.Font("Segoe UI", 10F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PreLogonScreen";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Login to Teleopti WFM";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvSDKList)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelChooseSDK;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvSDKList;
	}
}