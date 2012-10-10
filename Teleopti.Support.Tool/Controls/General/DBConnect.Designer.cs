namespace Teleopti.Support.Tool.Controls.General
{
    partial class DBConnect
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
            this.LConnected = new System.Windows.Forms.Label();
            this.TBServer = new System.Windows.Forms.TextBox();
            this.CBWindowsAuth = new System.Windows.Forms.CheckBox();
            this.TBPassword = new System.Windows.Forms.TextBox();
            this.TBSQLUser = new System.Windows.Forms.TextBox();
            this.LPassword = new System.Windows.Forms.Label();
            this.LSQLUser = new System.Windows.Forms.Label();
            this.LServer = new System.Windows.Forms.Label();
            this.BConnect = new System.Windows.Forms.Button();
            this.labelSQLServerConnection = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.SuspendLayout();
            // 
            // LConnected
            // 
            this.LConnected.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.LConnected.BackColor = System.Drawing.SystemColors.Window;
            this.LConnected.Location = new System.Drawing.Point(167, 182);
            this.LConnected.Name = "LConnected";
            this.LConnected.Size = new System.Drawing.Size(269, 62);
            this.LConnected.TabIndex = 27;
            this.LConnected.Text = "Not Connected";
            this.LConnected.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TBServer
            // 
            this.TBServer.Location = new System.Drawing.Point(226, 57);
            this.TBServer.Name = "TBServer";
            this.TBServer.Size = new System.Drawing.Size(170, 20);
            this.TBServer.TabIndex = 0;
            // 
            // CBWindowsAuth
            // 
            this.CBWindowsAuth.AutoSize = true;
            this.CBWindowsAuth.Location = new System.Drawing.Point(226, 135);
            this.CBWindowsAuth.Name = "CBWindowsAuth";
            this.CBWindowsAuth.Size = new System.Drawing.Size(141, 17);
            this.CBWindowsAuth.TabIndex = 3;
            this.CBWindowsAuth.Text = "Windows Authentication";
            this.CBWindowsAuth.UseVisualStyleBackColor = true;
            this.CBWindowsAuth.CheckedChanged += new System.EventHandler(this.CBWindowsAuth_CheckedChanged);
            // 
            // TBPassword
            // 
            this.TBPassword.Location = new System.Drawing.Point(226, 109);
            this.TBPassword.Name = "TBPassword";
            this.TBPassword.Size = new System.Drawing.Size(170, 20);
            this.TBPassword.TabIndex = 2;
            // 
            // TBSQLUser
            // 
            this.TBSQLUser.Location = new System.Drawing.Point(226, 83);
            this.TBSQLUser.Name = "TBSQLUser";
            this.TBSQLUser.Size = new System.Drawing.Size(170, 20);
            this.TBSQLUser.TabIndex = 1;
            // 
            // LPassword
            // 
            this.LPassword.AutoSize = true;
            this.LPassword.Location = new System.Drawing.Point(167, 109);
            this.LPassword.Name = "LPassword";
            this.LPassword.Size = new System.Drawing.Size(53, 13);
            this.LPassword.TabIndex = 21;
            this.LPassword.Text = "Password";
            // 
            // LSQLUser
            // 
            this.LSQLUser.AutoSize = true;
            this.LSQLUser.Location = new System.Drawing.Point(167, 86);
            this.LSQLUser.Name = "LSQLUser";
            this.LSQLUser.Size = new System.Drawing.Size(53, 13);
            this.LSQLUser.TabIndex = 20;
            this.LSQLUser.Text = "SQL User";
            // 
            // LServer
            // 
            this.LServer.AutoSize = true;
            this.LServer.Location = new System.Drawing.Point(182, 60);
            this.LServer.Name = "LServer";
            this.LServer.Size = new System.Drawing.Size(38, 13);
            this.LServer.TabIndex = 19;
            this.LServer.Text = "Server";
            // 
            // BConnect
            // 
            this.BConnect.Location = new System.Drawing.Point(256, 156);
            this.BConnect.Name = "BConnect";
            this.BConnect.Size = new System.Drawing.Size(77, 23);
            this.BConnect.TabIndex = 4;
            this.BConnect.Text = "Connect";
            this.BConnect.UseVisualStyleBackColor = true;
            this.BConnect.Click += new System.EventHandler(this.BConnect_Click);
            // 
            // labelSQLServerConnection
            // 
            this.labelSQLServerConnection.AutoSize = true;
            this.labelSQLServerConnection.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSQLServerConnection.Location = new System.Drawing.Point(10, 10);
            this.labelSQLServerConnection.Margin = new System.Windows.Forms.Padding(10);
            this.labelSQLServerConnection.Name = "labelSQLServerConnection";
            this.labelSQLServerConnection.Size = new System.Drawing.Size(171, 21);
            this.labelSQLServerConnection.TabIndex = 26;
            this.labelSQLServerConnection.Text = "SQL Server Connection";
            this.labelSQLServerConnection.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // DBConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.LConnected);
            this.Controls.Add(this.labelSQLServerConnection);
            this.Controls.Add(this.TBServer);
            this.Controls.Add(this.CBWindowsAuth);
            this.Controls.Add(this.TBPassword);
            this.Controls.Add(this.TBSQLUser);
            this.Controls.Add(this.LPassword);
            this.Controls.Add(this.LSQLUser);
            this.Controls.Add(this.LServer);
            this.Controls.Add(this.BConnect);
            this.Name = "DBConnect";
            this.Size = new System.Drawing.Size(670, 320);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LConnected;
        private SmoothLabel labelSQLServerConnection;
        private System.Windows.Forms.TextBox TBServer;
        private System.Windows.Forms.CheckBox CBWindowsAuth;
        private System.Windows.Forms.TextBox TBPassword;
        private System.Windows.Forms.TextBox TBSQLUser;
        private System.Windows.Forms.Label LPassword;
        private System.Windows.Forms.Label LSQLUser;
        private System.Windows.Forms.Label LServer;
        private System.Windows.Forms.Button BConnect;
    }
}
