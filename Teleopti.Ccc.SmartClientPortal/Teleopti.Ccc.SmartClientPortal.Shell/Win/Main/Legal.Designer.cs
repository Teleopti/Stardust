namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Main
{
    partial class Legal
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Legal));
			this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.richTextBoxLegalNotice = new System.Windows.Forms.RichTextBox();
			this.buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
			this.gradientPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// gradientPanel2
			// 
			this.gradientPanel2.BackColor = System.Drawing.Color.Transparent;
			this.gradientPanel2.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))), System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))));
			this.gradientPanel2.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanel2.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
			this.gradientPanel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gradientPanel2.Controls.Add(this.richTextBoxLegalNotice);
			this.gradientPanel2.Controls.Add(this.buttonAdv2);
			this.gradientPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel2.Location = new System.Drawing.Point(0, 0);
			this.gradientPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanel2.Name = "gradientPanel2";
			this.gradientPanel2.Size = new System.Drawing.Size(681, 473);
			this.gradientPanel2.TabIndex = 8;
			// 
			// richTextBoxLegalNotice
			// 
			this.richTextBoxLegalNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBoxLegalNotice.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxLegalNotice.Location = new System.Drawing.Point(0, 0);
			this.richTextBoxLegalNotice.Name = "richTextBoxLegalNotice";
			this.richTextBoxLegalNotice.ReadOnly = true;
			this.richTextBoxLegalNotice.ShowSelectionMargin = true;
			this.richTextBoxLegalNotice.Size = new System.Drawing.Size(681, 473);
			this.richTextBoxLegalNotice.TabIndex = 3;
			this.richTextBoxLegalNotice.Text = resources.GetString("richTextBoxLegalNotice.Text");
			// 
			// buttonAdv2
			// 
			this.buttonAdv2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdv2.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdv2.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdv2.IsBackStageButton = false;
			this.buttonAdv2.Location = new System.Drawing.Point(580, 438);
			this.buttonAdv2.Name = "buttonAdv2";
			this.buttonAdv2.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdv2.Size = new System.Drawing.Size(87, 27);
			this.buttonAdv2.TabIndex = 2;
			this.buttonAdv2.Text = "xxOk";
			this.buttonAdv2.UseVisualStyle = true;
			this.buttonAdv2.UseVisualStyleBackColor = true;
			this.buttonAdv2.Click += new System.EventHandler(this.buttonAdv2_Click);
			// 
			// Legal
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(681, 473);
			this.Controls.Add(this.gradientPanel2);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(697, 513);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(522, 513);
			this.Name = "Legal";
			this.ShowIcon = false;
			this.Text = "xxLegalNotice";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
			this.gradientPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
        private System.Windows.Forms.RichTextBox richTextBoxLegalNotice;
    }
}