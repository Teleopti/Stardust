namespace Teleopti.Support.Tool.Controls
{
    partial class ManageDatabaseVersions
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Demo_TeleoptiAnalytics", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Demo_TeleoptiCCC7", 0);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Demo_TeleoptiCCCAgg", 1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageDatabaseVersions));
            this.listViewDatabases = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListIconsForListview = new System.Windows.Forms.ImageList(this.components);
            this.labelNHibFolder = new System.Windows.Forms.Label();
            this.textBoxNHibFolder = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.folderBrowserDialogNHib = new System.Windows.Forms.FolderBrowserDialog();
            this.smoothLabelCurrentVersionIs = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.smoothLabelCurrentVersion = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.labelManageDatabaseVersions = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.SuspendLayout();
            // 
            // listViewDatabases
            // 
            this.listViewDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewDatabases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderType,
            this.columnHeaderVersion});
            this.listViewDatabases.FullRowSelect = true;
            this.listViewDatabases.HideSelection = false;
            this.listViewDatabases.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.listViewDatabases.Location = new System.Drawing.Point(14, 83);
            this.listViewDatabases.Margin = new System.Windows.Forms.Padding(5);
            this.listViewDatabases.MultiSelect = false;
            this.listViewDatabases.Name = "listViewDatabases";
            this.listViewDatabases.Size = new System.Drawing.Size(651, 199);
            this.listViewDatabases.SmallImageList = this.imageListIconsForListview;
            this.listViewDatabases.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewDatabases.TabIndex = 0;
            this.listViewDatabases.UseCompatibleStateImageBehavior = false;
            this.listViewDatabases.View = System.Windows.Forms.View.Details;
            this.listViewDatabases.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewDatabases_ItemSelectionChanged);
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Database Name";
            this.columnHeaderName.Width = 400;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            this.columnHeaderType.Width = 120;
            // 
            // columnHeaderVersion
            // 
            this.columnHeaderVersion.Text = "Version";
            this.columnHeaderVersion.Width = 110;
            // 
            // imageListIconsForListview
            // 
            this.imageListIconsForListview.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIconsForListview.ImageStream")));
            this.imageListIconsForListview.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIconsForListview.Images.SetKeyName(0, "database.png");
            this.imageListIconsForListview.Images.SetKeyName(1, "database_error.png");
            // 
            // labelNHibFolder
            // 
            this.labelNHibFolder.AutoSize = true;
            this.labelNHibFolder.Location = new System.Drawing.Point(11, 46);
            this.labelNHibFolder.Margin = new System.Windows.Forms.Padding(5);
            this.labelNHibFolder.Name = "labelNHibFolder";
            this.labelNHibFolder.Size = new System.Drawing.Size(61, 13);
            this.labelNHibFolder.TabIndex = 29;
            this.labelNHibFolder.Text = "Nhib Folder";
            // 
            // textBoxNHibFolder
            // 
            this.textBoxNHibFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNHibFolder.Location = new System.Drawing.Point(82, 43);
            this.textBoxNHibFolder.Margin = new System.Windows.Forms.Padding(5);
            this.textBoxNHibFolder.Name = "textBoxNHibFolder";
            this.textBoxNHibFolder.Size = new System.Drawing.Size(548, 20);
            this.textBoxNHibFolder.TabIndex = 30;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(638, 40);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(27, 23);
            this.buttonBrowse.TabIndex = 31;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonBack
            // 
            this.buttonBack.Location = new System.Drawing.Point(14, 292);
            this.buttonBack.Margin = new System.Windows.Forms.Padding(5);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(84, 23);
            this.buttonBack.TabIndex = 34;
            this.buttonBack.Text = "< Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.BackColor = System.Drawing.SystemColors.Window;
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdate.Location = new System.Drawing.Point(487, 292);
            this.buttonUpdate.Margin = new System.Windows.Forms.Padding(5);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(84, 23);
            this.buttonUpdate.TabIndex = 35;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = false;
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonRefresh.Location = new System.Drawing.Point(581, 292);
            this.buttonRefresh.Margin = new System.Windows.Forms.Padding(5);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(84, 23);
            this.buttonRefresh.TabIndex = 36;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // folderBrowserDialogNHib
            // 
            this.folderBrowserDialogNHib.ShowNewFolderButton = false;
            // 
            // smoothLabelCurrentVersionIs
            // 
            this.smoothLabelCurrentVersionIs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.smoothLabelCurrentVersionIs.AutoSize = true;
            this.smoothLabelCurrentVersionIs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabelCurrentVersionIs.Location = new System.Drawing.Point(475, 10);
            this.smoothLabelCurrentVersionIs.Name = "smoothLabelCurrentVersionIs";
            this.smoothLabelCurrentVersionIs.Size = new System.Drawing.Size(123, 21);
            this.smoothLabelCurrentVersionIs.TabIndex = 33;
            this.smoothLabelCurrentVersionIs.Text = "Current Version:";
            this.smoothLabelCurrentVersionIs.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.smoothLabelCurrentVersionIs.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // smoothLabelCurrentVersion
            // 
            this.smoothLabelCurrentVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.smoothLabelCurrentVersion.AutoSize = true;
            this.smoothLabelCurrentVersion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabelCurrentVersion.ForeColor = System.Drawing.Color.Blue;
            this.smoothLabelCurrentVersion.Location = new System.Drawing.Point(604, 10);
            this.smoothLabelCurrentVersion.Name = "smoothLabelCurrentVersion";
            this.smoothLabelCurrentVersion.Size = new System.Drawing.Size(61, 21);
            this.smoothLabelCurrentVersion.TabIndex = 32;
            this.smoothLabelCurrentVersion.Text = "7.2.XXX";
            this.smoothLabelCurrentVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.smoothLabelCurrentVersion.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // labelManageDatabaseVersions
            // 
            this.labelManageDatabaseVersions.AutoSize = true;
            this.labelManageDatabaseVersions.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelManageDatabaseVersions.Location = new System.Drawing.Point(10, 7);
            this.labelManageDatabaseVersions.Margin = new System.Windows.Forms.Padding(10);
            this.labelManageDatabaseVersions.Name = "labelManageDatabaseVersions";
            this.labelManageDatabaseVersions.Size = new System.Drawing.Size(198, 21);
            this.labelManageDatabaseVersions.TabIndex = 28;
            this.labelManageDatabaseVersions.Text = "Manage Database Versions";
            this.labelManageDatabaseVersions.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // ManageDatabaseVersions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.smoothLabelCurrentVersionIs);
            this.Controls.Add(this.smoothLabelCurrentVersion);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxNHibFolder);
            this.Controls.Add(this.labelNHibFolder);
            this.Controls.Add(this.labelManageDatabaseVersions);
            this.Controls.Add(this.listViewDatabases);
            this.Name = "ManageDatabaseVersions";
            this.Size = new System.Drawing.Size(670, 320);
            this.Load += new System.EventHandler(this.ManageDatabaseVersions_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewDatabases;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderVersion;
        private General.SmoothLabel labelManageDatabaseVersions;
        private System.Windows.Forms.ImageList imageListIconsForListview;
        private System.Windows.Forms.Label labelNHibFolder;
        private System.Windows.Forms.TextBox textBoxNHibFolder;
        private System.Windows.Forms.Button buttonBrowse;
        private General.SmoothLabel smoothLabelCurrentVersion;
        private General.SmoothLabel smoothLabelCurrentVersionIs;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogNHib;
    }
}
