namespace Teleopti.Support.Tool.Controls.General
{
    partial class SqlAccount
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
            this.CNewSqlUserPwd = new System.Windows.Forms.TextBox();
            this.LTestConnection = new System.Windows.Forms.Label();
            this.BTestConnection = new System.Windows.Forms.Button();
            this.LNewSqlUserPwd = new System.Windows.Forms.Label();
            this.LNewSqlUser = new System.Windows.Forms.Label();
            this.CNewSqlUser = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // CNewSqlUserPwd
            // 
            this.CNewSqlUserPwd.Location = new System.Drawing.Point(20, 69);
            this.CNewSqlUserPwd.Name = "CNewSqlUserPwd";
            this.CNewSqlUserPwd.Size = new System.Drawing.Size(256, 20);
            this.CNewSqlUserPwd.TabIndex = 1;
            // 
            // LTestConnection
            // 
            this.LTestConnection.Location = new System.Drawing.Point(126, 95);
            this.LTestConnection.Name = "LTestConnection";
            this.LTestConnection.Size = new System.Drawing.Size(178, 68);
            this.LTestConnection.TabIndex = 38;
            this.LTestConnection.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // BTestConnection
            // 
            this.BTestConnection.Location = new System.Drawing.Point(21, 95);
            this.BTestConnection.Name = "BTestConnection";
            this.BTestConnection.Size = new System.Drawing.Size(99, 23);
            this.BTestConnection.TabIndex = 2;
            this.BTestConnection.Text = "Test Connection";
            this.BTestConnection.UseVisualStyleBackColor = true;
            // 
            // LNewSqlUserPwd
            // 
            this.LNewSqlUserPwd.AutoSize = true;
            this.LNewSqlUserPwd.Location = new System.Drawing.Point(18, 51);
            this.LNewSqlUserPwd.Name = "LNewSqlUserPwd";
            this.LNewSqlUserPwd.Size = new System.Drawing.Size(102, 13);
            this.LNewSqlUserPwd.TabIndex = 36;
            this.LNewSqlUserPwd.Text = "SQL User Password";
            // 
            // LNewSqlUser
            // 
            this.LNewSqlUser.AutoSize = true;
            this.LNewSqlUser.Location = new System.Drawing.Point(18, 5);
            this.LNewSqlUser.Name = "LNewSqlUser";
            this.LNewSqlUser.Size = new System.Drawing.Size(78, 13);
            this.LNewSqlUser.TabIndex = 35;
            this.LNewSqlUser.Text = "New SQL User";
            // 
            // CNewSqlUser
            // 
            this.CNewSqlUser.FormattingEnabled = true;
            this.CNewSqlUser.Location = new System.Drawing.Point(20, 24);
            this.CNewSqlUser.Name = "CNewSqlUser";
            this.CNewSqlUser.Size = new System.Drawing.Size(256, 21);
            this.CNewSqlUser.TabIndex = 0;
            // 
            // SqlAccount
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.CNewSqlUserPwd);
            this.Controls.Add(this.LTestConnection);
            this.Controls.Add(this.BTestConnection);
            this.Controls.Add(this.LNewSqlUserPwd);
            this.Controls.Add(this.LNewSqlUser);
            this.Controls.Add(this.CNewSqlUser);
            this.Name = "SqlAccount";
            this.Size = new System.Drawing.Size(299, 126);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox CNewSqlUserPwd;
        private System.Windows.Forms.Label LTestConnection;
        private System.Windows.Forms.Button BTestConnection;
        private System.Windows.Forms.Label LNewSqlUserPwd;
        private System.Windows.Forms.Label LNewSqlUser;
        private System.Windows.Forms.ComboBox CNewSqlUser;
    }
}
