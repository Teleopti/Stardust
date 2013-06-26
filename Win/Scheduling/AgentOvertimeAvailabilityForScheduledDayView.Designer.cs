namespace Teleopti.Ccc.Win.Scheduling
{
	partial class AgentOvertimeAvailabilityForScheduledDayView
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNextDay"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOvertimeAvailability"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxCancel"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxOk"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFromColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxToColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.Parse(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxNextDayColon"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAgentOvertimeAvailabilityView"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.Control.set_Text(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1302:DoNotHardcodeLocaleSpecificStrings", MessageId = "Start menu"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MinimizeToolTip(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.Tools.RibbonControlAdv.set_MaximizeToolTip(System.String)")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.outlookTimePickerFrom = new Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker();
			this.label1 = new System.Windows.Forms.Label();
			this.labelFrom = new System.Windows.Forms.Label();
			this.checkBoxAdvNextDay = new Syncfusion.Windows.Forms.Tools.CheckBoxAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOk = new Syncfusion.Windows.Forms.ButtonAdv();
			this.labelTo = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.outlookTimePickerTo = new Teleopti.Ccc.Win.Common.Controls.OutlookTimePicker();
			this.labelPreviousSavedOvertimeAvailabilityColon = new System.Windows.Forms.Label();
			this.labelPreviousSavedOvertimeAvailability = new System.Windows.Forms.Label();
			this.labelShiftStartsAt = new System.Windows.Forms.Label();
			this.labelShiftEndsAt = new System.Windows.Forms.Label();
			this.ribbonControlAdv1 = new Syncfusion.Windows.Forms.Tools.RibbonControlAdv();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerFrom)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvNextDay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerTo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 6;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerFrom, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 3, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelFrom, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxAdvNextDay, 5, 2);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvCancel, 5, 4);
			this.tableLayoutPanel1.Controls.Add(this.buttonAdvOk, 4, 4);
			this.tableLayoutPanel1.Controls.Add(this.labelTo, 3, 2);
			this.tableLayoutPanel1.Controls.Add(this.label2, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerTo, 4, 2);
			this.tableLayoutPanel1.Controls.Add(this.labelPreviousSavedOvertimeAvailabilityColon, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.labelPreviousSavedOvertimeAvailability, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.labelShiftStartsAt, 4, 1);
			this.tableLayoutPanel1.Controls.Add(this.labelShiftEndsAt, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 34);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(511, 145);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// outlookTimePickerFrom
			// 
			this.outlookTimePickerFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerFrom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.outlookTimePickerFrom.DefaultResolution = 0;
			this.outlookTimePickerFrom.EnableNull = true;
			this.outlookTimePickerFrom.FormatFromCulture = true;
			this.outlookTimePickerFrom.Location = new System.Drawing.Point(124, 22);
			this.outlookTimePickerFrom.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerFrom.Name = "outlookTimePickerFrom";
			this.outlookTimePickerFrom.Size = new System.Drawing.Size(74, 21);
			this.outlookTimePickerFrom.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.outlookTimePickerFrom.TabIndex = 1;
			this.outlookTimePickerFrom.Text = "00:00";
			this.outlookTimePickerFrom.TextChanged += new System.EventHandler(this.outlookTimePickerFromTextChanged);
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(220, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(103, 26);
			this.label1.TabIndex = 6;
			this.label1.Text = "xxToShiftStartsAtColon";
			// 
			// labelFrom
			// 
			this.labelFrom.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelFrom.AutoSize = true;
			this.labelFrom.Location = new System.Drawing.Point(18, 26);
			this.labelFrom.Name = "labelFrom";
			this.labelFrom.Size = new System.Drawing.Size(67, 13);
			this.labelFrom.TabIndex = 2;
			this.labelFrom.Text = "xxFromColon";
			// 
			// checkBoxAdvNextDay
			// 
			this.checkBoxAdvNextDay.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxAdvNextDay.Location = new System.Drawing.Point(424, 57);
			this.checkBoxAdvNextDay.Name = "checkBoxAdvNextDay";
			this.checkBoxAdvNextDay.Size = new System.Drawing.Size(82, 21);
			this.checkBoxAdvNextDay.Style = Syncfusion.Windows.Forms.Tools.CheckBoxAdvStyle.Office2007;
			this.checkBoxAdvNextDay.TabIndex = 3;
			this.checkBoxAdvNextDay.Text = "xxNextDay";
			this.checkBoxAdvNextDay.ThemesEnabled = true;
			this.checkBoxAdvNextDay.CheckedChanged += new Syncfusion.Windows.Forms.Tools.CheckedChangedEventHandler(this.checkBoxAdvNextDayCheckedChanged);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvCancel.Location = new System.Drawing.Point(424, 113);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvCancel.TabIndex = 5;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			this.buttonAdvCancel.Click += new System.EventHandler(this.buttonAdvCancelClick);
			// 
			// buttonAdvOk
			// 
			this.buttonAdvOk.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.buttonAdvOk.Location = new System.Drawing.Point(332, 113);
			this.buttonAdvOk.Name = "buttonAdvOk";
			this.buttonAdvOk.Size = new System.Drawing.Size(75, 23);
			this.buttonAdvOk.TabIndex = 4;
			this.buttonAdvOk.Text = "xxOk";
			this.buttonAdvOk.UseVisualStyle = true;
			this.buttonAdvOk.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// labelTo
			// 
			this.labelTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelTo.AutoSize = true;
			this.labelTo.Location = new System.Drawing.Point(220, 61);
			this.labelTo.Name = "labelTo";
			this.labelTo.Size = new System.Drawing.Size(57, 13);
			this.labelTo.TabIndex = 3;
			this.labelTo.Text = "xxToColon";
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(18, 54);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(95, 26);
			this.label2.TabIndex = 8;
			this.label2.Text = "xxFromShiftEndsAtColon";
			// 
			// outlookTimePickerTo
			// 
			this.outlookTimePickerTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.outlookTimePickerTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.outlookTimePickerTo.DefaultResolution = 0;
			this.outlookTimePickerTo.EnableNull = true;
			this.outlookTimePickerTo.FormatFromCulture = true;
			this.outlookTimePickerTo.Location = new System.Drawing.Point(332, 57);
			this.outlookTimePickerTo.MaxTime = System.TimeSpan.Parse("23:59:00");
			this.outlookTimePickerTo.Name = "outlookTimePickerTo";
			this.outlookTimePickerTo.Size = new System.Drawing.Size(74, 21);
			this.outlookTimePickerTo.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.outlookTimePickerTo.TabIndex = 9;
			this.outlookTimePickerTo.Text = "00:00";
			// 
			// labelPreviousSavedOvertimeAvailabilityColon
			// 
			this.labelPreviousSavedOvertimeAvailabilityColon.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelPreviousSavedOvertimeAvailabilityColon.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.labelPreviousSavedOvertimeAvailabilityColon, 3);
			this.labelPreviousSavedOvertimeAvailabilityColon.Location = new System.Drawing.Point(18, 91);
			this.labelPreviousSavedOvertimeAvailabilityColon.Name = "labelPreviousSavedOvertimeAvailabilityColon";
			this.labelPreviousSavedOvertimeAvailabilityColon.Size = new System.Drawing.Size(207, 13);
			this.labelPreviousSavedOvertimeAvailabilityColon.TabIndex = 10;
			this.labelPreviousSavedOvertimeAvailabilityColon.Text = "xxPreviousSavedOvertimeAvailabilityColon";
			this.labelPreviousSavedOvertimeAvailabilityColon.Visible = false;
			// 
			// labelPreviousSavedOvertimeAvailability
			// 
			this.labelPreviousSavedOvertimeAvailability.AutoSize = true;
			this.labelPreviousSavedOvertimeAvailability.Location = new System.Drawing.Point(18, 110);
			this.labelPreviousSavedOvertimeAvailability.Name = "labelPreviousSavedOvertimeAvailability";
			this.labelPreviousSavedOvertimeAvailability.Size = new System.Drawing.Size(0, 13);
			this.labelPreviousSavedOvertimeAvailability.TabIndex = 11;
			// 
			// labelShiftStartsAt
			// 
			this.labelShiftStartsAt.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelShiftStartsAt.AutoSize = true;
			this.labelShiftStartsAt.Location = new System.Drawing.Point(332, 26);
			this.labelShiftStartsAt.Name = "labelShiftStartsAt";
			this.labelShiftStartsAt.Size = new System.Drawing.Size(35, 13);
			this.labelShiftStartsAt.TabIndex = 12;
			this.labelShiftStartsAt.Text = "label3";
			// 
			// labelShiftEndsAt
			// 
			this.labelShiftEndsAt.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.labelShiftEndsAt.AutoSize = true;
			this.labelShiftEndsAt.Location = new System.Drawing.Point(124, 61);
			this.labelShiftEndsAt.Name = "labelShiftEndsAt";
			this.labelShiftEndsAt.Size = new System.Drawing.Size(35, 13);
			this.labelShiftEndsAt.TabIndex = 13;
			this.labelShiftEndsAt.Text = "label3";
			// 
			// ribbonControlAdv1
			// 
			this.ribbonControlAdv1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ribbonControlAdv1.Location = new System.Drawing.Point(1, 0);
			this.ribbonControlAdv1.MaximizeToolTip = "Maximize Ribbon";
			this.ribbonControlAdv1.MenuButtonText = "";
			this.ribbonControlAdv1.MenuButtonVisible = false;
			this.ribbonControlAdv1.MinimizeToolTip = "Minimize Ribbon";
			this.ribbonControlAdv1.Name = "ribbonControlAdv1";
			// 
			// ribbonControlAdv1.OfficeMenu
			// 
			this.ribbonControlAdv1.OfficeMenu.Name = "OfficeMenu";
			this.ribbonControlAdv1.OfficeMenu.ShowItemToolTips = true;
			this.ribbonControlAdv1.OfficeMenu.Size = new System.Drawing.Size(12, 65);
			this.ribbonControlAdv1.QuickPanelVisible = false;
			this.ribbonControlAdv1.SelectedTab = null;
			this.ribbonControlAdv1.Size = new System.Drawing.Size(521, 33);
			this.ribbonControlAdv1.SystemText.QuickAccessDialogDropDownName = "Start menu";
			this.ribbonControlAdv1.TabIndex = 0;
			this.ribbonControlAdv1.Text = "ribbonControlAdv1";
			// 
			// errorProvider1
			// 
			this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider1.ContainerControl = this;
			// 
			// AgentOvertimeAvailabilityForScheduledDayView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(523, 185);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.ribbonControlAdv1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AgentOvertimeAvailabilityForScheduledDayView";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxOvertimeAvailability";
			this.Load += new System.EventHandler(this.agentOvertimeAvailabilityViewLoad);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerFrom)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.checkBoxAdvNextDay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerTo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ribbonControlAdv1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Syncfusion.Windows.Forms.Tools.RibbonControlAdv ribbonControlAdv1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOk;
		private System.Windows.Forms.Label labelFrom;
		private System.Windows.Forms.Label labelTo;
		private Syncfusion.Windows.Forms.Tools.CheckBoxAdv checkBoxAdvNextDay;
		private Common.Controls.OutlookTimePicker outlookTimePickerFrom;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Common.Controls.OutlookTimePicker outlookTimePickerTo;
		private System.Windows.Forms.Label labelPreviousSavedOvertimeAvailabilityColon;
		private System.Windows.Forms.Label labelPreviousSavedOvertimeAvailability;
		private System.Windows.Forms.Label labelShiftStartsAt;
		private System.Windows.Forms.Label labelShiftEndsAt;
	}
}