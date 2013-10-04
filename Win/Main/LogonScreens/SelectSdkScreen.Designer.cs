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
            this.labelChooseDataSource = new System.Windows.Forms.Label();
            this.lbxSelectSDK = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // labelChooseDataSource
            // 
            this.labelChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelChooseDataSource.BackColor = System.Drawing.Color.Transparent;
            this.labelChooseDataSource.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelChooseDataSource.Location = new System.Drawing.Point(0, 0);
            this.labelChooseDataSource.Name = "labelChooseDataSource";
            this.labelChooseDataSource.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.labelChooseDataSource.Size = new System.Drawing.Size(483, 296);
            this.labelChooseDataSource.TabIndex = 36;
            this.labelChooseDataSource.Text = "xxPlease choose SDK";
            this.labelChooseDataSource.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbxSelectSDK
            // 
            this.lbxSelectSDK.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbxSelectSDK.FormattingEnabled = true;
            this.lbxSelectSDK.ItemHeight = 17;
            this.lbxSelectSDK.Location = new System.Drawing.Point(71, 51);
            this.lbxSelectSDK.Name = "lbxSelectSDK";
            this.lbxSelectSDK.Size = new System.Drawing.Size(352, 191);
            this.lbxSelectSDK.TabIndex = 37;
            // 
            // SelectSdkScreen
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.CornflowerBlue;
            this.Controls.Add(this.lbxSelectSDK);
            this.Controls.Add(this.labelChooseDataSource);
            this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SelectSdkScreen";
            this.Size = new System.Drawing.Size(483, 345);
            this.Load += new System.EventHandler(this.selectSdkScreenLoad);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Label labelChooseDataSource;
		private System.Windows.Forms.ListBox lbxSelectSDK;
	}
}
