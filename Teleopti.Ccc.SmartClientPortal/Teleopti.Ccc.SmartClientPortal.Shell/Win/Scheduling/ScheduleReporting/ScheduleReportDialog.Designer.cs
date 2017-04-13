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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxShowPublicNote")]
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.checkBoxShowPublicNote = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.radioButtonAllDetails = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonBreaksOnly = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonNoDetails = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxSingleFile = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.radioButtonIndividualReport = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonTeamReport = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxShowPublicNote)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAllDetails)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonBreaksOnly)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonNoDetails)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxSingleFile)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonIndividualReport)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTeamReport)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(231, 273);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.TabIndex = 2;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(341, 273);
			this.buttonAdvCancel.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvCancel.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvCancel.TabIndex = 3;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// groupBox2
			// 
			this.tableLayoutPanel2.SetColumnSpan(this.groupBox2, 3);
			this.groupBox2.Controls.Add(this.checkBoxShowPublicNote);
			this.groupBox2.Controls.Add(this.radioButtonAllDetails);
			this.groupBox2.Controls.Add(this.radioButtonBreaksOnly);
			this.groupBox2.Controls.Add(this.radioButtonNoDetails);
			this.groupBox2.Location = new System.Drawing.Point(3, 123);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(432, 134);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "xxDisplay";
			// 
			// checkBoxShowPublicNote
			// 
			this.checkBoxShowPublicNote.BeforeTouchSize = new System.Drawing.Size(337, 20);
			this.checkBoxShowPublicNote.DrawFocusRectangle = false;
			this.checkBoxShowPublicNote.Location = new System.Drawing.Point(8, 108);
			this.checkBoxShowPublicNote.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxShowPublicNote.Name = "checkBoxShowPublicNote";
			this.checkBoxShowPublicNote.Size = new System.Drawing.Size(337, 20);
			this.checkBoxShowPublicNote.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxShowPublicNote.TabIndex = 3;
			this.checkBoxShowPublicNote.Text = "xxShowPublicNote";
			this.checkBoxShowPublicNote.ThemesEnabled = false;
			// 
			// radioButtonAllDetails
			// 
			this.radioButtonAllDetails.BeforeTouchSize = new System.Drawing.Size(353, 20);
			this.radioButtonAllDetails.Checked = true;
			this.radioButtonAllDetails.DrawFocusRectangle = false;
			this.radioButtonAllDetails.Location = new System.Drawing.Point(8, 78);
			this.radioButtonAllDetails.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAllDetails.Name = "radioButtonAllDetails";
			this.radioButtonAllDetails.Size = new System.Drawing.Size(353, 20);
			this.radioButtonAllDetails.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAllDetails.TabIndex = 2;
			this.radioButtonAllDetails.Text = "xxAllActivities";
			this.radioButtonAllDetails.ThemesEnabled = false;
			// 
			// radioButtonBreaksOnly
			// 
			this.radioButtonBreaksOnly.BeforeTouchSize = new System.Drawing.Size(337, 20);
			this.radioButtonBreaksOnly.DrawFocusRectangle = false;
			this.radioButtonBreaksOnly.Location = new System.Drawing.Point(8, 51);
			this.radioButtonBreaksOnly.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonBreaksOnly.Name = "radioButtonBreaksOnly";
			this.radioButtonBreaksOnly.Size = new System.Drawing.Size(337, 20);
			this.radioButtonBreaksOnly.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonBreaksOnly.TabIndex = 1;
			this.radioButtonBreaksOnly.Text = "xxBreaks";
			this.radioButtonBreaksOnly.ThemesEnabled = false;
			// 
			// radioButtonNoDetails
			// 
			this.radioButtonNoDetails.BeforeTouchSize = new System.Drawing.Size(337, 20);
			this.radioButtonNoDetails.DrawFocusRectangle = false;
			this.radioButtonNoDetails.Location = new System.Drawing.Point(8, 23);
			this.radioButtonNoDetails.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonNoDetails.Name = "radioButtonNoDetails";
			this.radioButtonNoDetails.Size = new System.Drawing.Size(337, 20);
			this.radioButtonNoDetails.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonNoDetails.TabIndex = 0;
			this.radioButtonNoDetails.Text = "xxNoActivities";
			this.radioButtonNoDetails.ThemesEnabled = false;
			// 
			// groupBox1
			// 
			this.tableLayoutPanel2.SetColumnSpan(this.groupBox1, 3);
			this.groupBox1.Controls.Add(this.checkBoxSingleFile);
			this.groupBox1.Controls.Add(this.radioButtonIndividualReport);
			this.groupBox1.Controls.Add(this.radioButtonTeamReport);
			this.groupBox1.Location = new System.Drawing.Point(3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(432, 105);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "xxType";
			// 
			// checkBoxSingleFile
			// 
			this.checkBoxSingleFile.BeforeTouchSize = new System.Drawing.Size(302, 20);
			this.checkBoxSingleFile.Checked = true;
			this.checkBoxSingleFile.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSingleFile.DrawFocusRectangle = false;
			this.checkBoxSingleFile.Location = new System.Drawing.Point(43, 75);
			this.checkBoxSingleFile.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxSingleFile.Name = "checkBoxSingleFile";
			this.checkBoxSingleFile.Size = new System.Drawing.Size(302, 20);
			this.checkBoxSingleFile.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxSingleFile.TabIndex = 2;
			this.checkBoxSingleFile.Text = "xxSingleFile";
			this.checkBoxSingleFile.ThemesEnabled = false;
			// 
			// radioButtonIndividualReport
			// 
			this.radioButtonIndividualReport.BeforeTouchSize = new System.Drawing.Size(354, 20);
			this.radioButtonIndividualReport.Checked = true;
			this.radioButtonIndividualReport.DrawFocusRectangle = false;
			this.radioButtonIndividualReport.Location = new System.Drawing.Point(7, 48);
			this.radioButtonIndividualReport.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonIndividualReport.Name = "radioButtonIndividualReport";
			this.radioButtonIndividualReport.Size = new System.Drawing.Size(354, 20);
			this.radioButtonIndividualReport.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonIndividualReport.TabIndex = 1;
			this.radioButtonIndividualReport.Text = "xxIndividual";
			this.radioButtonIndividualReport.ThemesEnabled = false;
			this.radioButtonIndividualReport.CheckChanged += new System.EventHandler(this.radioButtonIndividualReportCheckedChanged);
			// 
			// radioButtonTeamReport
			// 
			this.radioButtonTeamReport.BeforeTouchSize = new System.Drawing.Size(338, 20);
			this.radioButtonTeamReport.DrawFocusRectangle = false;
			this.radioButtonTeamReport.Location = new System.Drawing.Point(7, 22);
			this.radioButtonTeamReport.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonTeamReport.Name = "radioButtonTeamReport";
			this.radioButtonTeamReport.Size = new System.Drawing.Size(338, 20);
			this.radioButtonTeamReport.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonTeamReport.TabIndex = 0;
			this.radioButtonTeamReport.Text = "xxTeam";
			this.radioButtonTeamReport.ThemesEnabled = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 107F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvOk, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.buttonAdvCancel, 2, 2);
			this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(438, 310);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// ScheduleReportDialog
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(438, 310);
			this.Controls.Add(this.tableLayoutPanel2);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(212, 40);
			this.Name = "ScheduleReportDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxExportToPDF";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.scheduleReportDialogFormClosed);
			this.Load += new System.EventHandler(this.scheduleReportDialogLoad);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxShowPublicNote)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAllDetails)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonBreaksOnly)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonNoDetails)).EndInit();
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxSingleFile)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonIndividualReport)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTeamReport)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private System.Windows.Forms.GroupBox groupBox1;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxSingleFile;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonIndividualReport;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonTeamReport;
        private System.Windows.Forms.GroupBox groupBox2;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAllDetails;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonBreaksOnly;
		  private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonNoDetails;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv  checkBoxShowPublicNote;
    }
}