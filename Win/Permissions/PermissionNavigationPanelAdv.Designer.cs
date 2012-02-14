namespace Teleopti.Ccc.Win.Permissions
{
    partial class PermissionNavigationPanelAdv
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
            Teleopti.Ccc.Domain.Security.AuthorizationEntities.ApplicationFunction applicationFunction1 = new Teleopti.Ccc.Domain.Security.AuthorizationEntities.ApplicationFunction();
            Teleopti.Ccc.Domain.Common.UserTextTranslator userTextTranslator1 = new Teleopti.Ccc.Domain.Common.UserTextTranslator();
            this.businessUnitHierarchyPanel1 = new Teleopti.Ccc.Win.Common.Controls.BusinessUnitHierarchyPanelAdv();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // businessUnitHierarchyPanel1
            // 
            applicationFunction1.ForeignId = "0001-1000";
            applicationFunction1.ForeignSource = "Raptor";
            applicationFunction1.FunctionCode = "Raptor";
            applicationFunction1.FunctionDescription = "xxOpenRaptorApplication";
            applicationFunction1.IsPermitted = null;
            applicationFunction1.IsPreliminary = false;
            applicationFunction1.Parent = null;
            applicationFunction1.SortOrder = 10;
            applicationFunction1.UserTextTranslator = userTextTranslator1;
            this.businessUnitHierarchyPanel1.ApplicationFunction = applicationFunction1;
            this.businessUnitHierarchyPanel1.BackColor = System.Drawing.Color.Transparent;
            this.businessUnitHierarchyPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.businessUnitHierarchyPanel1.Location = new System.Drawing.Point(0, 0);
            this.businessUnitHierarchyPanel1.Name = "businessUnitHierarchyPanel1";
            this.businessUnitHierarchyPanel1.Size = new System.Drawing.Size(195, 363);
            this.businessUnitHierarchyPanel1.TabIndex = 1;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.showToolStripMenuItem.Text = "Show rights";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);
            // 
            // PermissionNavigationPanelAdv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.businessUnitHierarchyPanel1);
            this.Name = "PermissionNavigationPanelAdv";
            this.Size = new System.Drawing.Size(195, 363);
            this.Load += new System.EventHandler(this.UserControl_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Common.Controls.BusinessUnitHierarchyPanelAdv businessUnitHierarchyPanel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
    }
}
