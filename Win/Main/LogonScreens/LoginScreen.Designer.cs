namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	partial class LoginScreen
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
			this.labelLogOn = new System.Windows.Forms.Label();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.textBoxLogOnName = new System.Windows.Forms.TextBox();
			this.labelLoginName = new System.Windows.Forms.Label();
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.SuspendLayout();
			// 
			// labelLogOn
			// 
			this.labelLogOn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelLogOn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelLogOn.Location = new System.Drawing.Point(0, 0);
			this.labelLogOn.Name = "labelLogOn";
			this.labelLogOn.Padding = new System.Windows.Forms.Padding(0, 30, 0, 0);
			this.labelLogOn.Size = new System.Drawing.Size(483, 245);
			this.labelLogOn.TabIndex = 40;
			this.labelLogOn.Text = "xxPlease enter your logon credentials";
			this.labelLogOn.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxPassword.Location = new System.Drawing.Point(157, 108);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.PasswordChar = '*';
			this.textBoxPassword.Size = new System.Drawing.Size(262, 22);
			this.textBoxPassword.TabIndex = 1;
			// 
			// labelPassword
			// 
			this.labelPassword.AutoSize = true;
			this.labelPassword.Location = new System.Drawing.Point(66, 111);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(69, 13);
			this.labelPassword.TabIndex = 39;
			this.labelPassword.Text = "xxPassword:";
			// 
			// textBoxLogOnName
			// 
			this.textBoxLogOnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLogOnName.Location = new System.Drawing.Point(157, 82);
			this.textBoxLogOnName.Name = "textBoxLogOnName";
			this.textBoxLogOnName.Size = new System.Drawing.Size(262, 22);
			this.textBoxLogOnName.TabIndex = 0;
			// 
			// labelLoginName
			// 
			this.labelLoginName.AutoSize = true;
			this.labelLoginName.Location = new System.Drawing.Point(66, 85);
			this.labelLoginName.Name = "labelLoginName";
			this.labelLoginName.Size = new System.Drawing.Size(80, 13);
			this.labelLoginName.TabIndex = 38;
			this.labelLoginName.Text = "xxLogin name:";
			// 
			// btnBack
			// 
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(183, 198);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 23);
			this.btnBack.TabIndex = 43;
			this.btnBack.Text = "xxBack";
			this.btnBack.UseVisualStyleBackColor = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// buttonLogOnCancel
			// 
			this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.buttonLogOnCancel.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonLogOnCancel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnCancel.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnCancel.IsBackStageButton = false;
			this.buttonLogOnCancel.Location = new System.Drawing.Point(345, 198);
			this.buttonLogOnCancel.Name = "buttonLogOnCancel";
			this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.TabIndex = 42;
			this.buttonLogOnCancel.Text = "xxCancel";
			this.buttonLogOnCancel.UseVisualStyleBackColor = false;
			this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.buttonLogOnOK.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnOK.IsBackStageButton = false;
			this.buttonLogOnOK.Location = new System.Drawing.Point(264, 198);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.TabIndex = 41;
			this.buttonLogOnOK.Text = "xxOK";
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// LoginScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.buttonLogOnCancel);
			this.Controls.Add(this.buttonLogOnOK);
			this.Controls.Add(this.textBoxPassword);
			this.Controls.Add(this.labelPassword);
			this.Controls.Add(this.textBoxLogOnName);
			this.Controls.Add(this.labelLoginName);
			this.Controls.Add(this.labelLogOn);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "LoginScreen";
			this.Size = new System.Drawing.Size(483, 296);
			this.Enter += new System.EventHandler(this.LoginScreen_Enter);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Label labelLogOn;
        internal System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        internal System.Windows.Forms.TextBox textBoxLogOnName;
        private System.Windows.Forms.Label labelLoginName;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
	}
}
