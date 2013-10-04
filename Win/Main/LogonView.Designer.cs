namespace Teleopti.Ccc.Win.Main
{
	partial class LogonView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogonView));
            this.pnlContent = new System.Windows.Forms.Panel();
            this.buttonLogOnCancel = new System.Windows.Forms.Button();
            this.buttonLogOnOK = new System.Windows.Forms.Button();
            this.labelStatusText = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // pnlContent
            // 
            this.pnlContent.BackColor = System.Drawing.Color.Transparent;
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlContent.Location = new System.Drawing.Point(0, 0);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(483, 242);
            this.pnlContent.TabIndex = 5;
            // 
            // buttonLogOnCancel
            // 
            this.buttonLogOnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogOnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonLogOnCancel.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLogOnCancel.Location = new System.Drawing.Point(399, 308);
            this.buttonLogOnCancel.Name = "buttonLogOnCancel";
            this.buttonLogOnCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonLogOnCancel.TabIndex = 44;
            this.buttonLogOnCancel.Text = "xxCancel";
            this.buttonLogOnCancel.UseVisualStyleBackColor = true;
            this.buttonLogOnCancel.Click += new System.EventHandler(this.buttonLogOnCancelClick);
            // 
            // buttonLogOnOK
            // 
            this.buttonLogOnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLogOnOK.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonLogOnOK.Location = new System.Drawing.Point(311, 308);
            this.buttonLogOnOK.Name = "buttonLogOnOK";
            this.buttonLogOnOK.Size = new System.Drawing.Size(75, 23);
            this.buttonLogOnOK.TabIndex = 43;
            this.buttonLogOnOK.Text = "xxOK";
            this.buttonLogOnOK.UseVisualStyleBackColor = true;
            this.buttonLogOnOK.Click += new System.EventHandler(this.buttonLogOnOkClick);
            // 
            // labelStatusText
            // 
            this.labelStatusText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatusText.ForeColor = System.Drawing.Color.Orange;
            this.labelStatusText.Location = new System.Drawing.Point(2, 305);
            this.labelStatusText.Name = "labelStatusText";
            this.labelStatusText.Size = new System.Drawing.Size(481, 25);
            this.labelStatusText.TabIndex = 42;
            this.labelStatusText.Text = "xxSearching for data sources...";
            this.labelStatusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnBack
            // 
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBack.Location = new System.Drawing.Point(222, 308);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(75, 23);
            this.btnBack.TabIndex = 46;
            this.btnBack.Text = "xxBack";
            this.btnBack.UseVisualStyleBackColor = true;
            // 
            // LogonView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CornflowerBlue;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(483, 345);
            this.Controls.Add(this.buttonLogOnCancel);
            this.Controls.Add(this.buttonLogOnOK);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.labelStatusText);
            this.Controls.Add(this.pnlContent);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "LogonView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LogonView";
            this.Shown += new System.EventHandler(this.logonViewShown);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Label labelStatusText;
        private System.Windows.Forms.Button buttonLogOnCancel;
        private System.Windows.Forms.Button buttonLogOnOK;
        private System.Windows.Forms.Button btnBack;
	}
}