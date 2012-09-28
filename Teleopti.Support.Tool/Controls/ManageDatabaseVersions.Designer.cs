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
            this.labelManageDatabaseVersions = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageListIconsForListview = new System.Windows.Forms.ImageList(this.components);
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
            this.listViewDatabases.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.listViewDatabases.Location = new System.Drawing.Point(5, 83);
            this.listViewDatabases.Margin = new System.Windows.Forms.Padding(5);
            this.listViewDatabases.Name = "listViewDatabases";
            this.listViewDatabases.Size = new System.Drawing.Size(660, 131);
            this.listViewDatabases.SmallImageList = this.imageListIconsForListview;
            this.listViewDatabases.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewDatabases.TabIndex = 0;
            this.listViewDatabases.UseCompatibleStateImageBehavior = false;
            this.listViewDatabases.View = System.Windows.Forms.View.Details;
            // 
            // labelManageDatabaseVersions
            // 
            this.labelManageDatabaseVersions.AutoSize = true;
            this.labelManageDatabaseVersions.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelManageDatabaseVersions.Location = new System.Drawing.Point(10, 10);
            this.labelManageDatabaseVersions.Margin = new System.Windows.Forms.Padding(10);
            this.labelManageDatabaseVersions.Name = "labelManageDatabaseVersions";
            this.labelManageDatabaseVersions.Size = new System.Drawing.Size(198, 21);
            this.labelManageDatabaseVersions.TabIndex = 28;
            this.labelManageDatabaseVersions.Text = "Manage Database Versions";
            this.labelManageDatabaseVersions.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 300;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            // 
            // columnHeaderVersion
            // 
            this.columnHeaderVersion.Text = "Version";
            // 
            // imageListIconsForListview
            // 
            this.imageListIconsForListview.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListIconsForListview.ImageStream")));
            this.imageListIconsForListview.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListIconsForListview.Images.SetKeyName(0, "database.png");
            this.imageListIconsForListview.Images.SetKeyName(1, "database_error.png");
            // 
            // ManageDatabaseVersions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelManageDatabaseVersions);
            this.Controls.Add(this.listViewDatabases);
            this.Name = "ManageDatabaseVersions";
            this.Size = new System.Drawing.Size(670, 320);
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
    }
}
