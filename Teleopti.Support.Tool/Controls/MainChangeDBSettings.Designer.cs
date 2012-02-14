using Teleopti.Support.Tool.Controls.ConnectionString;
using Teleopti.Support.Tool.Controls.General;

namespace Teleopti.Support.Tool.Controls
{
    partial class MainChangeDBSettings
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
            this.BNext = new System.Windows.Forms.Button();
            this.BBack = new System.Windows.Forms.Button();
            this.BInstall = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BNext
            // 
            this.BNext.BackColor = System.Drawing.SystemColors.Window;
            this.BNext.Location = new System.Drawing.Point(515, 386);
            this.BNext.Name = "BNext";
            this.BNext.Size = new System.Drawing.Size(75, 23);
            this.BNext.TabIndex = 0;
            this.BNext.Text = "Next";
            this.BNext.UseVisualStyleBackColor = false;
            this.BNext.Click += new System.EventHandler(this.BNext_Click);
            // 
            // BBack
            // 
            this.BBack.Location = new System.Drawing.Point(418, 385);
            this.BBack.Name = "BBack";
            this.BBack.Size = new System.Drawing.Size(75, 23);
            this.BBack.TabIndex = 1;
            this.BBack.Text = "Back";
            this.BBack.UseVisualStyleBackColor = true;
            this.BBack.Click += new System.EventHandler(this.BBack_Click);
            // 
            // BInstall
            // 
            this.BInstall.Location = new System.Drawing.Point(515, 386);
            this.BInstall.Name = "BInstall";
            this.BInstall.Size = new System.Drawing.Size(75, 23);
            this.BInstall.TabIndex = 2;
            this.BInstall.Text = "Execute";
            this.BInstall.UseVisualStyleBackColor = true;
            this.BInstall.Visible = false;
            this.BInstall.Click += new System.EventHandler(this.BInstall_Click);
            // 
            // MainChangeDBConn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BInstall);
            this.Controls.Add(this.BBack);
            this.Controls.Add(this.BNext);
            this.Name = "MainChangeDBConn";
            this.Size = new System.Drawing.Size(700, 450);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BNext;
        private System.Windows.Forms.Button BBack;
        private System.Windows.Forms.Button BInstall;
        private readonly DBConnect dbConnect;
        private readonly Summary connSummary;
        private readonly string configFile;
    }
}
