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
			this.panelLogin = new System.Windows.Forms.Panel();
			this.btnBack = new System.Windows.Forms.Button();
			this.labelLogOn = new System.Windows.Forms.Label();
			this.textBoxPassword = new System.Windows.Forms.TextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.textBoxLogOnName = new System.Windows.Forms.TextBox();
			this.labelLoginName = new System.Windows.Forms.Label();
			this.buttonLogOnCancel = new System.Windows.Forms.Button();
			this.buttonLogOnOK = new System.Windows.Forms.Button();
			this.panelLogin.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelLogin
			// 
			this.panelLogin.BackColor = System.Drawing.Color.White;
			this.panelLogin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelLogin.Controls.Add(this.btnBack);
			this.panelLogin.Controls.Add(this.labelLogOn);
			this.panelLogin.Controls.Add(this.textBoxPassword);
			this.panelLogin.Controls.Add(this.labelPassword);
			this.panelLogin.Controls.Add(this.textBoxLogOnName);
			this.panelLogin.Controls.Add(this.labelLoginName);
			this.panelLogin.Controls.Add(this.buttonLogOnCancel);
			this.panelLogin.Controls.Add(this.buttonLogOnOK);
			this.panelLogin.Location = new System.Drawing.Point(3, 3);
			this.panelLogin.Name = "panelLogin";
			this.panelLogin.Size = new System.Drawing.Size(484, 331);
			this.panelLogin.TabIndex = 32;
			this.panelLogin.Visible = false;
			// 
			// btnBack
			// 
			this.btnBack.Location = new System.Drawing.Point(178, 262);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 23);
			this.btnBack.TabIndex = 30;
			this.btnBack.Text = "xxBack";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// labelLogOn
			// 
			this.labelLogOn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelLogOn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelLogOn.Location = new System.Drawing.Point(31, 25);
			this.labelLogOn.Name = "labelLogOn";
			this.labelLogOn.Size = new System.Drawing.Size(410, 23);
			this.labelLogOn.TabIndex = 29;
			this.labelLogOn.Text = "xxPlease enter your logon credentials";
			this.labelLogOn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textBoxPassword
			// 
			this.textBoxPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxPassword.Location = new System.Drawing.Point(151, 105);
			this.textBoxPassword.Name = "textBoxPassword";
			this.textBoxPassword.PasswordChar = '*';
			this.textBoxPassword.Size = new System.Drawing.Size(264, 20);
			this.textBoxPassword.TabIndex = 1;
			// 
			// labelPassword
			// 
			this.labelPassword.AutoSize = true;
			this.labelPassword.Location = new System.Drawing.Point(60, 108);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(66, 13);
			this.labelPassword.TabIndex = 27;
			this.labelPassword.Text = "xxPassword:";
			// 
			// textBoxLogOnName
			// 
			this.textBoxLogOnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxLogOnName.Location = new System.Drawing.Point(151, 79);
			this.textBoxLogOnName.Name = "textBoxLogOnName";
			this.textBoxLogOnName.Size = new System.Drawing.Size(264, 20);
			this.textBoxLogOnName.TabIndex = 0;
			// 
			// labelLoginName
			// 
			this.labelLoginName.AutoSize = true;
			this.labelLoginName.Location = new System.Drawing.Point(60, 82);
			this.labelLoginName.Name = "labelLoginName";
			this.labelLoginName.Size = new System.Drawing.Size(75, 13);
			this.labelLoginName.TabIndex = 25;
			this.labelLoginName.Text = "xxLogin name:";
			// 
			// buttonLogOnCancel
			// 
			this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonLogOnCancel.Location = new System.Drawing.Point(340, 262);
			this.buttonLogOnCancel.Name = "buttonLogOnCancel";
			this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.TabIndex = 3;
			this.buttonLogOnCancel.Text = "xxCancel";
			this.buttonLogOnCancel.UseVisualStyleBackColor = true;
			this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Location = new System.Drawing.Point(259, 262);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.TabIndex = 2;
			this.buttonLogOnOK.Text = "xxOK";
			this.buttonLogOnOK.UseVisualStyleBackColor = true;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// LoginScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelLogin);
			this.Name = "LoginScreen";
			this.Size = new System.Drawing.Size(490, 337);
			this.panelLogin.ResumeLayout(false);
			this.panelLogin.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelLogin;
		private System.Windows.Forms.Label labelLogOn;
		internal System.Windows.Forms.TextBox textBoxPassword;
		private System.Windows.Forms.Label labelPassword;
		internal System.Windows.Forms.TextBox textBoxLogOnName;
		private System.Windows.Forms.Label labelLoginName;
		private System.Windows.Forms.Button buttonLogOnCancel;
		private System.Windows.Forms.Button buttonLogOnOK;
		private System.Windows.Forms.Button btnBack;
	}
}
