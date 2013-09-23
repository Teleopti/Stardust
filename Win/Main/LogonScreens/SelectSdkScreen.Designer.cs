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
			this.buttonDataSourcesListCancel = new System.Windows.Forms.Button();
			this.buttonDataSourceListOK = new System.Windows.Forms.Button();
			this.lbxSelectSDK = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// labelChooseDataSource
			// 
			this.labelChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelChooseDataSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelChooseDataSource.Location = new System.Drawing.Point(9, 38);
			this.labelChooseDataSource.Name = "labelChooseDataSource";
			this.labelChooseDataSource.Size = new System.Drawing.Size(472, 23);
			this.labelChooseDataSource.TabIndex = 36;
			this.labelChooseDataSource.Text = "xxPlease choose SDK";
			this.labelChooseDataSource.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonDataSourcesListCancel
			// 
			this.buttonDataSourcesListCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDataSourcesListCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonDataSourcesListCancel.Location = new System.Drawing.Point(378, 275);
			this.buttonDataSourcesListCancel.Name = "buttonDataSourcesListCancel";
			this.buttonDataSourcesListCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonDataSourcesListCancel.TabIndex = 35;
			this.buttonDataSourcesListCancel.Text = "xxCancel";
			this.buttonDataSourcesListCancel.UseVisualStyleBackColor = true;
			this.buttonDataSourcesListCancel.Click += new System.EventHandler(this.buttonDataSourcesListCancel_Click);
			// 
			// buttonDataSourceListOK
			// 
			this.buttonDataSourceListOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDataSourceListOK.Location = new System.Drawing.Point(297, 275);
			this.buttonDataSourceListOK.Name = "buttonDataSourceListOK";
			this.buttonDataSourceListOK.Size = new System.Drawing.Size(75, 23);
			this.buttonDataSourceListOK.TabIndex = 34;
			this.buttonDataSourceListOK.Text = "xxOK";
			this.buttonDataSourceListOK.UseVisualStyleBackColor = true;
			this.buttonDataSourceListOK.Click += new System.EventHandler(this.buttonDataSourceListOK_Click);
			// 
			// lbxSelectSDK
			// 
			this.lbxSelectSDK.FormattingEnabled = true;
			this.lbxSelectSDK.Location = new System.Drawing.Point(50, 64);
			this.lbxSelectSDK.Name = "lbxSelectSDK";
			this.lbxSelectSDK.Size = new System.Drawing.Size(343, 173);
			this.lbxSelectSDK.TabIndex = 37;
			// 
			// SelectSdkScreen
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lbxSelectSDK);
			this.Controls.Add(this.labelChooseDataSource);
			this.Controls.Add(this.buttonDataSourcesListCancel);
			this.Controls.Add(this.buttonDataSourceListOK);
			this.Name = "SelectSdkScreen";
			this.Size = new System.Drawing.Size(490, 337);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelChooseDataSource;
		private System.Windows.Forms.Button buttonDataSourcesListCancel;
		private System.Windows.Forms.Button buttonDataSourceListOK;
		private System.Windows.Forms.ListBox lbxSelectSDK;
	}
}
