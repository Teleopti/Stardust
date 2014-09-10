namespace Teleopti.Support.Tool.Controls.DatabaseDeployment
{
    partial class DatabaseDeployMain
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
            this.labelDeployDatabase = new Teleopti.Support.Tool.Controls.General.SmoothLabel();
            this.buttonBack = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.panelPageContainer = new System.Windows.Forms.Panel();
            this.linkLabelGoHome = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // labelDeployDatabase
            // 
            this.labelDeployDatabase.AutoSize = true;
            this.labelDeployDatabase.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDeployDatabase.Location = new System.Drawing.Point(-4, 10);
            this.labelDeployDatabase.Margin = new System.Windows.Forms.Padding(10);
            this.labelDeployDatabase.Name = "labelDeployDatabase";
            this.labelDeployDatabase.Size = new System.Drawing.Size(220, 21);
            this.labelDeployDatabase.TabIndex = 29;
            this.labelDeployDatabase.Text = "Deploy Teleopti CCC Database";
            this.labelDeployDatabase.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            // 
            // buttonBack
            // 
            this.buttonBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBack.Location = new System.Drawing.Point(510, 292);
            this.buttonBack.Margin = new System.Windows.Forms.Padding(5);
            this.buttonBack.Name = "buttonBack";
            this.buttonBack.Size = new System.Drawing.Size(75, 23);
            this.buttonBack.TabIndex = 35;
            this.buttonBack.Text = "< &Back";
            this.buttonBack.UseVisualStyleBackColor = true;
            this.buttonBack.Click += new System.EventHandler(this.buttonBack_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNext.Enabled = false;
            this.buttonNext.Location = new System.Drawing.Point(595, 292);
            this.buttonNext.Margin = new System.Windows.Forms.Padding(5);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 36;
            this.buttonNext.Text = "&Next >";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // panelPageContainer
            // 
            this.panelPageContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPageContainer.Location = new System.Drawing.Point(0, 44);
            this.panelPageContainer.Margin = new System.Windows.Forms.Padding(0);
            this.panelPageContainer.Name = "panelPageContainer";
            this.panelPageContainer.Size = new System.Drawing.Size(670, 240);
            this.panelPageContainer.TabIndex = 37;
            // 
            // linkLabelGoHome
            // 
            this.linkLabelGoHome.AutoSize = true;
            this.linkLabelGoHome.Location = new System.Drawing.Point(-3, 297);
            this.linkLabelGoHome.Name = "linkLabelGoHome";
            this.linkLabelGoHome.Size = new System.Drawing.Size(50, 13);
            this.linkLabelGoHome.TabIndex = 38;
            this.linkLabelGoHome.TabStop = true;
            this.linkLabelGoHome.Text = "<< &Home";
            this.linkLabelGoHome.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelGoHome_LinkClicked);
            // 
            // DatabaseDeployMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.linkLabelGoHome);
            this.Controls.Add(this.panelPageContainer);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.buttonBack);
            this.Controls.Add(this.labelDeployDatabase);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DatabaseDeployMain";
            this.Size = new System.Drawing.Size(670, 320);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private General.SmoothLabel labelDeployDatabase;
        private System.Windows.Forms.Button buttonBack;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Panel panelPageContainer;
        private System.Windows.Forms.LinkLabel linkLabelGoHome;
    }
}
