namespace Teleopti.Ccc.Win.Main.LogonScreens
{
	partial class SelectDatasource
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
			this.panelChooseDataSource = new System.Windows.Forms.Panel();
			this.tabControlChooseDataSource = new System.Windows.Forms.TabControl();
			this.tabPageWindowsDataSources = new System.Windows.Forms.TabPage();
			this.listBoxWindowsDataSources = new System.Windows.Forms.ListBox();
			this.tabPageApplicationDataSources = new System.Windows.Forms.TabPage();
			this.listBoxApplicationDataSources = new System.Windows.Forms.ListBox();
			this.labelChooseDataSource = new System.Windows.Forms.Label();
			this.buttonDataSourcesListCancel = new System.Windows.Forms.Button();
			this.buttonDataSourceListOK = new System.Windows.Forms.Button();
			this.panelChooseDataSource.SuspendLayout();
			this.tabControlChooseDataSource.SuspendLayout();
			this.tabPageWindowsDataSources.SuspendLayout();
			this.tabPageApplicationDataSources.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelChooseDataSource
			// 
			this.panelChooseDataSource.BackColor = System.Drawing.Color.White;
			this.panelChooseDataSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelChooseDataSource.Controls.Add(this.tabControlChooseDataSource);
			this.panelChooseDataSource.Controls.Add(this.labelChooseDataSource);
			this.panelChooseDataSource.Controls.Add(this.buttonDataSourcesListCancel);
			this.panelChooseDataSource.Controls.Add(this.buttonDataSourceListOK);
			this.panelChooseDataSource.Location = new System.Drawing.Point(3, 3);
			this.panelChooseDataSource.Name = "panelChooseDataSource";
			this.panelChooseDataSource.Size = new System.Drawing.Size(484, 331);
			this.panelChooseDataSource.TabIndex = 35;
			this.panelChooseDataSource.Visible = false;
			// 
			// tabControlChooseDataSource
			// 
			this.tabControlChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlChooseDataSource.Controls.Add(this.tabPageWindowsDataSources);
			this.tabControlChooseDataSource.Controls.Add(this.tabPageApplicationDataSources);
			this.tabControlChooseDataSource.Location = new System.Drawing.Point(63, 51);
			this.tabControlChooseDataSource.Name = "tabControlChooseDataSource";
			this.tabControlChooseDataSource.SelectedIndex = 0;
			this.tabControlChooseDataSource.Size = new System.Drawing.Size(357, 206);
			this.tabControlChooseDataSource.TabIndex = 33;
			// 
			// tabPageWindowsDataSources
			// 
			this.tabPageWindowsDataSources.Controls.Add(this.listBoxWindowsDataSources);
			this.tabPageWindowsDataSources.Location = new System.Drawing.Point(4, 22);
			this.tabPageWindowsDataSources.Name = "tabPageWindowsDataSources";
			this.tabPageWindowsDataSources.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageWindowsDataSources.Size = new System.Drawing.Size(349, 180);
			this.tabPageWindowsDataSources.TabIndex = 0;
			this.tabPageWindowsDataSources.Text = "xxWindows logon";
			this.tabPageWindowsDataSources.UseVisualStyleBackColor = true;
			// 
			// listBoxWindowsDataSources
			// 
			this.listBoxWindowsDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBoxWindowsDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxWindowsDataSources.FormattingEnabled = true;
			this.listBoxWindowsDataSources.Location = new System.Drawing.Point(3, 3);
			this.listBoxWindowsDataSources.Name = "listBoxWindowsDataSources";
			this.listBoxWindowsDataSources.Size = new System.Drawing.Size(343, 174);
			this.listBoxWindowsDataSources.TabIndex = 3;
			// 
			// tabPageApplicationDataSources
			// 
			this.tabPageApplicationDataSources.Controls.Add(this.listBoxApplicationDataSources);
			this.tabPageApplicationDataSources.Location = new System.Drawing.Point(4, 22);
			this.tabPageApplicationDataSources.Name = "tabPageApplicationDataSources";
			this.tabPageApplicationDataSources.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageApplicationDataSources.Size = new System.Drawing.Size(142, 48);
			this.tabPageApplicationDataSources.TabIndex = 1;
			this.tabPageApplicationDataSources.Text = "xxApplication logon";
			this.tabPageApplicationDataSources.UseVisualStyleBackColor = true;
			// 
			// listBoxApplicationDataSources
			// 
			this.listBoxApplicationDataSources.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBoxApplicationDataSources.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxApplicationDataSources.FormattingEnabled = true;
			this.listBoxApplicationDataSources.Location = new System.Drawing.Point(3, 3);
			this.listBoxApplicationDataSources.Name = "listBoxApplicationDataSources";
			this.listBoxApplicationDataSources.Size = new System.Drawing.Size(136, 42);
			this.listBoxApplicationDataSources.TabIndex = 4;
			// 
			// labelChooseDataSource
			// 
			this.labelChooseDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelChooseDataSource.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelChooseDataSource.Location = new System.Drawing.Point(31, 25);
			this.labelChooseDataSource.Name = "labelChooseDataSource";
			this.labelChooseDataSource.Size = new System.Drawing.Size(412, 23);
			this.labelChooseDataSource.TabIndex = 29;
			this.labelChooseDataSource.Text = "xxPlease choose a datasource";
			this.labelChooseDataSource.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// buttonDataSourcesListCancel
			// 
			this.buttonDataSourcesListCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDataSourcesListCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonDataSourcesListCancel.Location = new System.Drawing.Point(340, 262);
			this.buttonDataSourcesListCancel.Name = "buttonDataSourcesListCancel";
			this.buttonDataSourcesListCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonDataSourcesListCancel.TabIndex = 2;
			this.buttonDataSourcesListCancel.Text = "xxCancel";
			this.buttonDataSourcesListCancel.UseVisualStyleBackColor = true;
			// 
			// buttonDataSourceListOK
			// 
			this.buttonDataSourceListOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonDataSourceListOK.Location = new System.Drawing.Point(259, 262);
			this.buttonDataSourceListOK.Name = "buttonDataSourceListOK";
			this.buttonDataSourceListOK.Size = new System.Drawing.Size(75, 23);
			this.buttonDataSourceListOK.TabIndex = 1;
			this.buttonDataSourceListOK.Text = "xxOK";
			this.buttonDataSourceListOK.UseVisualStyleBackColor = true;
			// 
			// SelectBusinnesUnit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panelChooseDataSource);
			this.Name = "SelectBusinnesUnit";
			this.Size = new System.Drawing.Size(490, 337);
			this.panelChooseDataSource.ResumeLayout(false);
			this.tabControlChooseDataSource.ResumeLayout(false);
			this.tabPageWindowsDataSources.ResumeLayout(false);
			this.tabPageApplicationDataSources.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelChooseDataSource;
		private System.Windows.Forms.TabControl tabControlChooseDataSource;
		private System.Windows.Forms.TabPage tabPageWindowsDataSources;
		private System.Windows.Forms.ListBox listBoxWindowsDataSources;
		private System.Windows.Forms.TabPage tabPageApplicationDataSources;
		private System.Windows.Forms.ListBox listBoxApplicationDataSources;
		private System.Windows.Forms.Label labelChooseDataSource;
		private System.Windows.Forms.Button buttonDataSourcesListCancel;
		private System.Windows.Forms.Button buttonDataSourceListOK;

	}
}
