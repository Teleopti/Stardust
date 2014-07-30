using System;
using System.ComponentModel;

namespace Teleopti.Ccc.Win.Meetings
{
    partial class MeetingRecurrenceView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MeetingRecurrenceView));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.groupBoxAppointmentTime = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.outlookTimePickerEnd = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.labelStart = new System.Windows.Forms.Label();
			this.labelEnd = new System.Windows.Forms.Label();
			this.outlookTimePickerStart = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.groupBoxRecurrencePattern = new System.Windows.Forms.GroupBox();
			this.splitContainerAdv1 = new Syncfusion.Windows.Forms.Tools.SplitContainerAdv();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.radioButtonAdvMonthlyRecurring = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAdvDailyRecurring = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.radioButtonAdvWeeklyRecurring = new Syncfusion.Windows.Forms.Tools.RadioButtonAdv();
			this.groupBoxRangeOfRecurrence = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
			this.labelRangeStart = new System.Windows.Forms.Label();
			this.dateTimePickerAdvStart = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.labelEndBy = new System.Windows.Forms.Label();
			this.dateTimePickerAdvEnd = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
			this.buttonAdvRemove = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvCancel = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvOK = new Syncfusion.Windows.Forms.ButtonAdv();
			this.tableLayoutPanel1.SuspendLayout();
			this.groupBoxAppointmentTime.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEnd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStart)).BeginInit();
			this.groupBoxRecurrencePattern.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).BeginInit();
			this.splitContainerAdv1.Panel1.SuspendLayout();
			this.splitContainerAdv1.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvMonthlyRecurring)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvDailyRecurring)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvWeeklyRecurring)).BeginInit();
			this.groupBoxRangeOfRecurrence.SuspendLayout();
			this.tableLayoutPanel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStart.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEnd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEnd.Calendar)).BeginInit();
			this.tableLayoutPanel5.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.groupBoxAppointmentTime, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.groupBoxRecurrencePattern, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.groupBoxRangeOfRecurrence, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 3);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 4;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 125F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(534, 362);
			this.tableLayoutPanel1.TabIndex = 8;
			// 
			// groupBoxAppointmentTime
			// 
			this.groupBoxAppointmentTime.BackColor = System.Drawing.Color.Transparent;
			this.groupBoxAppointmentTime.Controls.Add(this.tableLayoutPanel2);
			this.groupBoxAppointmentTime.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxAppointmentTime.Location = new System.Drawing.Point(10, 3);
			this.groupBoxAppointmentTime.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.groupBoxAppointmentTime.Name = "groupBoxAppointmentTime";
			this.groupBoxAppointmentTime.Size = new System.Drawing.Size(514, 84);
			this.groupBoxAppointmentTime.TabIndex = 0;
			this.groupBoxAppointmentTime.TabStop = false;
			this.groupBoxAppointmentTime.Text = "xxApointmentTime";
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.outlookTimePickerEnd, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.labelStart, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.labelEnd, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.outlookTimePickerStart, 1, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 18);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(508, 63);
			this.tableLayoutPanel2.TabIndex = 0;
			// 
			// outlookTimePickerEnd
			// 
			this.outlookTimePickerEnd.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerEnd.BeforeTouchSize = new System.Drawing.Size(175, 21);
			this.outlookTimePickerEnd.BindableTimeValue = System.TimeSpan.Parse("02:00:00");
			this.outlookTimePickerEnd.Location = new System.Drawing.Point(123, 33);
			this.outlookTimePickerEnd.MaxDropDownItems = 16;
			this.outlookTimePickerEnd.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerEnd.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEnd.Name = "outlookTimePickerEnd";
			this.outlookTimePickerEnd.Size = new System.Drawing.Size(175, 21);
			this.outlookTimePickerEnd.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerEnd.TabIndex = 2;
			this.outlookTimePickerEnd.Text = "02:00";
			// 
			// labelStart
			// 
			this.labelStart.AutoSize = true;
			this.labelStart.Location = new System.Drawing.Point(3, 8);
			this.labelStart.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.labelStart.Name = "labelStart";
			this.labelStart.Size = new System.Drawing.Size(72, 13);
			this.labelStart.TabIndex = 0;
			this.labelStart.Text = "xxStartColon";
			this.labelStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelEnd
			// 
			this.labelEnd.AutoSize = true;
			this.labelEnd.Location = new System.Drawing.Point(3, 38);
			this.labelEnd.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
			this.labelEnd.Name = "labelEnd";
			this.labelEnd.Size = new System.Drawing.Size(68, 13);
			this.labelEnd.TabIndex = 1;
			this.labelEnd.Text = "xxEndColon";
			this.labelEnd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// outlookTimePickerStart
			// 
			this.outlookTimePickerStart.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerStart.BeforeTouchSize = new System.Drawing.Size(175, 21);
			this.outlookTimePickerStart.BindableTimeValue = System.TimeSpan.Parse("01:00:00");
			this.outlookTimePickerStart.Location = new System.Drawing.Point(123, 3);
			this.outlookTimePickerStart.MaxDropDownItems = 16;
			this.outlookTimePickerStart.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerStart.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.outlookTimePickerStart.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStart.Name = "outlookTimePickerStart";
			this.outlookTimePickerStart.Size = new System.Drawing.Size(175, 21);
			this.outlookTimePickerStart.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerStart.TabIndex = 1;
			this.outlookTimePickerStart.Text = "01:00";
			// 
			// groupBoxRecurrencePattern
			// 
			this.groupBoxRecurrencePattern.BackColor = System.Drawing.Color.Transparent;
			this.groupBoxRecurrencePattern.Controls.Add(this.splitContainerAdv1);
			this.groupBoxRecurrencePattern.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxRecurrencePattern.Location = new System.Drawing.Point(10, 93);
			this.groupBoxRecurrencePattern.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.groupBoxRecurrencePattern.Name = "groupBoxRecurrencePattern";
			this.groupBoxRecurrencePattern.Size = new System.Drawing.Size(514, 119);
			this.groupBoxRecurrencePattern.TabIndex = 0;
			this.groupBoxRecurrencePattern.TabStop = false;
			this.groupBoxRecurrencePattern.Text = "xxRecurrencePattern";
			// 
			// splitContainerAdv1
			// 
			this.splitContainerAdv1.BackColor = System.Drawing.Color.Transparent;
			this.splitContainerAdv1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(System.Drawing.Color.Transparent);
			this.splitContainerAdv1.BeforeTouchSize = 1;
			this.splitContainerAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerAdv1.IsSplitterFixed = true;
			this.splitContainerAdv1.Location = new System.Drawing.Point(3, 18);
			this.splitContainerAdv1.Name = "splitContainerAdv1";
			// 
			// splitContainerAdv1.Panel1
			// 
			this.splitContainerAdv1.Panel1.BackColor = System.Drawing.Color.Transparent;
			this.splitContainerAdv1.Panel1.Controls.Add(this.tableLayoutPanel3);
			// 
			// splitContainerAdv1.Panel2
			// 
			this.splitContainerAdv1.Panel2.BackColor = System.Drawing.Color.Transparent;
			this.splitContainerAdv1.Size = new System.Drawing.Size(508, 98);
			this.splitContainerAdv1.SplitterDistance = 127;
			this.splitContainerAdv1.SplitterWidth = 1;
			this.splitContainerAdv1.TabIndex = 0;
			this.splitContainerAdv1.Text = "yysplitContainerAdv1";
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 1;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel3.Controls.Add(this.radioButtonAdvMonthlyRecurring, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.radioButtonAdvDailyRecurring, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.radioButtonAdvWeeklyRecurring, 0, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 3;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(127, 98);
			this.tableLayoutPanel3.TabIndex = 0;
			// 
			// radioButtonAdvMonthlyRecurring
			// 
			this.radioButtonAdvMonthlyRecurring.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.radioButtonAdvMonthlyRecurring.CheckedInt = 2;
			this.radioButtonAdvMonthlyRecurring.DrawFocusRectangle = false;
			this.radioButtonAdvMonthlyRecurring.Location = new System.Drawing.Point(3, 67);
			this.radioButtonAdvMonthlyRecurring.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvMonthlyRecurring.Name = "radioButtonAdvMonthlyRecurring";
			this.radioButtonAdvMonthlyRecurring.Size = new System.Drawing.Size(114, 19);
			this.radioButtonAdvMonthlyRecurring.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAdvMonthlyRecurring.TabIndex = 1;
			this.radioButtonAdvMonthlyRecurring.Text = "xxMonthly";
			this.radioButtonAdvMonthlyRecurring.ThemesEnabled = false;
			this.radioButtonAdvMonthlyRecurring.CheckChanged += new System.EventHandler(this.radioButtonAdvRecurringTypeCheckChanged);
			// 
			// radioButtonAdvDailyRecurring
			// 
			this.radioButtonAdvDailyRecurring.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.radioButtonAdvDailyRecurring.CheckedInt = 0;
			this.radioButtonAdvDailyRecurring.DrawFocusRectangle = false;
			this.radioButtonAdvDailyRecurring.Location = new System.Drawing.Point(3, 3);
			this.radioButtonAdvDailyRecurring.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvDailyRecurring.Name = "radioButtonAdvDailyRecurring";
			this.radioButtonAdvDailyRecurring.Size = new System.Drawing.Size(114, 18);
			this.radioButtonAdvDailyRecurring.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAdvDailyRecurring.TabIndex = 3;
			this.radioButtonAdvDailyRecurring.Text = "xxDaily";
			this.radioButtonAdvDailyRecurring.ThemesEnabled = false;
			this.radioButtonAdvDailyRecurring.CheckChanged += new System.EventHandler(this.radioButtonAdvRecurringTypeCheckChanged);
			// 
			// radioButtonAdvWeeklyRecurring
			// 
			this.radioButtonAdvWeeklyRecurring.BeforeTouchSize = new System.Drawing.Size(150, 21);
			this.radioButtonAdvWeeklyRecurring.DrawFocusRectangle = false;
			this.radioButtonAdvWeeklyRecurring.Location = new System.Drawing.Point(3, 35);
			this.radioButtonAdvWeeklyRecurring.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(89)))), ((int)(((byte)(91)))));
			this.radioButtonAdvWeeklyRecurring.Name = "radioButtonAdvWeeklyRecurring";
			this.radioButtonAdvWeeklyRecurring.Size = new System.Drawing.Size(114, 18);
			this.radioButtonAdvWeeklyRecurring.Style = Syncfusion.Windows.Forms.Tools.RadioButtonAdvStyle.Metro;
			this.radioButtonAdvWeeklyRecurring.TabIndex = 2;
			this.radioButtonAdvWeeklyRecurring.Text = "xxWeekly";
			this.radioButtonAdvWeeklyRecurring.ThemesEnabled = false;
			this.radioButtonAdvWeeklyRecurring.CheckChanged += new System.EventHandler(this.radioButtonAdvRecurringTypeCheckChanged);
			// 
			// groupBoxRangeOfRecurrence
			// 
			this.groupBoxRangeOfRecurrence.Controls.Add(this.tableLayoutPanel4);
			this.groupBoxRangeOfRecurrence.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxRangeOfRecurrence.Location = new System.Drawing.Point(10, 218);
			this.groupBoxRangeOfRecurrence.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
			this.groupBoxRangeOfRecurrence.Name = "groupBoxRangeOfRecurrence";
			this.groupBoxRangeOfRecurrence.Size = new System.Drawing.Size(514, 94);
			this.groupBoxRangeOfRecurrence.TabIndex = 0;
			this.groupBoxRangeOfRecurrence.TabStop = false;
			this.groupBoxRangeOfRecurrence.Text = "xxRangeOfRecurrence";
			// 
			// tableLayoutPanel4
			// 
			this.tableLayoutPanel4.ColumnCount = 4;
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
			this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.tableLayoutPanel4.Controls.Add(this.labelRangeStart, 0, 0);
			this.tableLayoutPanel4.Controls.Add(this.dateTimePickerAdvStart, 1, 0);
			this.tableLayoutPanel4.Controls.Add(this.labelEndBy, 2, 0);
			this.tableLayoutPanel4.Controls.Add(this.dateTimePickerAdvEnd, 3, 0);
			this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 18);
			this.tableLayoutPanel4.Name = "tableLayoutPanel4";
			this.tableLayoutPanel4.RowCount = 1;
			this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel4.Size = new System.Drawing.Size(508, 73);
			this.tableLayoutPanel4.TabIndex = 0;
			// 
			// labelRangeStart
			// 
			this.labelRangeStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.labelRangeStart.AutoSize = true;
			this.labelRangeStart.Location = new System.Drawing.Point(3, 0);
			this.labelRangeStart.Name = "labelRangeStart";
			this.labelRangeStart.Size = new System.Drawing.Size(65, 73);
			this.labelRangeStart.TabIndex = 1;
			this.labelRangeStart.Text = "xxStartColon";
			this.labelRangeStart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// dateTimePickerAdvStart
			// 
			this.dateTimePickerAdvStart.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.dateTimePickerAdvStart.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvStart.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// 
			// 
			this.dateTimePickerAdvStart.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvStart.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvStart.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvStart.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvStart.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStart.Calendar.DayNamesColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvStart.Calendar.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvStart.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStart.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvStart.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStart.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStart.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvStart.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStart.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStart.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStart.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStart.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvStart.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStart.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvStart.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvStart.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvStart.Calendar.Size = new System.Drawing.Size(90, 174);
			this.dateTimePickerAdvStart.Calendar.SizeToFit = true;
			this.dateTimePickerAdvStart.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStart.Calendar.TabIndex = 0;
			this.dateTimePickerAdvStart.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStart.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvStart.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStart.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStart.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStart.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvStart.Calendar.NoneButton.Location = new System.Drawing.Point(137, 0);
			this.dateTimePickerAdvStart.Calendar.NoneButton.Size = new System.Drawing.Size(72, 25);
			this.dateTimePickerAdvStart.Calendar.NoneButton.Text = "xxNone";
			this.dateTimePickerAdvStart.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvStart.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvStart.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStart.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStart.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStart.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvStart.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStart.Calendar.TodayButton.Size = new System.Drawing.Size(90, 25);
			this.dateTimePickerAdvStart.Calendar.TodayButton.Text = "xxToday";
			this.dateTimePickerAdvStart.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvStart.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStart.CalendarForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStart.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvStart.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStart.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStart.DropDownImage = null;
			this.dateTimePickerAdvStart.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.EnableNullDate = false;
			this.dateTimePickerAdvStart.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvStart.Location = new System.Drawing.Point(79, 26);
			this.dateTimePickerAdvStart.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStart.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvStart.Name = "dateTimePickerAdvStart";
			this.dateTimePickerAdvStart.NoneButtonVisible = false;
			this.dateTimePickerAdvStart.ShowCheckBox = false;
			this.dateTimePickerAdvStart.Size = new System.Drawing.Size(94, 20);
			this.dateTimePickerAdvStart.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStart.TabIndex = 10;
			this.dateTimePickerAdvStart.ThemesEnabled = true;
			this.dateTimePickerAdvStart.Value = new System.DateTime(2008, 12, 5, 6, 42, 38, 484);
			// 
			// labelEndBy
			// 
			this.labelEndBy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.labelEndBy.AutoSize = true;
			this.labelEndBy.Location = new System.Drawing.Point(256, 0);
			this.labelEndBy.Name = "labelEndBy";
			this.labelEndBy.Size = new System.Drawing.Size(66, 73);
			this.labelEndBy.TabIndex = 3;
			this.labelEndBy.Text = "xxEndByColon";
			this.labelEndBy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// dateTimePickerAdvEnd
			// 
			this.dateTimePickerAdvEnd.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.dateTimePickerAdvEnd.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvEnd.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// 
			// 
			this.dateTimePickerAdvEnd.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvEnd.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvEnd.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvEnd.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvEnd.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEnd.Calendar.DayNamesColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvEnd.Calendar.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvEnd.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEnd.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvEnd.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEnd.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEnd.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvEnd.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEnd.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEnd.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEnd.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEnd.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvEnd.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEnd.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvEnd.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvEnd.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvEnd.Calendar.Size = new System.Drawing.Size(84, 174);
			this.dateTimePickerAdvEnd.Calendar.SizeToFit = true;
			this.dateTimePickerAdvEnd.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEnd.Calendar.TabIndex = 0;
			this.dateTimePickerAdvEnd.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEnd.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvEnd.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEnd.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEnd.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEnd.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvEnd.Calendar.NoneButton.Location = new System.Drawing.Point(137, 0);
			this.dateTimePickerAdvEnd.Calendar.NoneButton.Size = new System.Drawing.Size(72, 25);
			this.dateTimePickerAdvEnd.Calendar.NoneButton.Text = "xxNone";
			this.dateTimePickerAdvEnd.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvEnd.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvEnd.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEnd.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEnd.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEnd.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvEnd.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEnd.Calendar.TodayButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvEnd.Calendar.TodayButton.Text = "xxToday";
			this.dateTimePickerAdvEnd.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvEnd.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEnd.CalendarForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEnd.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvEnd.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEnd.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEnd.DropDownImage = null;
			this.dateTimePickerAdvEnd.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.EnableNullDate = false;
			this.dateTimePickerAdvEnd.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvEnd.Location = new System.Drawing.Point(332, 26);
			this.dateTimePickerAdvEnd.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEnd.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvEnd.Name = "dateTimePickerAdvEnd";
			this.dateTimePickerAdvEnd.NoneButtonVisible = false;
			this.dateTimePickerAdvEnd.ShowCheckBox = false;
			this.dateTimePickerAdvEnd.Size = new System.Drawing.Size(88, 20);
			this.dateTimePickerAdvEnd.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEnd.TabIndex = 11;
			this.dateTimePickerAdvEnd.ThemesEnabled = true;
			this.dateTimePickerAdvEnd.Value = new System.DateTime(2008, 12, 5, 6, 42, 0, 0);
			// 
			// tableLayoutPanel5
			// 
			this.tableLayoutPanel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel5.ColumnCount = 3;
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
			this.tableLayoutPanel5.Controls.Add(this.buttonAdvRemove, 0, 0);
			this.tableLayoutPanel5.Controls.Add(this.buttonAdvCancel, 1, 0);
			this.tableLayoutPanel5.Controls.Add(this.buttonAdvOK, 1, 0);
			this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 318);
			this.tableLayoutPanel5.Name = "tableLayoutPanel5";
			this.tableLayoutPanel5.RowCount = 1;
			this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel5.Size = new System.Drawing.Size(528, 44);
			this.tableLayoutPanel5.TabIndex = 3;
			// 
			// buttonAdvRemove
			// 
			this.buttonAdvRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonAdvRemove.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvRemove.BeforeTouchSize = new System.Drawing.Size(201, 23);
			this.buttonAdvRemove.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAdvRemove.Enabled = false;
			this.buttonAdvRemove.ForeColor = System.Drawing.Color.White;
			this.buttonAdvRemove.IsBackStageButton = false;
			this.buttonAdvRemove.Location = new System.Drawing.Point(10, 10);
			this.buttonAdvRemove.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
			this.buttonAdvRemove.Name = "buttonAdvRemove";
			this.buttonAdvRemove.Size = new System.Drawing.Size(201, 23);
			this.buttonAdvRemove.TabIndex = 14;
			this.buttonAdvRemove.Text = "xxRemoveRecurrence";
			this.buttonAdvRemove.UseVisualStyle = true;
			this.buttonAdvRemove.Click += new System.EventHandler(this.buttonAdvRemoveClick);
			// 
			// buttonAdvCancel
			// 
			this.buttonAdvCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonAdvCancel.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvCancel.BeforeTouchSize = new System.Drawing.Size(82, 23);
			this.buttonAdvCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonAdvCancel.ForeColor = System.Drawing.Color.White;
			this.buttonAdvCancel.IsBackStageButton = false;
			this.buttonAdvCancel.Location = new System.Drawing.Point(437, 10);
			this.buttonAdvCancel.Name = "buttonAdvCancel";
			this.buttonAdvCancel.Size = new System.Drawing.Size(82, 23);
			this.buttonAdvCancel.TabIndex = 13;
			this.buttonAdvCancel.Text = "xxCancel";
			this.buttonAdvCancel.UseVisualStyle = true;
			// 
			// buttonAdvOK
			// 
			this.buttonAdvOK.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.buttonAdvOK.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvOK.BeforeTouchSize = new System.Drawing.Size(82, 23);
			this.buttonAdvOK.ForeColor = System.Drawing.Color.White;
			this.buttonAdvOK.IsBackStageButton = false;
			this.buttonAdvOK.Location = new System.Drawing.Point(343, 10);
			this.buttonAdvOK.Name = "buttonAdvOK";
			this.buttonAdvOK.Size = new System.Drawing.Size(82, 23);
			this.buttonAdvOK.TabIndex = 12;
			this.buttonAdvOK.Text = "xxOk";
			this.buttonAdvOK.UseVisualStyle = true;
			this.buttonAdvOK.Click += new System.EventHandler(this.buttonAdvOkClick);
			// 
			// MeetingRecurrenceView
			// 
			this.AcceptButton = this.AcceptButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.buttonAdvCancel;
			this.CaptionForeColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(534, 362);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HelpButton = false;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(550, 400);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(550, 400);
			this.Name = "MeetingRecurrenceView";
			this.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.RightToLeftLayout = true;
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "xxAppointmentRecurrence";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.groupBoxAppointmentTime.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEnd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStart)).EndInit();
			this.groupBoxRecurrencePattern.ResumeLayout(false);
			this.splitContainerAdv1.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerAdv1)).EndInit();
			this.splitContainerAdv1.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvMonthlyRecurring)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvDailyRecurring)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.radioButtonAdvWeeklyRecurring)).EndInit();
			this.groupBoxRangeOfRecurrence.ResumeLayout(false);
			this.tableLayoutPanel4.ResumeLayout(false);
			this.tableLayoutPanel4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStart.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStart)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEnd.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEnd)).EndInit();
			this.tableLayoutPanel5.ResumeLayout(false);
			this.ResumeLayout(false);

        }
        
        #endregion

		  private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBoxAppointmentTime;
        private System.Windows.Forms.GroupBox groupBoxRecurrencePattern;
        private System.Windows.Forms.GroupBox groupBoxRangeOfRecurrence;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelStart;
		  private System.Windows.Forms.Label labelEnd;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label labelRangeStart;
        private System.Windows.Forms.Label labelEndBy;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvStart;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker outlookTimePickerStart;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker outlookTimePickerEnd;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvRemove;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvCancel;
        private Syncfusion.Windows.Forms.ButtonAdv buttonAdvOK;
        private Syncfusion.Windows.Forms.Tools.SplitContainerAdv splitContainerAdv1;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvEnd;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvDailyRecurring;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvMonthlyRecurring;
        private Syncfusion.Windows.Forms.Tools.RadioButtonAdv radioButtonAdvWeeklyRecurring;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    }
}

