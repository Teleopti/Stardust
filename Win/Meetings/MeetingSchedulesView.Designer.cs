using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Meetings
{
    partial class MeetingSchedulesView
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
                if (_presenter != null)
                    _presenter.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle1 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle2 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle3 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			Syncfusion.Windows.Forms.Grid.GridBaseStyle gridBaseStyle4 = new Syncfusion.Windows.Forms.Grid.GridBaseStyle();
			this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelGrid = new System.Windows.Forms.TableLayoutPanel();
			this.gridControlSchedules = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelStartTime = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelEndTime = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.dateTimePickerAdvStartDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.dateTimePickerAdvEndDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.outlookTimePickerStartTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.outlookTimePickerEndTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.tableLayoutPanelCalendar = new System.Windows.Forms.TableLayoutPanel();
			this.monthCalendarAdvDateSelection = new Syncfusion.Windows.Forms.Tools.MonthCalendarAdv();
			this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.gridControlSuggestions = new Syncfusion.Windows.Forms.Grid.GridControl();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelEndSpan = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelStartSpan = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.office2007OutlookTimePickerStartSpan = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.office2007OutlookTimePickerEndSpan = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gridControlSchedules)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStartTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEndTime)).BeginInit();
			this.tableLayoutPanelCalendar.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.monthCalendarAdvDateSelection)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlSuggestions)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerStartSpan)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerEndSpan)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanelMain
			// 
			this.tableLayoutPanelMain.ColumnCount = 2;
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelGrid, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelCalendar, 1, 0);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 1;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(832, 650);
			this.tableLayoutPanelMain.TabIndex = 0;
			// 
			// tableLayoutPanelGrid
			// 
			this.tableLayoutPanelGrid.ColumnCount = 1;
			this.tableLayoutPanelGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelGrid.Controls.Add(this.gridControlSchedules, 0, 0);
			this.tableLayoutPanelGrid.Controls.Add(this.tableLayoutPanel1, 0, 1);
			this.tableLayoutPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelGrid.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanelGrid.Name = "tableLayoutPanelGrid";
			this.tableLayoutPanelGrid.RowCount = 2;
			this.tableLayoutPanelGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanelGrid.Size = new System.Drawing.Size(646, 644);
			this.tableLayoutPanelGrid.TabIndex = 0;
			// 
			// gridControlSchedules
			// 
			this.gridControlSchedules.ColCount = 1;
			this.gridControlSchedules.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 140),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 2160)});
			this.gridControlSchedules.DefaultRowHeight = 25;
			this.gridControlSchedules.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlSchedules.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlSchedules.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlSchedules.HScrollPixel = true;
			this.gridControlSchedules.Location = new System.Drawing.Point(0, 0);
			this.gridControlSchedules.Margin = new System.Windows.Forms.Padding(0);
			this.gridControlSchedules.Name = "gridControlSchedules";
			this.gridControlSchedules.NumberedColHeaders = false;
			this.gridControlSchedules.NumberedRowHeaders = false;
			this.gridControlSchedules.Office2007ScrollBars = true;
			this.gridControlSchedules.RowCount = 0;
			this.gridControlSchedules.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 35)});
			this.gridControlSchedules.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlSchedules.Size = new System.Drawing.Size(646, 564);
			this.gridControlSchedules.SmartSizeBox = false;
			this.gridControlSchedules.TabIndex = 0;
			this.gridControlSchedules.Text = "gridControl1";
			this.gridControlSchedules.ThemesEnabled = true;
			this.gridControlSchedules.UseRightToLeftCompatibleTextBox = true;
			this.gridControlSchedules.VScrollPixel = true;
			this.gridControlSchedules.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.GridControlSchedulesQueryCellInfo);
			this.gridControlSchedules.QueryColCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.GridControlSchedulesQueryColCount);
			this.gridControlSchedules.QueryRowCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.GridControlSchedulesQueryRowCount);
			this.gridControlSchedules.HScrollPixelPosChanged += new Syncfusion.Windows.Forms.Grid.GridScrollPositionChangedEventHandler(this.GridControlSchedulesScrollPixelPosChanged);
			this.gridControlSchedules.VScrollPixelPosChanged += new Syncfusion.Windows.Forms.Grid.GridScrollPositionChangedEventHandler(this.GridControlSchedulesScrollPixelPosChanged);
			this.gridControlSchedules.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridControlSchedulesMouseMove);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.autoLabelStartTime, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelEndTime, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvStartDate, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvEndDate, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerStartTime, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerEndTime, 2, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 564);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(646, 80);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// autoLabelStartTime
			// 
			this.autoLabelStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelStartTime.Location = new System.Drawing.Point(3, 6);
			this.autoLabelStartTime.Name = "autoLabelStartTime";
			this.autoLabelStartTime.Size = new System.Drawing.Size(134, 13);
			this.autoLabelStartTime.TabIndex = 0;
			this.autoLabelStartTime.Text = "xxStartTimeColon";
			// 
			// autoLabelEndTime
			// 
			this.autoLabelEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelEndTime.Location = new System.Drawing.Point(3, 31);
			this.autoLabelEndTime.Name = "autoLabelEndTime";
			this.autoLabelEndTime.Size = new System.Drawing.Size(134, 13);
			this.autoLabelEndTime.TabIndex = 0;
			this.autoLabelEndTime.Text = "xxEndTimeColon";
			// 
			// dateTimePickerAdvStartDate
			// 
			this.dateTimePickerAdvStartDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvStartDate.BorderColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvStartDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStartDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvStartDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvStartDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateTimePickerAdvStartDate.Calendar.HeaderHeight = 20;
			this.dateTimePickerAdvStartDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvStartDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Calendar.HeadGradient = true;
			this.dateTimePickerAdvStartDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvStartDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvStartDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvStartDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvStartDate.Calendar.Size = new System.Drawing.Size(194, 174);
			this.dateTimePickerAdvStartDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvStartDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvStartDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvStartDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStartDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Location = new System.Drawing.Point(122, 0);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Size = new System.Drawing.Size(194, 20);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvStartDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartDate.DropDownImage = null;
			this.dateTimePickerAdvStartDate.EnableNullDate = false;
			this.dateTimePickerAdvStartDate.EnableNullKeys = false;
			this.dateTimePickerAdvStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvStartDate.Location = new System.Drawing.Point(143, 3);
			this.dateTimePickerAdvStartDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvStartDate.Name = "dateTimePickerAdvStartDate";
			this.dateTimePickerAdvStartDate.NoneButtonVisible = false;
			this.dateTimePickerAdvStartDate.ShowCheckBox = false;
			this.dateTimePickerAdvStartDate.Size = new System.Drawing.Size(114, 19);
			this.dateTimePickerAdvStartDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvStartDate.TabIndex = 1;
			this.dateTimePickerAdvStartDate.Value = new System.DateTime(2009, 10, 28, 16, 45, 2, 975);
			this.dateTimePickerAdvStartDate.ValueChanged += new System.EventHandler(this.dateTimePickerAdvStartDate_ValueChanged);
			this.dateTimePickerAdvStartDate.PopupClosed += new Syncfusion.Windows.Forms.PopupClosedEventHandler(this.DateTimePickerAdvStartDatePopupClosed);
			// 
			// dateTimePickerAdvEndDate
			// 
			this.dateTimePickerAdvEndDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvEndDate.BorderColor = System.Drawing.Color.Empty;
			this.dateTimePickerAdvEndDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvEndDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndDate.Calendar.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEndDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvEndDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvEndDate.Calendar.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateTimePickerAdvEndDate.Calendar.HeaderHeight = 20;
			this.dateTimePickerAdvEndDate.Calendar.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvEndDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Calendar.HeadGradient = true;
			this.dateTimePickerAdvEndDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvEndDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvEndDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvEndDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvEndDate.Calendar.Size = new System.Drawing.Size(194, 174);
			this.dateTimePickerAdvEndDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvEndDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvEndDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvEndDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEndDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Location = new System.Drawing.Point(122, 0);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Size = new System.Drawing.Size(72, 20);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Size = new System.Drawing.Size(194, 20);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndDate.CalendarTitleBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.dateTimePickerAdvEndDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndDate.DropDownImage = null;
			this.dateTimePickerAdvEndDate.Enabled = false;
			this.dateTimePickerAdvEndDate.EnableNullDate = false;
			this.dateTimePickerAdvEndDate.EnableNullKeys = false;
			this.dateTimePickerAdvEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvEndDate.Location = new System.Drawing.Point(143, 28);
			this.dateTimePickerAdvEndDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvEndDate.Name = "dateTimePickerAdvEndDate";
			this.dateTimePickerAdvEndDate.NoneButtonVisible = false;
			this.dateTimePickerAdvEndDate.ShowCheckBox = false;
			this.dateTimePickerAdvEndDate.Size = new System.Drawing.Size(114, 19);
			this.dateTimePickerAdvEndDate.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.dateTimePickerAdvEndDate.TabIndex = 1;
			this.dateTimePickerAdvEndDate.Value = new System.DateTime(2009, 10, 28, 16, 45, 2, 975);
			// 
			// outlookTimePickerStartTime
			// 
			this.outlookTimePickerStartTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.outlookTimePickerStartTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Location = new System.Drawing.Point(263, 3);
			this.outlookTimePickerStartTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerStartTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Name = "outlookTimePickerStartTime";
			this.outlookTimePickerStartTime.Size = new System.Drawing.Size(65, 21);
			this.outlookTimePickerStartTime.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.outlookTimePickerStartTime.TabIndex = 2;
			this.outlookTimePickerStartTime.Text = "00:00";
			// 
			// outlookTimePickerEndTime
			// 
			this.outlookTimePickerEndTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.outlookTimePickerEndTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Location = new System.Drawing.Point(263, 28);
			this.outlookTimePickerEndTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerEndTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Name = "outlookTimePickerEndTime";
			this.outlookTimePickerEndTime.Size = new System.Drawing.Size(65, 21);
			this.outlookTimePickerEndTime.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.outlookTimePickerEndTime.TabIndex = 2;
			this.outlookTimePickerEndTime.Text = "00:00";
			// 
			// tableLayoutPanelCalendar
			// 
			this.tableLayoutPanelCalendar.ColumnCount = 1;
			this.tableLayoutPanelCalendar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCalendar.Controls.Add(this.monthCalendarAdvDateSelection, 0, 0);
			this.tableLayoutPanelCalendar.Controls.Add(this.autoLabel2, 0, 4);
			this.tableLayoutPanelCalendar.Controls.Add(this.gridControlSuggestions, 0, 6);
			this.tableLayoutPanelCalendar.Controls.Add(this.tableLayoutPanel2, 0, 5);
			this.tableLayoutPanelCalendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelCalendar.Location = new System.Drawing.Point(655, 3);
			this.tableLayoutPanelCalendar.Name = "tableLayoutPanelCalendar";
			this.tableLayoutPanelCalendar.RowCount = 7;
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCalendar.Size = new System.Drawing.Size(174, 644);
			this.tableLayoutPanelCalendar.TabIndex = 1;
			// 
			// monthCalendarAdvDateSelection
			// 
			this.monthCalendarAdvDateSelection.AllowMultipleSelection = false;
			this.monthCalendarAdvDateSelection.Culture = new System.Globalization.CultureInfo("");
			this.monthCalendarAdvDateSelection.DaysFont = new System.Drawing.Font("Verdana", 8F);
			this.monthCalendarAdvDateSelection.DaysHeaderInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			this.monthCalendarAdvDateSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.monthCalendarAdvDateSelection.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.monthCalendarAdvDateSelection.HeaderEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
			this.monthCalendarAdvDateSelection.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.monthCalendarAdvDateSelection.HeaderHeight = 20;
			this.monthCalendarAdvDateSelection.HeaderStartColor = System.Drawing.Color.FromArgb(((int)(((byte)(179)))), ((int)(((byte)(209)))), ((int)(((byte)(252)))));
			this.monthCalendarAdvDateSelection.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.monthCalendarAdvDateSelection.HeadGradient = true;
			this.monthCalendarAdvDateSelection.Iso8601CalenderFormat = false;
			this.monthCalendarAdvDateSelection.Location = new System.Drawing.Point(3, 3);
			this.monthCalendarAdvDateSelection.Name = "monthCalendarAdvDateSelection";
			this.monthCalendarAdvDateSelection.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.monthCalendarAdvDateSelection.SelectedDates = new System.DateTime[] {
        new System.DateTime(2013, 12, 12, 0, 0, 0, 0)};
			this.monthCalendarAdvDateSelection.Size = new System.Drawing.Size(168, 134);
			this.monthCalendarAdvDateSelection.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.monthCalendarAdvDateSelection.TabIndex = 0;
			this.monthCalendarAdvDateSelection.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.monthCalendarAdvDateSelection.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.monthCalendarAdvDateSelection.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.monthCalendarAdvDateSelection.NoneButton.Location = new System.Drawing.Point(96, 0);
			this.monthCalendarAdvDateSelection.NoneButton.Size = new System.Drawing.Size(72, 20);
			this.monthCalendarAdvDateSelection.NoneButton.Text = "None";
			this.monthCalendarAdvDateSelection.NoneButton.UseVisualStyle = true;
			this.monthCalendarAdvDateSelection.NoneButton.Visible = false;
			// 
			// 
			// 
			this.monthCalendarAdvDateSelection.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Office2007;
			this.monthCalendarAdvDateSelection.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.monthCalendarAdvDateSelection.TodayButton.Size = new System.Drawing.Size(168, 20);
			this.monthCalendarAdvDateSelection.TodayButton.Text = "Today";
			this.monthCalendarAdvDateSelection.TodayButton.UseVisualStyle = true;
			this.monthCalendarAdvDateSelection.DateSelected += new System.EventHandler(this.MonthCalendarAdvDateSelectionDateSelected);
			this.monthCalendarAdvDateSelection.DateCellQueryInfo += new Syncfusion.Windows.Forms.Tools.DateCellQueryInfoEventHandler(this.MonthCalendarAdvDateSelectionDateCellQueryInfo);
			// 
			// autoLabel2
			// 
			this.autoLabel2.Location = new System.Drawing.Point(3, 156);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(153, 13);
			this.autoLabel2.TabIndex = 3;
			this.autoLabel2.Text = "xxSuggestTimesBetweenColon";
			// 
			// gridControlSuggestions
			// 
			this.gridControlSuggestions.AllowSelection = Syncfusion.Windows.Forms.Grid.GridSelectionFlags.Row;
			gridBaseStyle1.Name = "Header";
			gridBaseStyle1.StyleInfo.Borders.Bottom = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Left = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Right = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.Borders.Top = new Syncfusion.Windows.Forms.Grid.GridBorder(Syncfusion.Windows.Forms.Grid.GridBorderStyle.None);
			gridBaseStyle1.StyleInfo.CellType = "Header";
			gridBaseStyle1.StyleInfo.Font.Bold = true;
			gridBaseStyle1.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			gridBaseStyle1.StyleInfo.VerticalAlignment = Syncfusion.Windows.Forms.Grid.GridVerticalAlignment.Middle;
			gridBaseStyle2.Name = "Standard";
			gridBaseStyle2.StyleInfo.Font.Facename = "Tahoma";
			gridBaseStyle2.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			gridBaseStyle3.Name = "Column Header";
			gridBaseStyle3.StyleInfo.BaseStyle = "Header";
			gridBaseStyle3.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Center;
			gridBaseStyle4.Name = "Row Header";
			gridBaseStyle4.StyleInfo.AutoSize = false;
			gridBaseStyle4.StyleInfo.BaseStyle = "Header";
			gridBaseStyle4.StyleInfo.HorizontalAlignment = Syncfusion.Windows.Forms.Grid.GridHorizontalAlignment.Left;
			gridBaseStyle4.StyleInfo.Interior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Horizontal, System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(199)))), ((int)(((byte)(184))))), System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(234)))), ((int)(((byte)(216))))));
			this.gridControlSuggestions.BaseStylesMap.AddRange(new Syncfusion.Windows.Forms.Grid.GridBaseStyle[] {
            gridBaseStyle1,
            gridBaseStyle2,
            gridBaseStyle3,
            gridBaseStyle4});
			this.gridControlSuggestions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.gridControlSuggestions.ColCount = 0;
			this.gridControlSuggestions.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 180)});
			this.gridControlSuggestions.DefaultRowHeight = 25;
			this.gridControlSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlSuggestions.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gridControlSuggestions.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Office2007;
			this.gridControlSuggestions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Office2007Blue;
			this.gridControlSuggestions.Location = new System.Drawing.Point(0, 238);
			this.gridControlSuggestions.Margin = new System.Windows.Forms.Padding(0);
			this.gridControlSuggestions.Name = "gridControlSuggestions";
			this.gridControlSuggestions.NumberedColHeaders = false;
			this.gridControlSuggestions.NumberedRowHeaders = false;
			this.gridControlSuggestions.Office2007ScrollBars = true;
			this.gridControlSuggestions.Properties.BackgroundColor = System.Drawing.Color.Transparent;
			this.gridControlSuggestions.ResizeColsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.gridControlSuggestions.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.gridControlSuggestions.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.gridControlSuggestions.RowCount = 48;
			this.gridControlSuggestions.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 25)});
			this.gridControlSuggestions.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlSuggestions.Size = new System.Drawing.Size(174, 406);
			this.gridControlSuggestions.SmartSizeBox = false;
			this.gridControlSuggestions.TabIndex = 0;
			this.gridControlSuggestions.ThemesEnabled = true;
			this.gridControlSuggestions.UseRightToLeftCompatibleTextBox = true;
			this.gridControlSuggestions.VScrollPixel = true;
			this.gridControlSuggestions.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.GridControlSuggestionsQueryCellInfo);
			this.gridControlSuggestions.QueryColCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.GridControlSchedulesQueryColCount);
			this.gridControlSuggestions.QueryRowCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.GridControlSuggestionsQueryRowCount);
			this.gridControlSuggestions.SelectionChanged += new Syncfusion.Windows.Forms.Grid.GridSelectionChangedEventHandler(this.GridControlSuggestionsSelectionChanged);
			this.gridControlSuggestions.HScrollPixelPosChanged += new Syncfusion.Windows.Forms.Grid.GridScrollPositionChangedEventHandler(this.GridControlSchedulesScrollPixelPosChanged);
			this.gridControlSuggestions.VScrollPixelPosChanged += new Syncfusion.Windows.Forms.Grid.GridScrollPositionChangedEventHandler(this.GridControlSchedulesScrollPixelPosChanged);
			this.gridControlSuggestions.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gridControlSuggestionsMouseUp);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 2;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.21429F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.78571F));
			this.tableLayoutPanel2.Controls.Add(this.autoLabelEndSpan, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelStartSpan, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.office2007OutlookTimePickerStartSpan, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.office2007OutlookTimePickerEndSpan, 1, 1);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 177);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.9434F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.0566F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(168, 58);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// autoLabelEndSpan
			// 
			this.autoLabelEndSpan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelEndSpan.Location = new System.Drawing.Point(3, 37);
			this.autoLabelEndSpan.Name = "autoLabelEndSpan";
			this.autoLabelEndSpan.Size = new System.Drawing.Size(75, 13);
			this.autoLabelEndSpan.TabIndex = 5;
			this.autoLabelEndSpan.Text = "xxEndPeriod";
			// 
			// autoLabelStartSpan
			// 
			this.autoLabelStartSpan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelStartSpan.Location = new System.Drawing.Point(3, 8);
			this.autoLabelStartSpan.Name = "autoLabelStartSpan";
			this.autoLabelStartSpan.Size = new System.Drawing.Size(75, 13);
			this.autoLabelStartSpan.TabIndex = 1;
			this.autoLabelStartSpan.Text = "xxStartPeriod";
			// 
			// office2007OutlookTimePickerStartSpan
			// 
			this.office2007OutlookTimePickerStartSpan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.office2007OutlookTimePickerStartSpan.BindableTimeValue = System.TimeSpan.Parse("08:00:00");
			this.office2007OutlookTimePickerStartSpan.Location = new System.Drawing.Point(84, 3);
			this.office2007OutlookTimePickerStartSpan.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerStartSpan.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerStartSpan.Name = "office2007OutlookTimePickerStartSpan";
			this.office2007OutlookTimePickerStartSpan.Size = new System.Drawing.Size(65, 21);
			this.office2007OutlookTimePickerStartSpan.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.office2007OutlookTimePickerStartSpan.TabIndex = 3;
			this.office2007OutlookTimePickerStartSpan.Text = "8:00 AM";
			this.office2007OutlookTimePickerStartSpan.TextChanged += new System.EventHandler(this.Office2007OutlookTimePickerSpanTextChanged);
			// 
			// office2007OutlookTimePickerEndSpan
			// 
			this.office2007OutlookTimePickerEndSpan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(251)))));
			this.office2007OutlookTimePickerEndSpan.BindableTimeValue = System.TimeSpan.Parse("17:00:00");
			this.office2007OutlookTimePickerEndSpan.Location = new System.Drawing.Point(84, 32);
			this.office2007OutlookTimePickerEndSpan.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerEndSpan.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerEndSpan.Name = "office2007OutlookTimePickerEndSpan";
			this.office2007OutlookTimePickerEndSpan.Size = new System.Drawing.Size(65, 21);
			this.office2007OutlookTimePickerEndSpan.Style = Syncfusion.Windows.Forms.VisualStyle.Office2007;
			this.office2007OutlookTimePickerEndSpan.TabIndex = 4;
			this.office2007OutlookTimePickerEndSpan.Text = "5:00 PM";
			this.office2007OutlookTimePickerEndSpan.TextChanged += new System.EventHandler(this.Office2007OutlookTimePickerSpanTextChanged);
			// 
			// MeetingSchedulesView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Name = "MeetingSchedulesView";
			this.Size = new System.Drawing.Size(832, 650);
			this.Resize += new System.EventHandler(this.MeetingSchedulesView_Resize);
			this.tableLayoutPanelMain.ResumeLayout(false);
			this.tableLayoutPanelGrid.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gridControlSchedules)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStartTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEndTime)).EndInit();
			this.tableLayoutPanelCalendar.ResumeLayout(false);
			this.tableLayoutPanelCalendar.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.monthCalendarAdvDateSelection)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControlSuggestions)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerStartSpan)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerEndSpan)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelGrid;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlSchedules;
        private Syncfusion.Windows.Forms.Grid.GridControl gridControlSuggestions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelStartTime;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelEndTime;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvStartDate;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvEndDate;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker outlookTimePickerStartTime;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker outlookTimePickerEndTime;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCalendar;
		private Syncfusion.Windows.Forms.Tools.MonthCalendarAdv monthCalendarAdvDateSelection;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelEndSpan;
        private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelStartSpan;
        private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker office2007OutlookTimePickerStartSpan;
		private Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker office2007OutlookTimePickerEndSpan;

    }
}
