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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dbSelect = new Teleopti.Support.Tool.Controls.General.DBSelect();
            this.sqlAccount = new Teleopti.Support.Tool.Controls.General.SqlAccount();
            this.CBRunInDevEnv = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.BackColor = System.Drawing.SystemColors.Window;
            this.groupBox1.Controls.Add(this.dbSelect);
            this.groupBox1.Controls.Add(this.sqlAccount);
            this.groupBox1.Controls.Add(this.CBRunInDevEnv);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(627, 237);
            this.groupBox1.TabIndex = 23;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection String Settings";
            // 
            // dbSelect
            // 
            this.dbSelect.BackColor = System.Drawing.SystemColors.Window;
            this.dbSelect.Location = new System.Drawing.Point(13, 43);
            this.dbSelect.Name = "dbSelect";
            this.dbSelect.Size = new System.Drawing.Size(285, 158);
            this.dbSelect.TabIndex = 0;
            // 
            // sqlAccount
            // 
            this.sqlAccount.BackColor = System.Drawing.SystemColors.Window;
            this.sqlAccount.ConnectedColor = System.Drawing.SystemColors.ControlText;
            this.sqlAccount.Location = new System.Drawing.Point(305, 40);
            this.sqlAccount.Name = "sqlAccount";
            this.sqlAccount.Size = new System.Drawing.Size(304, 143);
            this.sqlAccount.SqlUserPassword = "";
            this.sqlAccount.TabIndex = 1;
            this.sqlAccount.TestConnection = "";
            // 
            // CBRunInDevEnv
            // 
            this.CBRunInDevEnv.AutoSize = true;
            this.CBRunInDevEnv.Location = new System.Drawing.Point(70, -20);
            this.CBRunInDevEnv.Name = "CBRunInDevEnv";
            this.CBRunInDevEnv.Size = new System.Drawing.Size(142, 17);
            this.CBRunInDevEnv.TabIndex = 34;
            this.CBRunInDevEnv.Text = "Run in Dev Environment";
            this.CBRunInDevEnv.UseVisualStyleBackColor = true;
            this.CBRunInDevEnv.Visible = false;
            // 
            // ConnectionSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.groupBox1);
            this.Name = "ConnectionSettings";
            this.Size = new System.Drawing.Size(634, 246);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox CBRunInDevEnv;
        private SqlAccount sqlAccount;
       
        private DBSelect dbSelect;
    }
}
