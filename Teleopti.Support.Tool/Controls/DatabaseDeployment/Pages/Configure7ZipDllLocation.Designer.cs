namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    partial class Configure7ZipDllLocation
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
            this.smoothLabel1 = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.textBoxSevenZipDllUrn = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.smoothLabel2 = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.smoothLink1 = new Teleopti.Support.Tool.Controls.General.SmoothLink();
            this.SuspendLayout();
            // 
            // smoothLabel1
            // 
            this.smoothLabel1.AutoSize = true;
            this.smoothLabel1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabel1.Location = new System.Drawing.Point(-1, 100);
            this.smoothLabel1.Name = "smoothLabel1";
            this.smoothLabel1.Size = new System.Drawing.Size(152, 21);
            this.smoothLabel1.TabIndex = 0;
            this.smoothLabel1.Text = "Location of 7z.dll file";
            this.smoothLabel1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // 
            // textBoxSevenZipDllUrn
            // 
            this.textBoxSevenZipDllUrn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSevenZipDllUrn.Location = new System.Drawing.Point(3, 124);
            this.textBoxSevenZipDllUrn.Name = "textBoxSevenZipDllUrn";
            this.textBoxSevenZipDllUrn.Size = new System.Drawing.Size(626, 20);
            this.textBoxSevenZipDllUrn.TabIndex = 1;
            this.textBoxSevenZipDllUrn.TextChanged += new System.EventHandler(this.textBoxSevenZipDllUrn_TextChanged);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(635, 124);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(25, 20);
            this.buttonBrowse.TabIndex = 3;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "7z.dll";
            this.openFileDialog1.Title = "Find 7z.dll";
            // 
            // smoothLabel2
            // 
            this.smoothLabel2.AutoSize = true;
            this.smoothLabel2.Location = new System.Drawing.Point(0, 12);
            this.smoothLabel2.Name = "smoothLabel2";
            this.smoothLabel2.Size = new System.Drawing.Size(330, 13);
            this.smoothLabel2.TabIndex = 4;
            this.smoothLabel2.Text = "7z.dll from 7-Zip file archiver is required to run Database Deployment.";
            this.smoothLabel2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // 
            // smoothLink1
            // 
            this.smoothLink1.AutoSize = true;
            this.smoothLink1.Location = new System.Drawing.Point(-3, 45);
            this.smoothLink1.Name = "smoothLink1";
            this.smoothLink1.Size = new System.Drawing.Size(137, 13);
            this.smoothLink1.TabIndex = 5;
            this.smoothLink1.TabStop = true;
            this.smoothLink1.Text = "7-Zip file archiver download";
            this.smoothLink1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
            this.smoothLink1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.smoothLink1_LinkClicked);
            // 
            // Configure7ZipDllLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.smoothLink1);
            this.Controls.Add(this.smoothLabel2);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxSevenZipDllUrn);
            this.Controls.Add(this.smoothLabel1);
            this.Name = "Configure7ZipDllLocation";
            this.Size = new System.Drawing.Size(660, 240);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private General.SmoothLabel smoothLabel1;
        private System.Windows.Forms.TextBox textBoxSevenZipDllUrn;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private General.SmoothLabel smoothLabel2;
        private General.SmoothLink smoothLink1;
    }
}
