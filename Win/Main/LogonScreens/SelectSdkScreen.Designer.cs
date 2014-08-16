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
			this.components = new System.ComponentModel.Container();
			this.labelChooseSDK = new System.Windows.Forms.Label();
			this.comboBoxAdvSDKList = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvSDKList)).BeginInit();
			this.SuspendLayout();
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
			this.labelChooseSDK.Size = new System.Drawing.Size(101, 25);
			this.labelChooseSDK.TabIndex = 36;
			this.labelChooseSDK.Text = "Select SDK";
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
			this.comboBoxAdvSDKList.TabIndex = 43;
			// 
			// btnBack
			// 
			this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.White;
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.Image = global::Teleopti.Ccc.Win.Properties.Resources.left_round_32;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(209, 107);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(35, 35);
			this.btnBack.TabIndex = 42;
			this.btnBack.UseVisualStyleBackColor = false;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonLogOnOK.BackColor = System.Drawing.Color.White;
			this.buttonLogOnOK.BeforeTouchSize = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.ForeColor = System.Drawing.Color.White;
			this.buttonLogOnOK.Image = global::Teleopti.Ccc.Win.Properties.Resources.right_round_32;
			this.buttonLogOnOK.IsBackStageButton = false;
			this.buttonLogOnOK.Location = new System.Drawing.Point(250, 107);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(35, 35);
			this.buttonLogOnOK.TabIndex = 40;
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// SelectSdkScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.comboBoxAdvSDKList);
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.buttonLogOnOK);
			this.Controls.Add(this.labelChooseSDK);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectSdkScreen";
			this.Size = new System.Drawing.Size(285, 142);
			((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvSDKList)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelChooseSDK;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
		private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvSDKList;
	}
}
