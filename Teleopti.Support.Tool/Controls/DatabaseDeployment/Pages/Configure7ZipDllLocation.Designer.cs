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
            this.SuspendLayout();
            // 
            // smoothLabel1
            // 
            this.smoothLabel1.AutoSize = true;
            this.smoothLabel1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.smoothLabel1.Location = new System.Drawing.Point(-4, 10);
            this.smoothLabel1.Name = "smoothLabel1";
            this.smoothLabel1.Size = new System.Drawing.Size(215, 21);
            this.smoothLabel1.TabIndex = 0;
            this.smoothLabel1.Text = "Selecting location of 7z.dll file";
            this.smoothLabel1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            // 
            // textBoxSevenZipDllUrn
            // 
            this.textBoxSevenZipDllUrn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSevenZipDllUrn.Location = new System.Drawing.Point(0, 34);
            this.textBoxSevenZipDllUrn.Name = "textBoxSevenZipDllUrn";
            this.textBoxSevenZipDllUrn.Size = new System.Drawing.Size(626, 20);
            this.textBoxSevenZipDllUrn.TabIndex = 1;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(632, 34);
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
            // Configure7ZipDllLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
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
    }
}
