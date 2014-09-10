namespace Teleopti.Support.Tool.Controls.DatabaseDeployment.Pages
{
    partial class NHibernateSessionFilePage
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
            this.label1 = new System.Windows.Forms.Label();
            this.labelAppDB = new System.Windows.Forms.Label();
            this.labelAnalyticsDB = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelAggDB = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxNhibFilePath = new System.Windows.Forms.TextBox();
            this.buttonBrowseNHibLocation = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSessionName = new System.Windows.Forms.TextBox();
            this.buttonDeploy = new System.Windows.Forms.Button();
            this.folderBrowserDialogNhib = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Application DB:";
            // 
            // labelAppDB
            // 
            this.labelAppDB.AutoSize = true;
            this.labelAppDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAppDB.Location = new System.Drawing.Point(89, 10);
            this.labelAppDB.Name = "labelAppDB";
            this.labelAppDB.Size = new System.Drawing.Size(84, 13);
            this.labelAppDB.TabIndex = 1;
            this.labelAppDB.Text = "TeleoptiCCC7";
            // 
            // labelAnalyticsDB
            // 
            this.labelAnalyticsDB.AutoSize = true;
            this.labelAnalyticsDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnalyticsDB.Location = new System.Drawing.Point(89, 33);
            this.labelAnalyticsDB.Name = "labelAnalyticsDB";
            this.labelAnalyticsDB.Size = new System.Drawing.Size(104, 13);
            this.labelAnalyticsDB.TabIndex = 3;
            this.labelAnalyticsDB.Text = "TeleoptiAnalytics";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Analytics DB:";
            // 
            // labelAggDB
            // 
            this.labelAggDB.AutoSize = true;
            this.labelAggDB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAggDB.Location = new System.Drawing.Point(89, 57);
            this.labelAggDB.Name = "labelAggDB";
            this.labelAggDB.Size = new System.Drawing.Size(84, 13);
            this.labelAggDB.TabIndex = 5;
            this.labelAggDB.Text = "TeleoptiCCC7";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 57);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Application DB:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 117);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "NHib File";
            // 
            // textBoxNhibFilePath
            // 
            this.textBoxNhibFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNhibFilePath.Location = new System.Drawing.Point(92, 114);
            this.textBoxNhibFilePath.Name = "textBoxNhibFilePath";
            this.textBoxNhibFilePath.Size = new System.Drawing.Size(531, 20);
            this.textBoxNhibFilePath.TabIndex = 7;
            // 
            // buttonBrowseNHibLocation
            // 
            this.buttonBrowseNHibLocation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseNHibLocation.Location = new System.Drawing.Point(629, 111);
            this.buttonBrowseNHibLocation.Name = "buttonBrowseNHibLocation";
            this.buttonBrowseNHibLocation.Size = new System.Drawing.Size(31, 25);
            this.buttonBrowseNHibLocation.TabIndex = 8;
            this.buttonBrowseNHibLocation.Text = "...";
            this.buttonBrowseNHibLocation.UseVisualStyleBackColor = true;
            this.buttonBrowseNHibLocation.Click += new System.EventHandler(this.buttonBrowseNHibLocation_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Session Name";
            // 
            // textBoxSessionName
            // 
            this.textBoxSessionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSessionName.Location = new System.Drawing.Point(92, 140);
            this.textBoxSessionName.Name = "textBoxSessionName";
            this.textBoxSessionName.Size = new System.Drawing.Size(399, 20);
            this.textBoxSessionName.TabIndex = 10;
            // 
            // buttonDeploy
            // 
            this.buttonDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeploy.BackColor = System.Drawing.Color.LightGreen;
            this.buttonDeploy.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDeploy.Location = new System.Drawing.Point(512, 180);
            this.buttonDeploy.Name = "buttonDeploy";
            this.buttonDeploy.Size = new System.Drawing.Size(148, 42);
            this.buttonDeploy.TabIndex = 11;
            this.buttonDeploy.Text = "Deploy!";
            this.buttonDeploy.UseVisualStyleBackColor = false;
            this.buttonDeploy.Click += new System.EventHandler(this.buttonDeploy_Click);
            // 
            // NHibernateSessionFilePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.buttonDeploy);
            this.Controls.Add(this.textBoxSessionName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.buttonBrowseNHibLocation);
            this.Controls.Add(this.textBoxNhibFilePath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.labelAggDB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelAnalyticsDB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelAppDB);
            this.Controls.Add(this.label1);
            this.Name = "NHibernateSessionFilePage";
            this.Size = new System.Drawing.Size(660, 240);
            this.Load += new System.EventHandler(this.nHibernateSessionFilePage_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelAppDB;
        private System.Windows.Forms.Label labelAnalyticsDB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelAggDB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxNhibFilePath;
        private System.Windows.Forms.Button buttonBrowseNHibLocation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxSessionName;
        private System.Windows.Forms.Button buttonDeploy;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogNhib;
    }
}
