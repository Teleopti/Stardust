namespace Teleopti.Ccc.Win.Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Legal));
            this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.richTextBoxLegalNotice = new System.Windows.Forms.RichTextBox();
            this.buttonAdv2 = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.ribbonControlAdvFixed1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
            this.gradientPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).BeginInit();
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
            this.gradientPanel2.Location = new System.Drawing.Point(6, 34);
            this.gradientPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.gradientPanel2.Name = "gradientPanel2";
            this.gradientPanel2.Size = new System.Drawing.Size(588, 410);
            this.gradientPanel2.TabIndex = 8;
            // 
            // richTextBoxLegalNotice
            // 
            this.richTextBoxLegalNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxLegalNotice.Location = new System.Drawing.Point(0, -2);
            this.richTextBoxLegalNotice.Name = "richTextBoxLegalNotice";
            this.richTextBoxLegalNotice.ReadOnly = true;
            this.richTextBoxLegalNotice.ShowSelectionMargin = true;
            this.richTextBoxLegalNotice.Size = new System.Drawing.Size(588, 374);
            this.richTextBoxLegalNotice.TabIndex = 3;
            this.richTextBoxLegalNotice.Text = resources.GetString("richTextBoxLegalNotice.Text");
            // 
            // buttonAdv2
            // 
            this.buttonAdv2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdv2.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdv2.Location = new System.Drawing.Point(501, 380);
            this.buttonAdv2.Name = "buttonAdv2";
            this.buttonAdv2.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdv2.Size = new System.Drawing.Size(75, 23);
            this.buttonAdv2.TabIndex = 2;
            this.buttonAdv2.Text = "xxOk";
            this.buttonAdv2.UseVisualStyle = true;
            this.buttonAdv2.UseVisualStyleBackColor = true;
            this.buttonAdv2.Click += new System.EventHandler(this.buttonAdv2_Click);
            // 
            // buttonAdvOk
            // 
            this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOk.Location = new System.Drawing.Point(507, 414);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvOk.TabIndex = 7;
            this.buttonAdvOk.Text = "xxOk";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.UseVisualStyleBackColor = true;
            // 
            // ribbonControlAdvFixed1
            // 
            this.ribbonControlAdvFixed1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdvFixed1.MenuButtonVisible = false;
            this.ribbonControlAdvFixed1.Name = "ribbonControlAdvFixed1";
            // 
            // ribbonControlAdvFixed1.OfficeMenu
            // 
            this.ribbonControlAdvFixed1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdvFixed1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdvFixed1.QuickPanelVisible = false;
            this.ribbonControlAdvFixed1.SelectedTab = null;
            this.ribbonControlAdvFixed1.Size = new System.Drawing.Size(598, 33);
            this.ribbonControlAdvFixed1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdvFixed1.TabIndex = 6;
            this.ribbonControlAdvFixed1.Text = "xxLegalNotice";
            // 
            // Legal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 450);
            this.Controls.Add(this.gradientPanel2);
            this.Controls.Add(this.buttonAdvOk);
            this.Controls.Add(this.ribbonControlAdvFixed1);
            this.MaximumSize = new System.Drawing.Size(600, 450);
            this.MinimumSize = new System.Drawing.Size(450, 450);
            this.Name = "Legal";
            this.ShowIcon = false;
            this.Text = "xxLegalNotice";
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
            this.gradientPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvFixed1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdv2;
        private System.Windows.Forms.RichTextBox richTextBoxLegalNotice;
    }
}