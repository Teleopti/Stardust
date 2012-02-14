namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
    partial class ScheduleReportDialog
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
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonAllDetails = new System.Windows.Forms.RadioButton();
            this.radioButtonBreaksOnly = new System.Windows.Forms.RadioButton();
            this.radioButtonNoDetails = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonShiftsPerDay = new System.Windows.Forms.RadioButton();
            this.checkBoxSingleFile = new System.Windows.Forms.CheckBox();
            this.radioButtonIndividualReport = new System.Windows.Forms.RadioButton();
            this.radioButtonTeamReport = new System.Windows.Forms.RadioButton();
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonAdvOk
            // 
            this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOk.Location = new System.Drawing.Point(193, 243);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvOk.TabIndex = 2;
            this.buttonAdvOk.Text = "xxOk";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOk_Click);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(285, 243);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 3;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel2.SetColumnSpan(this.groupBox2, 3);
            this.groupBox2.Controls.Add(this.radioButtonAllDetails);
            this.groupBox2.Controls.Add(this.radioButtonBreaksOnly);
            this.groupBox2.Controls.Add(this.radioButtonNoDetails);
            this.groupBox2.Location = new System.Drawing.Point(3, 130);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(370, 100);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "xxDisplay";
            // 
            // radioButtonAllDetails
            // 
            this.radioButtonAllDetails.AutoSize = true;
            this.radioButtonAllDetails.Checked = true;
            this.radioButtonAllDetails.Location = new System.Drawing.Point(7, 68);
            this.radioButtonAllDetails.Name = "radioButtonAllDetails";
            this.radioButtonAllDetails.Size = new System.Drawing.Size(88, 17);
            this.radioButtonAllDetails.TabIndex = 2;
            this.radioButtonAllDetails.TabStop = true;
            this.radioButtonAllDetails.Text = "xxAllActivities";
            this.radioButtonAllDetails.UseVisualStyleBackColor = true;
            // 
            // radioButtonBreaksOnly
            // 
            this.radioButtonBreaksOnly.AutoSize = true;
            this.radioButtonBreaksOnly.Location = new System.Drawing.Point(7, 44);
            this.radioButtonBreaksOnly.Name = "radioButtonBreaksOnly";
            this.radioButtonBreaksOnly.Size = new System.Drawing.Size(68, 17);
            this.radioButtonBreaksOnly.TabIndex = 1;
            this.radioButtonBreaksOnly.TabStop = true;
            this.radioButtonBreaksOnly.Text = "xxBreaks";
            this.radioButtonBreaksOnly.UseVisualStyleBackColor = true;
            // 
            // radioButtonNoDetails
            // 
            this.radioButtonNoDetails.AutoSize = true;
            this.radioButtonNoDetails.Location = new System.Drawing.Point(7, 20);
            this.radioButtonNoDetails.Name = "radioButtonNoDetails";
            this.radioButtonNoDetails.Size = new System.Drawing.Size(91, 17);
            this.radioButtonNoDetails.TabIndex = 0;
            this.radioButtonNoDetails.TabStop = true;
            this.radioButtonNoDetails.Text = "xxNoActivities";
            this.radioButtonNoDetails.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel2.SetColumnSpan(this.groupBox1, 3);
            this.groupBox1.Controls.Add(this.radioButtonShiftsPerDay);
            this.groupBox1.Controls.Add(this.checkBoxSingleFile);
            this.groupBox1.Controls.Add(this.radioButtonIndividualReport);
            this.groupBox1.Controls.Add(this.radioButtonTeamReport);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(370, 114);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "xxType";
            // 
            // radioButtonShiftsPerDay
            // 
            this.radioButtonShiftsPerDay.AutoSize = true;
            this.radioButtonShiftsPerDay.Location = new System.Drawing.Point(6, 20);
            this.radioButtonShiftsPerDay.Name = "radioButtonShiftsPerDay";
            this.radioButtonShiftsPerDay.Size = new System.Drawing.Size(96, 17);
            this.radioButtonShiftsPerDay.TabIndex = 3;
            this.radioButtonShiftsPerDay.TabStop = true;
            this.radioButtonShiftsPerDay.Text = "xxShiftsPerDay";
            this.radioButtonShiftsPerDay.UseVisualStyleBackColor = true;
            this.radioButtonShiftsPerDay.CheckedChanged += new System.EventHandler(this.radioButtonShiftsPerDay_CheckedChanged);
            // 
            // checkBoxSingleFile
            // 
            this.checkBoxSingleFile.AutoSize = true;
            this.checkBoxSingleFile.Checked = true;
            this.checkBoxSingleFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSingleFile.Location = new System.Drawing.Point(37, 89);
            this.checkBoxSingleFile.Name = "checkBoxSingleFile";
            this.checkBoxSingleFile.Size = new System.Drawing.Size(81, 17);
            this.checkBoxSingleFile.TabIndex = 2;
            this.checkBoxSingleFile.Text = "xxSingleFile";
            this.checkBoxSingleFile.UseVisualStyleBackColor = true;
            // 
            // radioButtonIndividualReport
            // 
            this.radioButtonIndividualReport.AutoSize = true;
            this.radioButtonIndividualReport.Checked = true;
            this.radioButtonIndividualReport.Location = new System.Drawing.Point(6, 66);
            this.radioButtonIndividualReport.Name = "radioButtonIndividualReport";
            this.radioButtonIndividualReport.Size = new System.Drawing.Size(80, 17);
            this.radioButtonIndividualReport.TabIndex = 1;
            this.radioButtonIndividualReport.TabStop = true;
            this.radioButtonIndividualReport.Text = "xxIndividual";
            this.radioButtonIndividualReport.UseVisualStyleBackColor = true;
            this.radioButtonIndividualReport.CheckedChanged += new System.EventHandler(this.radioButtonIndividualReport_CheckedChanged);
            // 
            // radioButtonTeamReport
            // 
            this.radioButtonTeamReport.AutoSize = true;
            this.radioButtonTeamReport.Location = new System.Drawing.Point(6, 43);
            this.radioButtonTeamReport.Name = "radioButtonTeamReport";
            this.radioButtonTeamReport.Size = new System.Drawing.Size(62, 17);
            this.radioButtonTeamReport.TabIndex = 0;
            this.radioButtonTeamReport.Text = "xxTeam";
            this.radioButtonTeamReport.UseVisualStyleBackColor = true;
            // 
            // ribbonControlAdv1
            // 
            this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
            this.ribbonControlAdv1.MenuButtonVisible = false;
            this.ribbonControlAdv1.Name = "ribbonControlAdv1";
            // 
            // ribbonControlAdv1.OfficeMenu
            // 
            this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
            this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
            this.ribbonControlAdv1.QuickPanelVisible = false;
            this.ribbonControlAdv1.Size = new System.Drawing.Size(386, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Startmenu";
            this.ribbonControlAdv1.TabIndex = 0;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.buttonAdvCancel, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(376, 273);
            this.tableLayoutPanel2.TabIndex = 4;
            // 
            // ScheduleReportDialog
            // 
            this.AcceptButton = this.buttonAdvOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(388, 313);
            this.Controls.Add(this.ribbonControlAdv1);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScheduleReportDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxExportToPDF";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ScheduleReportDialog_FormClosed);
            this.Load += new System.EventHandler(this.ScheduleReportDialog_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Syncfusion.Windows.Forms.Tools.RibbonControlAdvFixed ribbonControlAdv1;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxSingleFile;
        private System.Windows.Forms.RadioButton radioButtonIndividualReport;
        private System.Windows.Forms.RadioButton radioButtonTeamReport;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButtonAllDetails;
        private System.Windows.Forms.RadioButton radioButtonBreaksOnly;
        private System.Windows.Forms.RadioButton radioButtonNoDetails;
        private System.Windows.Forms.RadioButton radioButtonShiftsPerDay;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    }
}