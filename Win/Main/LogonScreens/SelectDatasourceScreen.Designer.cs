namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	partial class SelectDatasourceScreen
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
			this.tabControlChooseDataSource = new System.Windows.Forms.TabControl();
			this.tabPageWindowsDataSources = new System.Windows.Forms.TabPage();
			this.listBoxWindowsDataSources = new System.Windows.Forms.ListBox();
			this.tabPageApplicationDataSources = new System.Windows.Forms.TabPage();
			this.listBoxApplicationDataSources = new System.Windows.Forms.ListBox();
			this.labelChooseDataSource = new System.Windows.Forms.Label();
			this.buttonLogOnOK = new System.Windows.Forms.Button();
			this.buttonLogOnCancel = new System.Windows.Forms.Button();
			this.btnBack = new System.Windows.Forms.Button();
			this.tabControlChooseDataSource.SuspendLayout();
			this.tabPageWindowsDataSources.SuspendLayout();
			this.tabPageApplicationDataSources.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlChooseDataSource
			// 
			this.tabControlChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlChooseDataSource.Controls.Add(this.tabPageWindowsDataSources);
			this.tabControlChooseDataSource.Controls.Add(this.tabPageApplicationDataSources);
			this.tabControlChooseDataSource.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControlChooseDataSource.Location = new System.Drawing.Point(71, 51);
			this.tabControlChooseDataSource.Margin = new System.Windows.Forms.Padding(0);
			this.tabControlChooseDataSource.Name = "tabControlChooseDataSource";
			this.tabControlChooseDataSource.SelectedIndex = 0;
			this.tabControlChooseDataSource.Size = new System.Drawing.Size(352, 140);
			this.tabControlChooseDataSource.TabIndex = 0;
			// 
			// tabPageWindowsDataSources
			// 
			this.tabPageWindowsDataSources.Controls.Add(this.listBoxWindowsDataSources);
			this.tabPageWindowsDataSources.Location = new System.Drawing.Point(4, 26);
			this.tabPageWindowsDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageWindowsDataSources.Name = "tabPageWindowsDataSources";
			this.tabPageWindowsDataSources.Size = new System.Drawing.Size(344, 110);
			this.tabPageWindowsDataSources.TabIndex = 0;
			this.tabPageWindowsDataSources.Text = "xxWindows logon";
			this.tabPageWindowsDataSources.UseVisualStyleBackColor = true;
			// 
			// listBoxWindowsDataSources
			// 
			this.listBoxWindowsDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBoxWindowsDataSources.DisplayMember = "DataSourceName";
			this.listBoxWindowsDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxWindowsDataSources.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBoxWindowsDataSources.FormattingEnabled = true;
			this.listBoxWindowsDataSources.ItemHeight = 17;
			this.listBoxWindowsDataSources.Location = new System.Drawing.Point(0, 0);
			this.listBoxWindowsDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.listBoxWindowsDataSources.Name = "listBoxWindowsDataSources";
			this.listBoxWindowsDataSources.Size = new System.Drawing.Size(344, 110);
			this.listBoxWindowsDataSources.TabIndex = 3;
			// 
			// tabPageApplicationDataSources
			// 
			this.tabPageApplicationDataSources.Controls.Add(this.listBoxApplicationDataSources);
			this.tabPageApplicationDataSources.Location = new System.Drawing.Point(4, 26);
			this.tabPageApplicationDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageApplicationDataSources.Name = "tabPageApplicationDataSources";
			this.tabPageApplicationDataSources.Size = new System.Drawing.Size(344, 110);
			this.tabPageApplicationDataSources.TabIndex = 1;
			this.tabPageApplicationDataSources.Text = "xxApplication logon";
			this.tabPageApplicationDataSources.UseVisualStyleBackColor = true;
			// 
			// listBoxApplicationDataSources
			// 
			this.listBoxApplicationDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBoxApplicationDataSources.DisplayMember = "DataSourceName";
			this.listBoxApplicationDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxApplicationDataSources.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBoxApplicationDataSources.FormattingEnabled = true;
			this.listBoxApplicationDataSources.ItemHeight = 17;
			this.listBoxApplicationDataSources.Location = new System.Drawing.Point(0, 0);
			this.listBoxApplicationDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.listBoxApplicationDataSources.Name = "listBoxApplicationDataSources";
			this.listBoxApplicationDataSources.Size = new System.Drawing.Size(344, 110);
			this.listBoxApplicationDataSources.TabIndex = 4;
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
			this.labelChooseDataSource.Size = new System.Drawing.Size(483, 245);
			this.labelChooseDataSource.TabIndex = 36;
			this.labelChooseDataSource.Text = "xxPlease choose a datasource";
			this.labelChooseDataSource.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// buttonLogOnOK
			// 
			this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnOK.Location = new System.Drawing.Point(263, 194);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.TabIndex = 37;
			this.buttonLogOnOK.Text = "xxOK";
			this.buttonLogOnOK.UseVisualStyleBackColor = true;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
			// 
			// buttonLogOnCancel
			// 
			this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonLogOnCancel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonLogOnCancel.Location = new System.Drawing.Point(344, 194);
			this.buttonLogOnCancel.Name = "buttonLogOnCancel";
			this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.TabIndex = 38;
			this.buttonLogOnCancel.Text = "xxCancel";
			this.buttonLogOnCancel.UseVisualStyleBackColor = true;
			this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
			// 
			// btnBack
			// 
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.Location = new System.Drawing.Point(182, 194);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 23);
			this.btnBack.TabIndex = 39;
			this.btnBack.Text = "xxBack";
			this.btnBack.UseVisualStyleBackColor = true;
			this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
			// 
			// SelectDatasourceScreen
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.btnBack);
			this.Controls.Add(this.buttonLogOnCancel);
			this.Controls.Add(this.buttonLogOnOK);
			this.Controls.Add(this.tabControlChooseDataSource);
			this.Controls.Add(this.labelChooseDataSource);
			this.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "SelectDatasourceScreen";
			this.Size = new System.Drawing.Size(483, 296);
			this.tabControlChooseDataSource.ResumeLayout(false);
			this.tabPageWindowsDataSources.ResumeLayout(false);
			this.tabPageApplicationDataSources.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControlChooseDataSource;
		private System.Windows.Forms.TabPage tabPageWindowsDataSources;
		private System.Windows.Forms.ListBox listBoxWindowsDataSources;
		private System.Windows.Forms.TabPage tabPageApplicationDataSources;
		private System.Windows.Forms.ListBox listBoxApplicationDataSources;
        private System.Windows.Forms.Label labelChooseDataSource;
		private System.Windows.Forms.Button buttonLogOnOK;
		private System.Windows.Forms.Button buttonLogOnCancel;
		private System.Windows.Forms.Button btnBack;


	}
}
