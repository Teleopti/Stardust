namespace Teleopti.Ccc.Win.Permissions
{
    partial class AuthorizationSettingsPanel
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
            this.containerPanel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.xchkActiveDirectoryDefinedRoles = new System.Windows.Forms.CheckBox();
            this.xchkDatabaseDefinedRoles = new System.Windows.Forms.CheckBox();
            this.containerPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerPanel
            // 
            this.containerPanel.Controls.Add(this.groupBox1);
            this.containerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerPanel.Location = new System.Drawing.Point(0, 0);
            this.containerPanel.Name = "containerPanel";
            this.containerPanel.Size = new System.Drawing.Size(421, 219);
            this.containerPanel.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.xchkActiveDirectoryDefinedRoles);
            this.groupBox1.Controls.Add(this.xchkDatabaseDefinedRoles);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(388, 87);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "xx Role sources";
            // 
            // xchkActiveDirectoryDefinedRoles
            // 
            this.xchkActiveDirectoryDefinedRoles.AutoSize = true;
            this.xchkActiveDirectoryDefinedRoles.Location = new System.Drawing.Point(20, 52);
            this.xchkActiveDirectoryDefinedRoles.Name = "xchkActiveDirectoryDefinedRoles";
            this.xchkActiveDirectoryDefinedRoles.Size = new System.Drawing.Size(196, 17);
            this.xchkActiveDirectoryDefinedRoles.TabIndex = 5;
            this.xchkActiveDirectoryDefinedRoles.Text = "xx Use active directory defined roles";
            this.xchkActiveDirectoryDefinedRoles.UseVisualStyleBackColor = true;
            // 
            // xchkDatabaseDefinedRoles
            // 
            this.xchkDatabaseDefinedRoles.AutoSize = true;
            this.xchkDatabaseDefinedRoles.Location = new System.Drawing.Point(20, 28);
            this.xchkDatabaseDefinedRoles.Name = "xchkDatabaseDefinedRoles";
            this.xchkDatabaseDefinedRoles.Size = new System.Drawing.Size(168, 17);
            this.xchkDatabaseDefinedRoles.TabIndex = 3;
            this.xchkDatabaseDefinedRoles.Text = "xx Use database defined roles";
            this.xchkDatabaseDefinedRoles.UseVisualStyleBackColor = true;
            // 
            // AuthorizationSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.containerPanel);
            this.Name = "AuthorizationSettingsPanel";
            this.Size = new System.Drawing.Size(421, 219);
            this.containerPanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel containerPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox xchkActiveDirectoryDefinedRoles;
        private System.Windows.Forms.CheckBox xchkDatabaseDefinedRoles;
    }
}
