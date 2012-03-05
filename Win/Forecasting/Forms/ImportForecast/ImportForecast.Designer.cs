namespace Teleopti.Ccc.Win.Forecasting.Forms.ImportForecast
{
    partial class ImportForecastForm
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
            this.ribbonControlAdvFixed1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblSkillNameColon = new System.Windows.Forms.Label();
            this.txtSkillName = new System.Windows.Forms.Label();
            this.lblWorkloadNameColon = new System.Windows.Forms.Label();
            this.comboBoxAdvWorkloads = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv();
            this.gradientPanelImportForecast = new Syncfusion.Windows.Forms.Tools.GradientPanel();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvImport = new Syncfusion.Windows.Forms.ButtonAdv();
            this.progressBarImportForecast = new System.Windows.Forms.ProgressBar();
            this.lblImportFileName = new System.Windows.Forms.Label();
            this.textBoxImportFileName = new Syncfusion.Windows.Forms.Tools.TextBoxExt();
            this.buttonAdvBrowseFile = new Syncfusion.Windows.Forms.ButtonAdv();
            this.radioButtonImportWorkload = new System.Windows.Forms.RadioButton();
            this.radioButtonImportStaffing = new System.Windows.Forms.RadioButton();
            this.radioButtonImportWLAndStaffing = new System.Windows.Forms.RadioButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvWorkloads)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelImportForecast)).BeginInit();
            this.gradientPanelImportForecast.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxImportFileName)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControlAdvFixed1
            // 
            this.ribbonControlAdvFixed1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdvFixed1.MenuButtonText = "";
            this.ribbonControlAdvFixed1.MenuButtonVisible = false;
            this.ribbonControlAdvFixed1.Name = "ribbonControlAdvFixed1";
            // 
            // ribbonControlAdvFixed1.OfficeMenu
            // 
            this.ribbonControlAdvFixed1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdvFixed1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdvFixed1.QuickPanelVisible = false;
            this.ribbonControlAdvFixed1.SelectedTab = null;
            this.ribbonControlAdvFixed1.ShowMinimizeButton = false;
            this.ribbonControlAdvFixed1.Size = new System.Drawing.Size(497, 33);
            this.ribbonControlAdvFixed1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdvFixed1.TabIndex = 0;
            this.ribbonControlAdvFixed1.Text = "xxImportForecast";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35.36585F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64.63415F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 173F));
            this.tableLayoutPanel1.Controls.Add(this.lblSkillNameColon, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSkillName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblWorkloadNameColon, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxAdvWorkloads, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.gradientPanelImportForecast, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.progressBarImportForecast, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblImportFileName, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBoxImportFileName, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvBrowseFile, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.radioButtonImportStaffing, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWorkload, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.radioButtonImportWLAndStaffing, 2, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(487, 158);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lblSkillNameColon
            // 
            this.lblSkillNameColon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblSkillNameColon.AutoSize = true;
            this.lblSkillNameColon.BackColor = System.Drawing.Color.Transparent;
            this.lblSkillNameColon.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSkillNameColon.Location = new System.Drawing.Point(3, 6);
            this.lblSkillNameColon.Name = "lblSkillNameColon";
            this.lblSkillNameColon.Size = new System.Drawing.Size(91, 13);
            this.lblSkillNameColon.TabIndex = 0;
            this.lblSkillNameColon.Text = "xxSkillNameColon";
            this.lblSkillNameColon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSkillName
            // 
            this.txtSkillName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtSkillName.AutoSize = true;
            this.txtSkillName.BackColor = System.Drawing.Color.Transparent;
            this.txtSkillName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSkillName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.txtSkillName.Location = new System.Drawing.Point(114, 6);
            this.txtSkillName.Name = "txtSkillName";
            this.txtSkillName.Size = new System.Drawing.Size(0, 13);
            this.txtSkillName.TabIndex = 1;
            this.txtSkillName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblWorkloadNameColon
            // 
            this.lblWorkloadNameColon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblWorkloadNameColon.AutoSize = true;
            this.lblWorkloadNameColon.BackColor = System.Drawing.Color.Transparent;
            this.lblWorkloadNameColon.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblWorkloadNameColon.Location = new System.Drawing.Point(3, 25);
            this.lblWorkloadNameColon.Name = "lblWorkloadNameColon";
            this.lblWorkloadNameColon.Size = new System.Drawing.Size(104, 25);
            this.lblWorkloadNameColon.TabIndex = 2;
            this.lblWorkloadNameColon.Text = "xxWorkloadNameColon";
            this.lblWorkloadNameColon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxAdvWorkloads
            // 
            this.comboBoxAdvWorkloads.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBoxAdvWorkloads.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
            this.comboBoxAdvWorkloads.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAdvWorkloads.DropDownWidth = 294;
            this.comboBoxAdvWorkloads.Location = new System.Drawing.Point(114, 28);
            this.comboBoxAdvWorkloads.Name = "comboBoxAdvWorkloads";
            this.comboBoxAdvWorkloads.Size = new System.Drawing.Size(172, 21);
            this.comboBoxAdvWorkloads.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
            this.comboBoxAdvWorkloads.TabIndex = 3;
            // 
            // gradientPanelImportForecast
            // 
            this.gradientPanelImportForecast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.gradientPanelImportForecast.BackColor = System.Drawing.Color.Transparent;
            this.gradientPanelImportForecast.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255))))), System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252))))));
            this.gradientPanelImportForecast.BorderSingle = System.Windows.Forms.ButtonBorderStyle.None;
            this.gradientPanelImportForecast.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel1.SetColumnSpan(this.gradientPanelImportForecast, 3);
            this.gradientPanelImportForecast.Controls.Add(this.buttonAdvCancel);
            this.gradientPanelImportForecast.Controls.Add(this.buttonAdvImport);
            this.gradientPanelImportForecast.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gradientPanelImportForecast.Location = new System.Drawing.Point(3, 128);
            this.gradientPanelImportForecast.Name = "gradientPanelImportForecast";
            this.gradientPanelImportForecast.Size = new System.Drawing.Size(481, 30);
            this.gradientPanelImportForecast.TabIndex = 1;
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(375, 5);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvCancel.Size = new System.Drawing.Size(87, 23);
            this.buttonAdvCancel.TabIndex = 1;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            // 
            // buttonAdvImport
            // 
            this.buttonAdvImport.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvImport.BackColor = System.Drawing.Color.Transparent;
            this.buttonAdvImport.Location = new System.Drawing.Point(282, 5);
            this.buttonAdvImport.Name = "buttonAdvImport";
            this.buttonAdvImport.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvImport.Size = new System.Drawing.Size(87, 23);
            this.buttonAdvImport.TabIndex = 0;
            this.buttonAdvImport.Text = "xxImport";
            this.buttonAdvImport.UseVisualStyle = true;
            // 
            // progressBarImportForecast
            // 
            this.progressBarImportForecast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.progressBarImportForecast, 3);
            this.progressBarImportForecast.Location = new System.Drawing.Point(3, 103);
            this.progressBarImportForecast.Name = "progressBarImportForecast";
            this.progressBarImportForecast.Size = new System.Drawing.Size(481, 19);
            this.progressBarImportForecast.TabIndex = 0;
            // 
            // lblImportFileName
            // 
            this.lblImportFileName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblImportFileName.AutoSize = true;
            this.lblImportFileName.BackColor = System.Drawing.Color.Transparent;
            this.lblImportFileName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblImportFileName.Location = new System.Drawing.Point(3, 75);
            this.lblImportFileName.Name = "lblImportFileName";
            this.lblImportFileName.Size = new System.Drawing.Size(105, 25);
            this.lblImportFileName.TabIndex = 4;
            this.lblImportFileName.Text = "xxImportFileNameColon";
            this.lblImportFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxImportFileName
            // 
            this.textBoxImportFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxImportFileName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxImportFileName.Location = new System.Drawing.Point(114, 78);
            this.textBoxImportFileName.Name = "textBoxImportFileName";
            this.textBoxImportFileName.Size = new System.Drawing.Size(196, 20);
            this.textBoxImportFileName.TabIndex = 0;
            // 
            // buttonAdvBrowseFile
            // 
            this.buttonAdvBrowseFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.buttonAdvBrowseFile.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvBrowseFile.BackColor = System.Drawing.Color.Transparent;
            this.buttonAdvBrowseFile.ForeColor = System.Drawing.SystemColors.ControlText;
            this.buttonAdvBrowseFile.Location = new System.Drawing.Point(316, 78);
            this.buttonAdvBrowseFile.Name = "buttonAdvBrowseFile";
            this.buttonAdvBrowseFile.Size = new System.Drawing.Size(87, 19);
            this.buttonAdvBrowseFile.TabIndex = 1;
            this.buttonAdvBrowseFile.Text = "Browse";
            this.buttonAdvBrowseFile.UseVisualStyle = true;
            this.buttonAdvBrowseFile.Click += new System.EventHandler(this.browseImportFileButton_Click);
            // 
            // radioButtonImportWorkload
            // 
            this.radioButtonImportWorkload.AutoSize = true;
            this.radioButtonImportWorkload.Location = new System.Drawing.Point(316, 28);
            this.radioButtonImportWorkload.Name = "radioButtonImportWorkload";
            this.radioButtonImportWorkload.Size = new System.Drawing.Size(110, 17);
            this.radioButtonImportWorkload.TabIndex = 5;
            this.radioButtonImportWorkload.TabStop = true;
            this.radioButtonImportWorkload.Text = "xxImportWorkload";
            this.radioButtonImportWorkload.UseVisualStyleBackColor = true;
            // 
            // radioButtonImportStaffing
            // 
            this.radioButtonImportStaffing.AutoSize = true;
            this.radioButtonImportStaffing.Location = new System.Drawing.Point(316, 3);
            this.radioButtonImportStaffing.Name = "radioButtonImportStaffing";
            this.radioButtonImportStaffing.Size = new System.Drawing.Size(100, 17);
            this.radioButtonImportStaffing.TabIndex = 6;
            this.radioButtonImportStaffing.TabStop = true;
            this.radioButtonImportStaffing.Text = "xxImportStaffing";
            this.radioButtonImportStaffing.UseVisualStyleBackColor = true;
            // 
            // radioButtonImportWLAndStaffing
            // 
            this.radioButtonImportWLAndStaffing.AutoSize = true;
            this.radioButtonImportWLAndStaffing.Location = new System.Drawing.Point(316, 53);
            this.radioButtonImportWLAndStaffing.Name = "radioButtonImportWLAndStaffing";
            this.radioButtonImportWLAndStaffing.Size = new System.Drawing.Size(86, 17);
            this.radioButtonImportWLAndStaffing.TabIndex = 7;
            this.radioButtonImportWLAndStaffing.TabStop = true;
            this.radioButtonImportWLAndStaffing.Text = "xxImportBoth";
            this.radioButtonImportWLAndStaffing.UseVisualStyleBackColor = true;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // ImportForecastForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(499, 198);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlAdvFixed1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MaximizeBox = false;
            this.Name = "ImportForecastForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxImportForecast";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdvFixed1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAdvWorkloads)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gradientPanelImportForecast)).EndInit();
            this.gradientPanelImportForecast.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxImportFileName)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdvFixed1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblSkillNameColon;
        private System.Windows.Forms.Label txtSkillName;
        private System.Windows.Forms.Label lblWorkloadNameColon;
        private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxAdvWorkloads;
        private System.Windows.Forms.Label lblImportFileName;
        private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxImportFileName;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvBrowseFile;
        private System.Windows.Forms.ProgressBar progressBarImportForecast;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelImportForecast;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvImport;
        private System.Windows.Forms.RadioButton radioButtonImportWorkload;
        private System.Windows.Forms.RadioButton radioButtonImportStaffing;
        private System.Windows.Forms.RadioButton radioButtonImportWLAndStaffing;
    }
}