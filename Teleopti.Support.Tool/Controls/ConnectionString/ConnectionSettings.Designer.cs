using Teleopti.Support.Tool.Controls.General;
namespace Teleopti.Support.Tool.Controls.ConnectionString
{
    partial class ConnectionSettings
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
            this.labelConnectionStringSettings = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.sqlAccount = new Teleopti.Support.Tool.Controls.General.SqlAccount();
            this.dbSelect = new Teleopti.Support.Tool.Controls.General.DBSelect();
            this.SuspendLayout();
            // 
            // labelConnectionStringSettings
            // 
            this.labelConnectionStringSettings.AutoSize = true;
            this.labelConnectionStringSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConnectionStringSettings.Location = new System.Drawing.Point(10, 10);
            this.labelConnectionStringSettings.Margin = new System.Windows.Forms.Padding(10);
            this.labelConnectionStringSettings.Name = "labelConnectionStringSettings";
            this.labelConnectionStringSettings.Size = new System.Drawing.Size(195, 21);
            this.labelConnectionStringSettings.TabIndex = 27;
            this.labelConnectionStringSettings.Text = "Connection String Settings";
            this.labelConnectionStringSettings.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // sqlAccount
            // 
            this.sqlAccount.BackColor = System.Drawing.SystemColors.Window;
            this.sqlAccount.ConnectedColor = System.Drawing.SystemColors.ControlText;
            this.sqlAccount.Location = new System.Drawing.Point(320, 44);
            this.sqlAccount.Name = "sqlAccount";
            this.sqlAccount.Size = new System.Drawing.Size(300, 160);
            this.sqlAccount.SqlUserPassword = "";
            this.sqlAccount.TabIndex = 1;
            this.sqlAccount.TestConnection = "";
            // 
            // dbSelect
            // 
            this.dbSelect.BackColor = System.Drawing.SystemColors.Window;
            this.dbSelect.Location = new System.Drawing.Point(14, 44);
            this.dbSelect.Name = "dbSelect";
            this.dbSelect.Size = new System.Drawing.Size(300, 160);
            this.dbSelect.TabIndex = 0;
            // 
            // ConnectionSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.labelConnectionStringSettings);
            this.Controls.Add(this.sqlAccount);
            this.Controls.Add(this.dbSelect);
            this.Name = "ConnectionSettings";
            this.Size = new System.Drawing.Size(634, 246);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SqlAccount sqlAccount;
        private DBSelect dbSelect;
        private SmoothLabel labelConnectionStringSettings;

    }
}
