using Teleopti.Ccc.Win.Common.Controls;

namespace Teleopti.Ccc.Win.Permissions
{
    partial class PermissionNavigationScreenAdv
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.permissionNavigationPanel1 = new Teleopti.Ccc.Win.Permissions.PermissionNavigationPanelAdv();
            this.SuspendLayout();
            // 
            // permissionNavigationPanel1
            // 
            this.permissionNavigationPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.permissionNavigationPanel1.Location = new System.Drawing.Point(0, 0);
            this.permissionNavigationPanel1.Name = "permissionNavigationPanel1";
            this.permissionNavigationPanel1.Size = new System.Drawing.Size(283, 464);
            this.permissionNavigationPanel1.TabIndex = 0;
            // 
            // PermissionNavigationScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(283, 464);
            this.Controls.Add(this.permissionNavigationPanel1);
            this.Name = "PermissionNavigationScreen";
            this.Text = "xxPermissionNavigationScreen";
            this.ResumeLayout(false);

        }

        #endregion

        private PermissionNavigationPanelAdv permissionNavigationPanel1;


    }
}