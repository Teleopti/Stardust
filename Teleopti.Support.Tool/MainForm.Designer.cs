namespace Teleopti.Support.Tool
{
    partial class MainForm
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
            this.PTracks = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.LLChangeDBConn = new System.Windows.Forms.LinkLabel();
            this.BClose = new System.Windows.Forms.Button();
            this.PTracks.SuspendLayout();
            this.SuspendLayout();
            // 
            // PTracks
            // 
            this.PTracks.Controls.Add(this.label2);
            this.PTracks.Controls.Add(this.label1);
            this.PTracks.Controls.Add(this.linkLabel2);
            this.PTracks.Controls.Add(this.LLChangeDBConn);
            this.PTracks.Location = new System.Drawing.Point(12, 52);
            this.PTracks.Name = "PTracks";
            this.PTracks.Size = new System.Drawing.Size(652, 278);
            this.PTracks.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Patch databases";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(482, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Update database names and users in all connections and updates database reference" +
    "s in databases";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(26, 43);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(215, 13);
            this.linkLabel2.TabIndex = 1;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "2. Install Database Patch (Not Implemented)";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // LLChangeDBConn
            // 
            this.LLChangeDBConn.AutoSize = true;
            this.LLChangeDBConn.Location = new System.Drawing.Point(26, 8);
            this.LLChangeDBConn.Name = "LLChangeDBConn";
            this.LLChangeDBConn.Size = new System.Drawing.Size(167, 13);
            this.LLChangeDBConn.TabIndex = 0;
            this.LLChangeDBConn.TabStop = true;
            this.LLChangeDBConn.Text = "1. Change Database Connections";
            this.LLChangeDBConn.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LLChangeDBConn_LinkClicked);
            // 
            // BClose
            // 
            this.BClose.Location = new System.Drawing.Point(25, 385);
            this.BClose.Name = "BClose";
            this.BClose.Size = new System.Drawing.Size(75, 23);
            this.BClose.TabIndex = 0;
            this.BClose.Text = "Exit";
            this.BClose.UseVisualStyleBackColor = true;
            this.BClose.Click += new System.EventHandler(this.BClose_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(692, 416);
            this.Controls.Add(this.BClose);
            this.Controls.Add(this.PTracks);
            this.Name = "MainForm";
            this.Text = "Teleopti Support Tool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.PTracks.ResumeLayout(false);
            this.PTracks.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PTracks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.LinkLabel LLChangeDBConn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BClose;


    }
}