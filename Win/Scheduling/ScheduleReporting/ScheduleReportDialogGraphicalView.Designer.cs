namespace Teleopti.Ccc.Win.Scheduling.ScheduleReporting
{
	partial class ScheduleReportDialogGraphicalView
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxShowPublicNote"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxType"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxTeam"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxStartTime"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxSortOrder"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxSingleFile"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxIndividual"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxExportToPDFGraphical"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxEndTime"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxCancel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAgentName"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxPublicNote = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.checkBoxSingleFile = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.radioButtonIndividual = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonTeam = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.radioButtonEndTime = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonStartTime = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAgentName = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxPublicNote)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxSingleFile)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonIndividual)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTeam)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonEndTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonStartTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAgentName)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 3);
			this.groupBox1.Controls.Add(this.checkBoxPublicNote);
			this.groupBox1.Controls.Add(this.checkBoxSingleFile);
			this.groupBox1.Controls.Add(this.radioButtonIndividual);
			this.groupBox1.Controls.Add(this.radioButtonTeam);
			this.groupBox1.Location = new System.Drawing.Point(3, 3);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(427, 128);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "xxType";
			// 
			// checkBoxPublicNote
			// 
			this.checkBoxPublicNote.BeforeTouchSize = new System.Drawing.Size(134, 20);
			this.checkBoxPublicNote.Checked = true;
			this.checkBoxPublicNote.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxPublicNote.DrawFocusRectangle = false;
			this.checkBoxPublicNote.Location = new System.Drawing.Point(43, 48);
			this.checkBoxPublicNote.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxPublicNote.Name = "checkBoxPublicNote";
			this.checkBoxPublicNote.Size = new System.Drawing.Size(134, 20);
			this.checkBoxPublicNote.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxPublicNote.TabIndex = 3;
			this.checkBoxPublicNote.Text = "xxShowPublicNote";
			this.checkBoxPublicNote.ThemesEnabled = false;
			this.checkBoxPublicNote.CheckedChanged += new System.EventHandler(this.checkBoxPublicNoteCheckedChanged);
			// 
			// checkBoxSingleFile
			// 
			this.checkBoxSingleFile.BeforeTouchSize = new System.Drawing.Size(94, 20);
			this.checkBoxSingleFile.Checked = true;
			this.checkBoxSingleFile.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSingleFile.DrawFocusRectangle = false;
			this.checkBoxSingleFile.Location = new System.Drawing.Point(43, 96);
			this.checkBoxSingleFile.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.checkBoxSingleFile.Name = "checkBoxSingleFile";
			this.checkBoxSingleFile.Size = new System.Drawing.Size(94, 20);
			this.checkBoxSingleFile.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Metro;
			this.checkBoxSingleFile.TabIndex = 2;
			this.checkBoxSingleFile.Text = "xxSingleFile";
			this.checkBoxSingleFile.ThemesEnabled = false;
			this.checkBoxSingleFile.CheckedChanged += new System.EventHandler(this.checkBoxSingleFileCheckedChanged);
			// 
			// radioButtonIndividual
			// 
			this.radioButtonIndividual.BeforeTouchSize = new System.Drawing.Size(93, 20);
			this.radioButtonIndividual.Checked = true;
			this.radioButtonIndividual.DrawFocusRectangle = false;
			this.radioButtonIndividual.Location = new System.Drawing.Point(7, 69);
			this.radioButtonIndividual.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonIndividual.Name = "radioButtonIndividual";
			this.radioButtonIndividual.Size = new System.Drawing.Size(93, 20);
			this.radioButtonIndividual.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonIndividual.TabIndex = 1;
			this.radioButtonIndividual.Text = "xxIndividual";
			this.radioButtonIndividual.ThemesEnabled = false;
			this.radioButtonIndividual.CheckChanged += new System.EventHandler(this.radioButtonIndividualCheckedChanged);
			// 
			// radioButtonTeam
			// 
			this.radioButtonTeam.BeforeTouchSize = new System.Drawing.Size(72, 20);
			this.radioButtonTeam.DrawFocusRectangle = false;
			this.radioButtonTeam.Location = new System.Drawing.Point(7, 22);
			this.radioButtonTeam.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonTeam.Name = "radioButtonTeam";
			this.radioButtonTeam.Size = new System.Drawing.Size(72, 20);
			this.radioButtonTeam.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonTeam.TabIndex = 0;
			this.radioButtonTeam.Text = "xxTeam";
			this.radioButtonTeam.ThemesEnabled = false;
			this.radioButtonTeam.CheckChanged += new System.EventHandler(this.radioButtonTeamCheckedChanged);
			// 
			// groupBox2
			// 
			this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 3);
			this.groupBox2.Controls.Add(this.radioButtonEndTime);
			this.groupBox2.Controls.Add(this.radioButtonStartTime);
			this.groupBox2.Controls.Add(this.radioButtonAgentName);
			this.groupBox2.Location = new System.Drawing.Point(3, 148);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(427, 108);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "xxSortOrder";
			// 
			// radioButtonEndTime
			// 
			this.radioButtonEndTime.BeforeTouchSize = new System.Drawing.Size(90, 20);
			this.radioButtonEndTime.DrawFocusRectangle = false;
			this.radioButtonEndTime.Location = new System.Drawing.Point(7, 75);
			this.radioButtonEndTime.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonEndTime.Name = "radioButtonEndTime";
			this.radioButtonEndTime.Size = new System.Drawing.Size(90, 20);
			this.radioButtonEndTime.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonEndTime.TabIndex = 2;
			this.radioButtonEndTime.TabStop = false;
			this.radioButtonEndTime.Text = "xxEndTime";
			this.radioButtonEndTime.ThemesEnabled = false;
			this.radioButtonEndTime.CheckChanged += new System.EventHandler(this.radioButtonEndTimeCheckedChanged);
			// 
			// radioButtonStartTime
			// 
			this.radioButtonStartTime.BeforeTouchSize = new System.Drawing.Size(93, 20);
			this.radioButtonStartTime.Checked = true;
			this.radioButtonStartTime.DrawFocusRectangle = false;
			this.radioButtonStartTime.Location = new System.Drawing.Point(7, 48);
			this.radioButtonStartTime.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonStartTime.Name = "radioButtonStartTime";
			this.radioButtonStartTime.Size = new System.Drawing.Size(93, 20);
			this.radioButtonStartTime.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonStartTime.TabIndex = 1;
			this.radioButtonStartTime.Text = "xxStartTime";
			this.radioButtonStartTime.ThemesEnabled = false;
			this.radioButtonStartTime.CheckChanged += new System.EventHandler(this.radioButtonStartTimeCheckedChanged);
			// 
			// radioButtonAgentName
			// 
			this.radioButtonAgentName.BeforeTouchSize = new System.Drawing.Size(106, 20);
			this.radioButtonAgentName.DrawFocusRectangle = false;
			this.radioButtonAgentName.Location = new System.Drawing.Point(7, 22);
			this.radioButtonAgentName.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAgentName.Name = "radioButtonAgentName";
			this.radioButtonAgentName.Size = new System.Drawing.Size(106, 20);
			this.radioButtonAgentName.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAgentName.TabIndex = 0;
			this.radioButtonAgentName.Text = "xxAgentName";
			this.radioButtonAgentName.ThemesEnabled = false;
			this.radioButtonAgentName.CheckChanged += new System.EventHandler(this.radioButtonAgentNameCheckedChanged);
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
			this.buttonAdvCancel.TabIndex = 5;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOk.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOk.BeforeTouchSize = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOk.IsBackStageButton = false;
			this.buttonAdvOk.Location = new System.Drawing.Point(234, 273);
			this.buttonAdvOk.Margin = new System.Windows.Forms.Padding(3, 3, 10, 10);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
			this.buttonAdvOk.Size = new System.Drawing.Size(87, 27);
			this.buttonAdvOk.TabIndex = 6;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.97153F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.02847F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
			this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOk, 1, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 145F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(438, 310);
			this.tableLayoutPanel1.TabIndex = 7;
			// 
			// ScheduleReportDialogGraphicalView
			// 
			this.AcceptButton = this.buttonAdvOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderColor = System.Drawing.Color.Blue;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionFont = new System.Drawing.Font("Segoe UI", 12F);
			this.ClientSize = new System.Drawing.Size(438, 310);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(270, 40);
			this.Name = "ScheduleReportDialogGraphicalView";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxExportToPDFGraphical";
			this.Load += new System.EventHandler(this.scheduleReportDialogGraphicalViewLoad);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.checkBoxPublicNote)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxSingleFile)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonIndividual)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonTeam)).EndInit();
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.radioButtonEndTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonStartTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAgentName)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxSingleFile;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonIndividual;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonTeam;
		private System.Windows.Forms.GroupBox groupBox2;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonEndTime;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonStartTime;
		private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAgentName;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		  private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxPublicNote;
	}
}