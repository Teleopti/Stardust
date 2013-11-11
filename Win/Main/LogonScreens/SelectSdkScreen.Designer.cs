namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	partial class SelectSdkScreen
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
			this.labelChooseSDK = new System.Windows.Forms.Label();
			this.lbxSelectSDK = new System.Windows.Forms.ListBox();
			this.btnBack = new System.Windows.Forms.Button();
			this.buttonLogOnCancel = new System.Windows.Forms.Button();
			this.buttonLogOnOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelChooseSDK
			// 
			this.labelChooseSDK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelChooseSDK.BackColor = System.Drawing.Color.Transparent;
			this.labelChooseSDK.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelChooseSDK.Location = new System.Drawing.Point(0, 0);
			this.labelChooseSDK.Name = "labelChooseSDK";
			this.labelChooseSDK.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
			this.labelChooseSDK.Size = new System.Drawing.Size(483, 296);
			this.labelChooseSDK.TabIndex = 36;
			this.labelChooseSDK.Text = "xxPlease choose SDK";
			this.labelChooseSDK.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lbxSelectSDK
			// 
			this.lbxSelectSDK.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbxSelectSDK.FormattingEnabled = true;
			this.lbxSelectSDK.ItemHeight = 17;
			this.lbxSelectSDK.Location = new System.Drawing.Point(71, 51);
			this.lbxSelectSDK.Name = "lbxSelectSDK";
			this.lbxSelectSDK.Size = new System.Drawing.Size(352, 140);
			this.lbxSelectSDK.TabIndex = 37;
			// 
			// btnBack
			// 
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.Location = new System.Drawing.Point(186, 197);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 23);
			this.btnBack.TabIndex = 42;
			this.btnBack.Text = "xxBack";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// buttonLogOnCancel
			// 
			this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonLogOnCancel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnCancel.Location = new System.Drawing.Point(348, 197);
			this.buttonLogOnCancel.Name = "buttonLogOnCancel";
			this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.TabIndex = 41;
			this.buttonLogOnCancel.Text = "xxCancel";
			this.buttonLogOnCancel.UseVisualStyleBackColor = true;
			this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.Location = new System.Drawing.Point(267, 197);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.TabIndex = 40;
			this.buttonLogOnOK.Text = "xxOK";
			this.buttonLogOnOK.UseVisualStyleBackColor = true;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// SelectSdkScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.buttonLogOnCancel);
			this.Controls.Add(this.buttonLogOnOK);
			this.Controls.Add(this.lbxSelectSDK);
			this.Controls.Add(this.labelChooseSDK);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectSdkScreen";
			this.Size = new System.Drawing.Size(483, 296);
			this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Label labelChooseSDK;
		private System.Windows.Forms.ListBox lbxSelectSDK;
		private System.Windows.Forms.Button btnBack;
		private System.Windows.Forms.Button buttonLogOnCancel;
		private System.Windows.Forms.Button buttonLogOnOK;
	}
}
