namespace Teleopti.Ccc.Win.Permissions
{
    partial class LicenseSchemaStructurePanel
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LicenseSchemaStructurePanel));
            this.treeViewLicenseSchema = new System.Windows.Forms.TreeView();
            this.imageListLicenseSchema = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // treeViewLicenseSchema
            // 
            this.treeViewLicenseSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLicenseSchema.ImageIndex = 0;
            this.treeViewLicenseSchema.ImageList = this.imageListLicenseSchema;
            this.treeViewLicenseSchema.Location = new System.Drawing.Point(0, 0);
            this.treeViewLicenseSchema.Name = "treeViewLicenseSchema";
            this.treeViewLicenseSchema.SelectedImageIndex = 0;
            this.treeViewLicenseSchema.ShowNodeToolTips = true;
            this.treeViewLicenseSchema.Size = new System.Drawing.Size(205, 149);
            this.treeViewLicenseSchema.TabIndex = 1;
            // 
            // imageListLicenseSchema
            // 
            this.imageListLicenseSchema.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListLicenseSchema.ImageStream")));
            this.imageListLicenseSchema.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListLicenseSchema.Images.SetKeyName(0, "Open1.ico");
            this.imageListLicenseSchema.Images.SetKeyName(1, "Grid.ico");
            this.imageListLicenseSchema.Images.SetKeyName(2, "Columns.ico");
            this.imageListLicenseSchema.Images.SetKeyName(3, "functionSmall.ico");
            this.imageListLicenseSchema.Images.SetKeyName(4, "personSmall.ico");
            this.imageListLicenseSchema.Images.SetKeyName(5, "onePersonSmall.ico");
            this.imageListLicenseSchema.Images.SetKeyName(6, "mark_updated.ico");
            // 
            // LicenseSchemaStructurePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.treeViewLicenseSchema);
            this.Name = "LicenseSchemaStructurePanel";
            this.Size = new System.Drawing.Size(205, 149);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView treeViewLicenseSchema;
        private System.Windows.Forms.ImageList imageListLicenseSchema;

    }
}
