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
			this.components = new System.ComponentModel.Container();
			this.tabControlChooseDataSource = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.tabPageWindowsDataSources = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.listBoxWindowsDataSources = new System.Windows.Forms.ListBox();
			this.tabPageApplicationDataSources = new Syncfusion.Windows.Forms.Tools.TabPageAdv();
			this.listBoxApplicationDataSources = new System.Windows.Forms.ListBox();
			this.labelChooseDataSource = new System.Windows.Forms.Label();
			this.buttonLogOnOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonLogOnCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.btnBack = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.tabControlChooseDataSource)).BeginInit();
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
			this.tabControlChooseDataSource.BeforeTouchSize = new System.Drawing.Size(352, 140);
			this.tabControlChooseDataSource.Controls.Add(this.tabPageWindowsDataSources);
			this.tabControlChooseDataSource.Controls.Add(this.tabPageApplicationDataSources);
			this.tabControlChooseDataSource.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControlChooseDataSource.InactiveTabColor = System.Drawing.Color.White;
			this.tabControlChooseDataSource.Location = new System.Drawing.Point(71, 45);
			this.tabControlChooseDataSource.Margin = new System.Windows.Forms.Padding(0);
			this.tabControlChooseDataSource.Name = "tabControlChooseDataSource";
			this.tabControlChooseDataSource.Size = new System.Drawing.Size(352, 140);
			this.tabControlChooseDataSource.TabIndex = 1;
			this.tabControlChooseDataSource.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlChooseDataSource.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// tabPageWindowsDataSources
			// 
			this.tabPageWindowsDataSources.Controls.Add(this.listBoxWindowsDataSources);
			this.tabPageWindowsDataSources.Image = null;
			this.tabPageWindowsDataSources.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageWindowsDataSources.Location = new System.Drawing.Point(1, 22);
			this.tabPageWindowsDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageWindowsDataSources.Name = "tabPageWindowsDataSources";
			this.tabPageWindowsDataSources.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageWindowsDataSources.ShowCloseButton = true;
			this.tabPageWindowsDataSources.Size = new System.Drawing.Size(349, 116);
			this.tabPageWindowsDataSources.TabBackColor = System.Drawing.Color.White;
			this.tabPageWindowsDataSources.TabIndex = 1;
			this.tabPageWindowsDataSources.Text = "xxWindows logon";
			this.tabPageWindowsDataSources.ThemesEnabled = false;
			// 
			// listBoxWindowsDataSources
			// 
			this.listBoxWindowsDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBoxWindowsDataSources.DisplayMember = "DataSourceName";
			this.listBoxWindowsDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxWindowsDataSources.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBoxWindowsDataSources.FormattingEnabled = true;
			this.listBoxWindowsDataSources.ItemHeight = 17;
			this.listBoxWindowsDataSources.Location = new System.Drawing.Point(3, 3);
			this.listBoxWindowsDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.listBoxWindowsDataSources.Name = "listBoxWindowsDataSources";
			this.listBoxWindowsDataSources.Size = new System.Drawing.Size(343, 110);
			this.listBoxWindowsDataSources.TabIndex = 4;
			// 
			// tabPageApplicationDataSources
			// 
			this.tabPageApplicationDataSources.Controls.Add(this.listBoxApplicationDataSources);
			this.tabPageApplicationDataSources.Image = null;
			this.tabPageApplicationDataSources.ImageSize = new System.Drawing.Size(16, 16);
			this.tabPageApplicationDataSources.Location = new System.Drawing.Point(1, 22);
			this.tabPageApplicationDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.tabPageApplicationDataSources.Name = "tabPageApplicationDataSources";
			this.tabPageApplicationDataSources.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageApplicationDataSources.ShowCloseButton = true;
			this.tabPageApplicationDataSources.Size = new System.Drawing.Size(349, 116);
			this.tabPageApplicationDataSources.TabBackColor = System.Drawing.Color.White;
			this.tabPageApplicationDataSources.TabIndex = 1;
			this.tabPageApplicationDataSources.Text = "xxApplication logon";
			this.tabPageApplicationDataSources.ThemesEnabled = false;
			// 
			// listBoxApplicationDataSources
			// 
			this.listBoxApplicationDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBoxApplicationDataSources.DisplayMember = "DataSourceName";
			this.listBoxApplicationDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxApplicationDataSources.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.listBoxApplicationDataSources.FormattingEnabled = true;
			this.listBoxApplicationDataSources.ItemHeight = 17;
			this.listBoxApplicationDataSources.Location = new System.Drawing.Point(3, 3);
			this.listBoxApplicationDataSources.Margin = new System.Windows.Forms.Padding(0);
			this.listBoxApplicationDataSources.Name = "listBoxApplicationDataSources";
			this.listBoxApplicationDataSources.Size = new System.Drawing.Size(343, 110);
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
			this.labelChooseDataSource.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this.labelChooseDataSource.Size = new System.Drawing.Size(483, 245);
			this.labelChooseDataSource.TabIndex = 36;
			this.labelChooseDataSource.Text = "xxPlease choose a datasource";
			this.labelChooseDataSource.TextAlign = System.Drawing.ContentAlignment.TopCenter;
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
			this.buttonLogOnOK.Location = new System.Drawing.Point(263, 190);
			this.buttonLogOnOK.Name = "buttonLogOnOK";
			this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnOK.TabIndex = 0;
			this.buttonLogOnOK.Text = "xxOK";
			this.buttonLogOnOK.UseVisualStyleBackColor = false;
			this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOK_Click);
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
			this.buttonLogOnCancel.Location = new System.Drawing.Point(344, 190);
			this.buttonLogOnCancel.Name = "buttonLogOnCancel";
			this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonLogOnCancel.TabIndex = 2;
			this.buttonLogOnCancel.Text = "xxCancel";
			this.buttonLogOnCancel.UseVisualStyleBackColor = false;
			this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancel_Click);
			// 
			// btnBack
			// 
			this.btnBack.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.btnBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.btnBack.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnBack.ForeColor = System.Drawing.Color.White;
			this.btnBack.IsBackStageButton = false;
			this.btnBack.Location = new System.Drawing.Point(182, 190);
			this.btnBack.Name = "btnBack";
			this.btnBack.Size = new System.Drawing.Size(75, 23);
			this.btnBack.TabIndex = 3;
			this.btnBack.Text = "xxBack";
			this.btnBack.UseVisualStyleBackColor = false;
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
			((System.ComponentModel.ISupportInitialize)(this.tabControlChooseDataSource)).EndInit();
			this.tabControlChooseDataSource.ResumeLayout(false);
			this.tabPageWindowsDataSources.ResumeLayout(false);
			this.tabPageApplicationDataSources.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlChooseDataSource;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageWindowsDataSources;
		private System.Windows.Forms.ListBox listBoxWindowsDataSources;
		private Syncfusion.Windows.Forms.Tools.TabPageAdv tabPageApplicationDataSources;
		private System.Windows.Forms.ListBox listBoxApplicationDataSources;
        private System.Windows.Forms.Label labelChooseDataSource;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnOK;
		private Syncfusion.Windows.Forms.ButtonAdv buttonLogOnCancel;
		private Syncfusion.Windows.Forms.ButtonAdv btnBack;


	}
}
