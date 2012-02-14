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
            this.label3 = new System.Windows.Forms.Label();
            this.TBServer = new System.Windows.Forms.TextBox();
            this.CBWindowsAuth = new System.Windows.Forms.CheckBox();
            this.TBPassword = new System.Windows.Forms.TextBox();
            this.TBSQLUser = new System.Windows.Forms.TextBox();
            this.LPassword = new System.Windows.Forms.Label();
            this.LSQLUser = new System.Windows.Forms.Label();
            this.LServer = new System.Windows.Forms.Label();
            this.BConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LConnected
            // 
            this.LConnected.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.LConnected.BackColor = System.Drawing.SystemColors.Window;
            this.LConnected.Location = new System.Drawing.Point(8, 150);
            this.LConnected.Name = "LConnected";
            this.LConnected.Size = new System.Drawing.Size(269, 138);
            this.LConnected.TabIndex = 27;
            this.LConnected.Text = "Not Connected";
            this.LConnected.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(87, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Connect To SQL Server";
            // 
            // TBServer
            // 
            this.TBServer.Location = new System.Drawing.Point(67, 25);
            this.TBServer.Name = "TBServer";
            this.TBServer.Size = new System.Drawing.Size(170, 20);
            this.TBServer.TabIndex = 0;
            // 
            // CBWindowsAuth
            // 
            this.CBWindowsAuth.AutoSize = true;
            this.CBWindowsAuth.Location = new System.Drawing.Point(67, 103);
            this.CBWindowsAuth.Name = "CBWindowsAuth";
            this.CBWindowsAuth.Size = new System.Drawing.Size(141, 17);
            this.CBWindowsAuth.TabIndex = 3;
            this.CBWindowsAuth.Text = "Windows Authentication";
            this.CBWindowsAuth.UseVisualStyleBackColor = true;
            this.CBWindowsAuth.CheckedChanged += new System.EventHandler(this.CBWindowsAuth_CheckedChanged);
            // 
            // TBPassword
            // 
            this.TBPassword.Location = new System.Drawing.Point(67, 77);
            this.TBPassword.Name = "TBPassword";
            this.TBPassword.Size = new System.Drawing.Size(170, 20);
            this.TBPassword.TabIndex = 2;
            // 
            // TBSQLUser
            // 
            this.TBSQLUser.Location = new System.Drawing.Point(67, 51);
            this.TBSQLUser.Name = "TBSQLUser";
            this.TBSQLUser.Size = new System.Drawing.Size(170, 20);
            this.TBSQLUser.TabIndex = 1;
            // 
            // LPassword
            // 
            this.LPassword.AutoSize = true;
            this.LPassword.Location = new System.Drawing.Point(8, 77);
            this.LPassword.Name = "LPassword";
            this.LPassword.Size = new System.Drawing.Size(53, 13);
            this.LPassword.TabIndex = 21;
            this.LPassword.Text = "Password";
            // 
            // LSQLUser
            // 
            this.LSQLUser.AutoSize = true;
            this.LSQLUser.Location = new System.Drawing.Point(8, 54);
            this.LSQLUser.Name = "LSQLUser";
            this.LSQLUser.Size = new System.Drawing.Size(53, 13);
            this.LSQLUser.TabIndex = 20;
            this.LSQLUser.Text = "SQL User";
            // 
            // LServer
            // 
            this.LServer.AutoSize = true;
            this.LServer.Location = new System.Drawing.Point(23, 28);
            this.LServer.Name = "LServer";
            this.LServer.Size = new System.Drawing.Size(38, 13);
            this.LServer.TabIndex = 19;
            this.LServer.Text = "Server";
            // 
            // BConnect
            // 
            this.BConnect.Location = new System.Drawing.Point(97, 124);
            this.BConnect.Name = "BConnect";
            this.BConnect.Size = new System.Drawing.Size(77, 23);
            this.BConnect.TabIndex = 4;
            this.BConnect.Text = "Connect";
            this.BConnect.UseVisualStyleBackColor = true;
            this.BConnect.Click += new System.EventHandler(this.BConnect_Click);
            // 
            // DBConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.LConnected);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TBServer);
            this.Controls.Add(this.CBWindowsAuth);
            this.Controls.Add(this.TBPassword);
            this.Controls.Add(this.TBSQLUser);
            this.Controls.Add(this.LPassword);
            this.Controls.Add(this.LSQLUser);
            this.Controls.Add(this.LServer);
            this.Controls.Add(this.BConnect);
            this.Name = "DBConnect";
            this.Size = new System.Drawing.Size(285, 297);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label LConnected;
        private System.Windows.Forms.Label label3;
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
