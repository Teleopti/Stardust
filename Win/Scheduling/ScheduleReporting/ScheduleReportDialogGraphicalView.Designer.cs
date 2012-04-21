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
            this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSingleFile = new System.Windows.Forms.CheckBox();
            this.radioButtonIndividual = new System.Windows.Forms.RadioButton();
            this.radioButtonTeam = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonEndTime = new System.Windows.Forms.RadioButton();
            this.radioButtonStartTime = new System.Windows.Forms.RadioButton();
            this.radioButtonAgentName = new System.Windows.Forms.RadioButton();
            this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
            this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxPublicNote = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonControlAdv1
            // 
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
            this.ribbonControlAdv1.Size = new System.Drawing.Size(382, 33);
            this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
            this.ribbonControlAdv1.TabIndex = 0;
            this.ribbonControlAdv1.Text = "ribbonControlAdv1";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox1, 3);
            this.groupBox1.Controls.Add(this.checkBoxPublicNote);
            this.groupBox1.Controls.Add(this.checkBoxSingleFile);
            this.groupBox1.Controls.Add(this.radioButtonIndividual);
            this.groupBox1.Controls.Add(this.radioButtonTeam);
            this.groupBox1.Location = new System.Drawing.Point(3, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(366, 106);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "xxType";
            // 
            // checkBoxSingleFile
            // 
            this.checkBoxSingleFile.AutoSize = true;
            this.checkBoxSingleFile.Checked = true;
            this.checkBoxSingleFile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSingleFile.Location = new System.Drawing.Point(37, 83);
            this.checkBoxSingleFile.Name = "checkBoxSingleFile";
            this.checkBoxSingleFile.Size = new System.Drawing.Size(81, 17);
            this.checkBoxSingleFile.TabIndex = 2;
            this.checkBoxSingleFile.Text = "xxSingleFile";
            this.checkBoxSingleFile.UseVisualStyleBackColor = true;
            this.checkBoxSingleFile.CheckedChanged += new System.EventHandler(this.CheckBoxSingleFileCheckedChanged);
            // 
            // radioButtonIndividual
            // 
            this.radioButtonIndividual.AutoSize = true;
            this.radioButtonIndividual.Checked = true;
            this.radioButtonIndividual.Location = new System.Drawing.Point(6, 60);
            this.radioButtonIndividual.Name = "radioButtonIndividual";
            this.radioButtonIndividual.Size = new System.Drawing.Size(80, 17);
            this.radioButtonIndividual.TabIndex = 1;
            this.radioButtonIndividual.TabStop = true;
            this.radioButtonIndividual.Text = "xxIndividual";
            this.radioButtonIndividual.UseVisualStyleBackColor = true;
            this.radioButtonIndividual.CheckedChanged += new System.EventHandler(this.RadioButtonIndividualCheckedChanged);
            // 
            // radioButtonTeam
            // 
            this.radioButtonTeam.AutoSize = true;
            this.radioButtonTeam.Location = new System.Drawing.Point(6, 19);
            this.radioButtonTeam.Name = "radioButtonTeam";
            this.radioButtonTeam.Size = new System.Drawing.Size(62, 17);
            this.radioButtonTeam.TabIndex = 0;
            this.radioButtonTeam.Text = "xxTeam";
            this.radioButtonTeam.UseVisualStyleBackColor = true;
            this.radioButtonTeam.CheckedChanged += new System.EventHandler(this.RadioButtonTeamCheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel1.SetColumnSpan(this.groupBox2, 3);
            this.groupBox2.Controls.Add(this.radioButtonEndTime);
            this.groupBox2.Controls.Add(this.radioButtonStartTime);
            this.groupBox2.Controls.Add(this.radioButtonAgentName);
            this.groupBox2.Location = new System.Drawing.Point(3, 154);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(366, 94);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "xxSortOrder";
            // 
            // radioButtonEndTime
            // 
            this.radioButtonEndTime.AutoSize = true;
            this.radioButtonEndTime.Location = new System.Drawing.Point(6, 65);
            this.radioButtonEndTime.Name = "radioButtonEndTime";
            this.radioButtonEndTime.Size = new System.Drawing.Size(77, 17);
            this.radioButtonEndTime.TabIndex = 2;
            this.radioButtonEndTime.Text = "xxEndTime";
            this.radioButtonEndTime.UseVisualStyleBackColor = true;
            this.radioButtonEndTime.CheckedChanged += new System.EventHandler(this.RadioButtonEndTimeCheckedChanged);
            // 
            // radioButtonStartTime
            // 
            this.radioButtonStartTime.AutoSize = true;
            this.radioButtonStartTime.Checked = true;
            this.radioButtonStartTime.Location = new System.Drawing.Point(6, 42);
            this.radioButtonStartTime.Name = "radioButtonStartTime";
            this.radioButtonStartTime.Size = new System.Drawing.Size(80, 17);
            this.radioButtonStartTime.TabIndex = 1;
            this.radioButtonStartTime.TabStop = true;
            this.radioButtonStartTime.Text = "xxStartTime";
            this.radioButtonStartTime.UseVisualStyleBackColor = true;
            this.radioButtonStartTime.CheckedChanged += new System.EventHandler(this.RadioButtonStartTimeCheckedChanged);
            // 
            // radioButtonAgentName
            // 
            this.radioButtonAgentName.AutoSize = true;
            this.radioButtonAgentName.Location = new System.Drawing.Point(6, 19);
            this.radioButtonAgentName.Name = "radioButtonAgentName";
            this.radioButtonAgentName.Size = new System.Drawing.Size(91, 17);
            this.radioButtonAgentName.TabIndex = 0;
            this.radioButtonAgentName.Text = "xxAgentName";
            this.radioButtonAgentName.UseVisualStyleBackColor = true;
            this.radioButtonAgentName.CheckedChanged += new System.EventHandler(this.RadioButtonAgentNameCheckedChanged);
            // 
            // buttonAdvCancel
            // 
            this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonAdvCancel.Location = new System.Drawing.Point(284, 271);
            this.buttonAdvCancel.Name = "buttonAdvCancel";
            this.buttonAdvCancel.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvCancel.TabIndex = 5;
            this.buttonAdvCancel.Text = "xxCancel";
            this.buttonAdvCancel.UseVisualStyle = true;
            this.buttonAdvCancel.Click += new System.EventHandler(this.ButtonAdvCancelClick);
            // 
            // buttonAdvOk
            // 
            this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
            this.buttonAdvOk.Location = new System.Drawing.Point(194, 271);
            this.buttonAdvOk.Name = "buttonAdvOk";
            this.buttonAdvOk.Office2007ColorScheme = Syncfusion.Windows.Forms.Office2007Theme.Managed;
            this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvOk.TabIndex = 6;
            this.buttonAdvOk.Text = "xxOk";
            this.buttonAdvOk.UseVisualStyle = true;
            this.buttonAdvOk.Click += new System.EventHandler(this.ButtonAdvOkClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.97153F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.02847F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonAdvOk, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(372, 306);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // checkBoxPublicNote
            // 
            this.checkBoxPublicNote.AutoSize = true;
            this.checkBoxPublicNote.Checked = true;
            this.checkBoxPublicNote.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPublicNote.Location = new System.Drawing.Point(37, 42);
            this.checkBoxPublicNote.Name = "checkBoxPublicNote";
            this.checkBoxPublicNote.Size = new System.Drawing.Size(115, 17);
            this.checkBoxPublicNote.TabIndex = 3;
            this.checkBoxPublicNote.Text = "xxShowPublicNote";
            this.checkBoxPublicNote.UseVisualStyleBackColor = true;
            this.checkBoxPublicNote.CheckedChanged += new System.EventHandler(this.CheckBoxPublicNoteCheckedChanged);
            // 
            // ScheduleReportDialogGraphicalView
            // 
            this.AcceptButton = this.buttonAdvOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonAdvCancel;
            this.ClientSize = new System.Drawing.Size(384, 346);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ribbonControlAdv1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScheduleReportDialogGraphicalView";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xxExportToPDFGraphical";
            this.Load += new System.EventHandler(this.ScheduleReportDialogGraphicalViewLoad);
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBoxSingleFile;
		private System.Windows.Forms.RadioButton radioButtonIndividual;
		private System.Windows.Forms.RadioButton radioButtonTeam;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton radioButtonEndTime;
		private System.Windows.Forms.RadioButton radioButtonStartTime;
		private System.Windows.Forms.RadioButton radioButtonAgentName;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxPublicNote;
	}
}