namespace Teleopti.Ccc.Win.Main
{
    partial class ApplyLicense
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
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.buttonAdvApply = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonBrowse = new Syncfusion.Windows.Forms.ButtonAdv();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.textBoxExtLicenseFilePath = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.labelExplanation = new System.Windows.Forms.Label();
            this.textBoxIntructions = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtLicenseFilePath)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonText = "";
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.SelectedTab = null;
            this.ribbonControlAdv1.ShowMinimizeButton = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(581, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "xxStartmenu";
            this.ribbonControlAdv1.TabIndex = 0;
            this.ribbonControlAdv1.Text = "xxApplyLicense";
            // 
            // buttonAdvApply
            // 
            this.buttonAdvApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvApply.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvApply.Location = new System.Drawing.Point(484, 356);
            this.buttonAdvApply.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            this.buttonAdvApply.Name = "buttonAdvApply";
            this.buttonAdvApply.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvApply.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvApply.TabIndex = 3;
            this.buttonAdvApply.Text = "xxApply";
            this.buttonAdvApply.UseVisualStyle = true;
            this.buttonAdvApply.UseVisualStyleBackColor = true;
            this.buttonAdvApply.Click += new System.EventHandler(this.buttonAdvApply_Click);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.Location = new System.Drawing.Point(397, 356);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 2;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.UseVisualStyleBackColor = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonBrowse.Location = new System.Drawing.Point(484, 326);
            this.buttonBrowse.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "xxBrowse";
            this.buttonBrowse.UseVisualStyle = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "XML Files|*.xml|All Files|*.*";
            this.openFileDialog.Title = "xxOpenLicenseFile";
            // 
            // textBoxExtLicenseFilePath
            // 
            this.textBoxExtLicenseFilePath.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxExtLicenseFilePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxExtLicenseFilePath.Location = new System.Drawing.Point(3, 328);
            this.textBoxExtLicenseFilePath.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.textBoxExtLicenseFilePath.Name = "textBoxExtLicenseFilePath";
            this.textBoxExtLicenseFilePath.OverflowIndicatorToolTipText = null;
            this.textBoxExtLicenseFilePath.Size = new System.Drawing.Size(469, 20);
            this.textBoxExtLicenseFilePath.TabIndex = 0;
            // 
            // labelExplanation
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.labelExplanation, 2);
            this.labelExplanation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelExplanation.Location = new System.Drawing.Point(3, 0);
            this.labelExplanation.Name = "labelExplanation";
            this.labelExplanation.Size = new System.Drawing.Size(565, 30);
            this.labelExplanation.TabIndex = 4;
            this.labelExplanation.Tag = "";
            this.labelExplanation.Text = "Explanation";
            // 
            // textBoxIntructions
            // 
            this.textBoxIntructions.BackColor = System.Drawing.SystemColors.Window;
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxIntructions, 2);
            this.textBoxIntructions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxIntructions.Location = new System.Drawing.Point(3, 33);
            this.textBoxIntructions.Multiline = true;
            this.textBoxIntructions.Name = "textBoxIntructions";
            this.textBoxIntructions.ReadOnly = true;
            this.textBoxIntructions.Size = new System.Drawing.Size(565, 286);
            this.textBoxIntructions.TabIndex = 5;
            this.textBoxIntructions.Text = "xxApplyLicenseInstructions";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.tableLayoutPanel1.Controls.Add(this.labelExplanation, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvApply, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonBrowse, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxExtLicenseFilePath, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBoxIntructions, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(571, 382);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // ApplyLicense
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(583, 422);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(207, 243);
            this.Name = "ApplyLicense";
            this.ShowIcon = false;
            this.Text = "xxApplyLicense";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxExtLicenseFilePath)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvApply;
        private Syncfusion.Windows.Forms.ButtonAdv buttonBrowse;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxExtLicenseFilePath;
        private System.Windows.Forms.Label labelExplanation;
        private System.Windows.Forms.TextBox textBoxIntructions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}