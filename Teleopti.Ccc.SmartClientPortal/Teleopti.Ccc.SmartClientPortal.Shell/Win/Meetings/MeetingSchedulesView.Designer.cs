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
			this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelGrid, 0, 0);
			this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelCalendar, 1, 0);
			this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
			this.tableLayoutPanelMain.RowCount = 1;
			this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelMain.Size = new System.Drawing.Size(800, 600);
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
			this.tableLayoutPanelGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanelGrid.Size = new System.Drawing.Size(584, 594);
			this.tableLayoutPanelGrid.TabIndex = 0;
			// 
			// gridControlSchedules
			// 
			this.gridControlSchedules.AlphaBlendSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(94)))), ((int)(((byte)(171)))), ((int)(((byte)(222)))));
			this.gridControlSchedules.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.gridControlSchedules.ColCount = 1;
			this.gridControlSchedules.ColWidthEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridColWidth[] {
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 140),
            new Syncfusion.Windows.Forms.Grid.GridColWidth(1, 2160)});
			this.gridControlSchedules.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlSchedules.DefaultRowHeight = 20;
			this.gridControlSchedules.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlSchedules.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlSchedules.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlSchedules.HScrollPixel = true;
			this.gridControlSchedules.Location = new System.Drawing.Point(0, 0);
			this.gridControlSchedules.Margin = new System.Windows.Forms.Padding(0);
			this.gridControlSchedules.MetroScrollBars = true;
			this.gridControlSchedules.Name = "gridControlSchedules";
			this.gridControlSchedules.NumberedColHeaders = false;
			this.gridControlSchedules.NumberedRowHeaders = false;
			this.gridControlSchedules.Properties.ForceImmediateRepaint = false;
			this.gridControlSchedules.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlSchedules.Properties.MarkColHeader = false;
			this.gridControlSchedules.Properties.MarkRowHeader = false;
			this.gridControlSchedules.RowCount = 0;
			this.gridControlSchedules.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlSchedules.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlSchedules.Size = new System.Drawing.Size(584, 504);
			this.gridControlSchedules.SmartSizeBox = false;
			this.gridControlSchedules.TabIndex = 0;
			this.gridControlSchedules.Text = "gridControl1";
			this.gridControlSchedules.ThemesEnabled = true;
			this.gridControlSchedules.UseRightToLeftCompatibleTextBox = true;
			this.gridControlSchedules.VScrollPixel = true;
			this.gridControlSchedules.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControlSchedulesQueryCellInfo);
			this.gridControlSchedules.QueryColCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.gridControlSchedulesQueryColCount);
			this.gridControlSchedules.QueryRowCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.gridControlSchedulesQueryRowCount);
			this.gridControlSchedules.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gridControlSchedulesMouseMove);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 285F));
			this.tableLayoutPanel1.Controls.Add(this.autoLabelStartTime, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.autoLabelEndTime, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvStartDate, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.dateTimePickerAdvEndDate, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerStartTime, 2, 1);
			this.tableLayoutPanel1.Controls.Add(this.outlookTimePickerEndTime, 2, 2);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 504);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(584, 90);
			this.tableLayoutPanel1.TabIndex = 1;
			// 
			// autoLabelStartTime
			// 
			this.autoLabelStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelStartTime.Location = new System.Drawing.Point(3, 32);
			this.autoLabelStartTime.Name = "autoLabelStartTime";
			this.autoLabelStartTime.Size = new System.Drawing.Size(73, 29);
			this.autoLabelStartTime.TabIndex = 0;
			this.autoLabelStartTime.Text = "xxStartTimeColon";
			// 
			// autoLabelEndTime
			// 
			this.autoLabelEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelEndTime.Location = new System.Drawing.Point(3, 61);
			this.autoLabelEndTime.Name = "autoLabelEndTime";
			this.autoLabelEndTime.Size = new System.Drawing.Size(73, 29);
			this.autoLabelEndTime.TabIndex = 0;
			this.autoLabelEndTime.Text = "xxEndTimeColon";
			// 
			// dateTimePickerAdvStartDate
			// 
			this.dateTimePickerAdvStartDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvStartDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvStartDate.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvStartDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartDate.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvStartDate.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvStartDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvStartDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvStartDate.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvStartDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartDate.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvStartDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvStartDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvStartDate.Calendar.ShowWeekNumbers = true;
			this.dateTimePickerAdvStartDate.Calendar.Size = new System.Drawing.Size(131, 174);
			this.dateTimePickerAdvStartDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvStartDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStartDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvStartDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStartDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Size = new System.Drawing.Size(131, 25);
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvStartDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartDate.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvStartDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartDate.DropDownImage = null;
			this.dateTimePickerAdvStartDate.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.EnableNullDate = false;
			this.dateTimePickerAdvStartDate.EnableNullKeys = false;
			this.dateTimePickerAdvStartDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvStartDate.Location = new System.Drawing.Point(82, 35);
			this.dateTimePickerAdvStartDate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvStartDate.Name = "dateTimePickerAdvStartDate";
			this.dateTimePickerAdvStartDate.NoneButtonVisible = false;
			this.dateTimePickerAdvStartDate.ShowCheckBox = false;
			this.dateTimePickerAdvStartDate.Size = new System.Drawing.Size(133, 21);
			this.dateTimePickerAdvStartDate.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStartDate.TabIndex = 1;
			this.dateTimePickerAdvStartDate.ThemesEnabled = true;
			this.dateTimePickerAdvStartDate.Value = new System.DateTime(2009, 10, 28, 16, 45, 2, 975);
			this.dateTimePickerAdvStartDate.PopupClosed += new Syncfusion.Windows.Forms.PopupClosedEventHandler(this.dateTimePickerAdvStartDatePopupClosed);
			// 
			// dateTimePickerAdvEndDate
			// 
			this.dateTimePickerAdvEndDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvEndDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvEndDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvEndDate.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvEndDate.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvEndDate.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndDate.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvEndDate.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvEndDate.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvEndDate.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndDate.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvEndDate.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvEndDate.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndDate.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvEndDate.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvEndDate.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvEndDate.Calendar.ShowWeekNumbers = true;
			this.dateTimePickerAdvEndDate.Calendar.Size = new System.Drawing.Size(131, 174);
			this.dateTimePickerAdvEndDate.Calendar.SizeToFit = true;
			this.dateTimePickerAdvEndDate.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEndDate.Calendar.TabIndex = 0;
			this.dateTimePickerAdvEndDate.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEndDate.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndDate.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Size = new System.Drawing.Size(131, 25);
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvEndDate.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndDate.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndDate.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvEndDate.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndDate.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndDate.DropDownImage = null;
			this.dateTimePickerAdvEndDate.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.Enabled = false;
			this.dateTimePickerAdvEndDate.EnableNullDate = false;
			this.dateTimePickerAdvEndDate.EnableNullKeys = false;
			this.dateTimePickerAdvEndDate.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvEndDate.Location = new System.Drawing.Point(82, 64);
			this.dateTimePickerAdvEndDate.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndDate.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvEndDate.Name = "dateTimePickerAdvEndDate";
			this.dateTimePickerAdvEndDate.NoneButtonVisible = false;
			this.dateTimePickerAdvEndDate.ShowCheckBox = false;
			this.dateTimePickerAdvEndDate.Size = new System.Drawing.Size(133, 21);
			this.dateTimePickerAdvEndDate.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEndDate.TabIndex = 1;
			this.dateTimePickerAdvEndDate.ThemesEnabled = true;
			this.dateTimePickerAdvEndDate.Value = new System.DateTime(2009, 10, 28, 16, 45, 2, 975);
			// 
			// outlookTimePickerStartTime
			// 
			this.outlookTimePickerStartTime.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerStartTime.BeforeTouchSize = new System.Drawing.Size(74, 23);
			this.outlookTimePickerStartTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Location = new System.Drawing.Point(222, 35);
			this.outlookTimePickerStartTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerStartTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Name = "outlookTimePickerStartTime";
			this.outlookTimePickerStartTime.Size = new System.Drawing.Size(74, 23);
			this.outlookTimePickerStartTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerStartTime.TabIndex = 2;
			this.outlookTimePickerStartTime.Text = "00:00";
			// 
			// outlookTimePickerEndTime
			// 
			this.outlookTimePickerEndTime.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerEndTime.BeforeTouchSize = new System.Drawing.Size(74, 23);
			this.outlookTimePickerEndTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Location = new System.Drawing.Point(222, 64);
			this.outlookTimePickerEndTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerEndTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Name = "outlookTimePickerEndTime";
			this.outlookTimePickerEndTime.Size = new System.Drawing.Size(74, 23);
			this.outlookTimePickerEndTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
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
			this.tableLayoutPanelCalendar.Location = new System.Drawing.Point(590, 0);
			this.tableLayoutPanelCalendar.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanelCalendar.Name = "tableLayoutPanelCalendar";
			this.tableLayoutPanelCalendar.RowCount = 7;
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 185F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 9F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
			this.tableLayoutPanelCalendar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelCalendar.Size = new System.Drawing.Size(210, 600);
			this.tableLayoutPanelCalendar.TabIndex = 1;
			// 
			// monthCalendarAdvDateSelection
			// 
			this.monthCalendarAdvDateSelection.AllowMultipleSelection = false;
			this.monthCalendarAdvDateSelection.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.monthCalendarAdvDateSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.monthCalendarAdvDateSelection.BottomHeight = 25;
			this.monthCalendarAdvDateSelection.Culture = new System.Globalization.CultureInfo("");
			this.monthCalendarAdvDateSelection.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.monthCalendarAdvDateSelection.DayNamesFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
			this.monthCalendarAdvDateSelection.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.monthCalendarAdvDateSelection.DaysHeaderInterior = new Syncfusion.Drawing.BrushInfo(System.Drawing.SystemColors.Window);
			this.monthCalendarAdvDateSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.monthCalendarAdvDateSelection.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.monthCalendarAdvDateSelection.HeaderHeight = 34;
			this.monthCalendarAdvDateSelection.HeaderStartColor = System.Drawing.Color.White;
			this.monthCalendarAdvDateSelection.HighlightColor = System.Drawing.Color.Black;
			this.monthCalendarAdvDateSelection.Iso8601CalenderFormat = false;
			this.monthCalendarAdvDateSelection.Location = new System.Drawing.Point(3, 3);
			this.monthCalendarAdvDateSelection.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.monthCalendarAdvDateSelection.Name = "monthCalendarAdvDateSelection";
			this.monthCalendarAdvDateSelection.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.monthCalendarAdvDateSelection.SelectedDates = new System.DateTime[] {
        new System.DateTime(2014, 8, 11, 0, 0, 0, 0)};
			this.monthCalendarAdvDateSelection.ShowWeekNumbers = true;
			this.monthCalendarAdvDateSelection.Size = new System.Drawing.Size(204, 179);
			this.monthCalendarAdvDateSelection.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.monthCalendarAdvDateSelection.TabIndex = 0;
			this.monthCalendarAdvDateSelection.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.monthCalendarAdvDateSelection.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.monthCalendarAdvDateSelection.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.monthCalendarAdvDateSelection.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(165)))), ((int)(((byte)(220)))));
			this.monthCalendarAdvDateSelection.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.monthCalendarAdvDateSelection.NoneButton.ForeColor = System.Drawing.Color.White;
			this.monthCalendarAdvDateSelection.NoneButton.IsBackStageButton = false;
			this.monthCalendarAdvDateSelection.NoneButton.Location = new System.Drawing.Point(129, 0);
			this.monthCalendarAdvDateSelection.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.monthCalendarAdvDateSelection.NoneButton.Text = "None";
			this.monthCalendarAdvDateSelection.NoneButton.UseVisualStyle = true;
			this.monthCalendarAdvDateSelection.NoneButton.Visible = false;
			// 
			// 
			// 
			this.monthCalendarAdvDateSelection.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.monthCalendarAdvDateSelection.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.monthCalendarAdvDateSelection.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.monthCalendarAdvDateSelection.TodayButton.ForeColor = System.Drawing.Color.White;
			this.monthCalendarAdvDateSelection.TodayButton.IsBackStageButton = false;
			this.monthCalendarAdvDateSelection.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.monthCalendarAdvDateSelection.TodayButton.Size = new System.Drawing.Size(204, 25);
			this.monthCalendarAdvDateSelection.TodayButton.Text = "Today";
			this.monthCalendarAdvDateSelection.TodayButton.UseVisualStyle = true;
			this.monthCalendarAdvDateSelection.DateSelected += new System.EventHandler(this.monthCalendarAdvDateSelectionDateSelected);
			this.monthCalendarAdvDateSelection.DateCellQueryInfo += new Syncfusion.Windows.Forms.Tools.DateCellQueryInfoEventHandler(this.monthCalendarAdvDateSelectionDateCellQueryInfo);
			// 
			// autoLabel2
			// 
			this.autoLabel2.Location = new System.Drawing.Point(3, 203);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(168, 15);
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
            new Syncfusion.Windows.Forms.Grid.GridColWidth(0, 200)});
			this.gridControlSuggestions.DefaultGridBorderStyle = Syncfusion.Windows.Forms.Grid.GridBorderStyle.Solid;
			this.gridControlSuggestions.DefaultRowHeight = 20;
			this.gridControlSuggestions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControlSuggestions.ForeColor = System.Drawing.SystemColors.ControlText;
			this.gridControlSuggestions.GridOfficeScrollBars = Syncfusion.Windows.Forms.OfficeScrollBars.Metro;
			this.gridControlSuggestions.GridVisualStyles = Syncfusion.Windows.Forms.GridVisualStyles.Metro;
			this.gridControlSuggestions.Location = new System.Drawing.Point(0, 298);
			this.gridControlSuggestions.Margin = new System.Windows.Forms.Padding(0);
			this.gridControlSuggestions.MetroScrollBars = true;
			this.gridControlSuggestions.Name = "gridControlSuggestions";
			this.gridControlSuggestions.NumberedColHeaders = false;
			this.gridControlSuggestions.NumberedRowHeaders = false;
			this.gridControlSuggestions.Properties.BackgroundColor = System.Drawing.Color.Transparent;
			this.gridControlSuggestions.Properties.ForceImmediateRepaint = false;
			this.gridControlSuggestions.Properties.GridLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(212)))));
			this.gridControlSuggestions.Properties.MarkColHeader = false;
			this.gridControlSuggestions.Properties.MarkRowHeader = false;
			this.gridControlSuggestions.ResizeColsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.gridControlSuggestions.ResizeRowsBehavior = Syncfusion.Windows.Forms.Grid.GridResizeCellsBehavior.None;
			this.gridControlSuggestions.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.gridControlSuggestions.RowCount = 48;
			this.gridControlSuggestions.RowHeightEntries.AddRange(new Syncfusion.Windows.Forms.Grid.GridRowHeight[] {
            new Syncfusion.Windows.Forms.Grid.GridRowHeight(0, 29)});
			this.gridControlSuggestions.SerializeCellsBehavior = Syncfusion.Windows.Forms.Grid.GridSerializeCellsBehavior.SerializeAsRangeStylesIntoCode;
			this.gridControlSuggestions.Size = new System.Drawing.Size(210, 302);
			this.gridControlSuggestions.SmartSizeBox = false;
			this.gridControlSuggestions.TabIndex = 0;
			this.gridControlSuggestions.ThemesEnabled = true;
			this.gridControlSuggestions.UseRightToLeftCompatibleTextBox = true;
			this.gridControlSuggestions.VScrollPixel = true;
			this.gridControlSuggestions.QueryCellInfo += new Syncfusion.Windows.Forms.Grid.GridQueryCellInfoEventHandler(this.gridControlSuggestionsQueryCellInfo);
			this.gridControlSuggestions.QueryColCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.gridControlSchedulesQueryColCount);
			this.gridControlSuggestions.QueryRowCount += new Syncfusion.Windows.Forms.Grid.GridRowColCountEventHandler(this.gridControlSuggestionsQueryRowCount);
			this.gridControlSuggestions.SelectionChanged += new Syncfusion.Windows.Forms.Grid.GridSelectionChangedEventHandler(this.gridControlSuggestionsSelectionChanged);
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
			this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 227);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 2;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.9434F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 49.0566F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(204, 67);
			this.tableLayoutPanel2.TabIndex = 4;
			// 
			// autoLabelEndSpan
			// 
			this.autoLabelEndSpan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelEndSpan.Location = new System.Drawing.Point(3, 43);
			this.autoLabelEndSpan.Name = "autoLabelEndSpan";
			this.autoLabelEndSpan.Size = new System.Drawing.Size(92, 15);
			this.autoLabelEndSpan.TabIndex = 5;
			this.autoLabelEndSpan.Text = "xxEndPeriod";
			// 
			// autoLabelStartSpan
			// 
			this.autoLabelStartSpan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelStartSpan.Location = new System.Drawing.Point(3, 9);
			this.autoLabelStartSpan.Name = "autoLabelStartSpan";
			this.autoLabelStartSpan.Size = new System.Drawing.Size(92, 15);
			this.autoLabelStartSpan.TabIndex = 1;
			this.autoLabelStartSpan.Text = "xxStartPeriod";
			// 
			// office2007OutlookTimePickerStartSpan
			// 
			this.office2007OutlookTimePickerStartSpan.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerStartSpan.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.office2007OutlookTimePickerStartSpan.BindableTimeValue = System.TimeSpan.Parse("08:00:00");
			this.office2007OutlookTimePickerStartSpan.Location = new System.Drawing.Point(101, 3);
			this.office2007OutlookTimePickerStartSpan.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerStartSpan.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerStartSpan.Name = "office2007OutlookTimePickerStartSpan";
			this.office2007OutlookTimePickerStartSpan.Size = new System.Drawing.Size(75, 23);
			this.office2007OutlookTimePickerStartSpan.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerStartSpan.TabIndex = 3;
			this.office2007OutlookTimePickerStartSpan.Text = "8:00 AM";
			this.office2007OutlookTimePickerStartSpan.TextChanged += new System.EventHandler(this.office2007OutlookTimePickerSpanTextChanged);
			// 
			// office2007OutlookTimePickerEndSpan
			// 
			this.office2007OutlookTimePickerEndSpan.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerEndSpan.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.office2007OutlookTimePickerEndSpan.BindableTimeValue = System.TimeSpan.Parse("17:00:00");
			this.office2007OutlookTimePickerEndSpan.Location = new System.Drawing.Point(101, 37);
			this.office2007OutlookTimePickerEndSpan.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerEndSpan.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerEndSpan.Name = "office2007OutlookTimePickerEndSpan";
			this.office2007OutlookTimePickerEndSpan.Size = new System.Drawing.Size(75, 23);
			this.office2007OutlookTimePickerEndSpan.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerEndSpan.TabIndex = 4;
			this.office2007OutlookTimePickerEndSpan.Text = "5:00 PM";
			this.office2007OutlookTimePickerEndSpan.TextChanged += new System.EventHandler(this.office2007OutlookTimePickerSpanTextChanged);
			// 
			// MeetingSchedulesView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanelMain);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "MeetingSchedulesView";
			this.Size = new System.Drawing.Size(800, 600);
			this.Resize += new System.EventHandler(this.meetingSchedulesViewResize);
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
