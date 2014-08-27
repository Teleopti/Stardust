namespace Teleopti.Ccc.Win.Meetings
{
	partial class MeetingImpactView
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
                if(_meetingStateHolderLoaderHelper != null)
                    _meetingStateHolderLoaderHelper.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxFind")]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.gradientPanel2 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.autoLabelPickResult = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.buttonAdvPickBest = new Syncfusion.Windows.Forms.ButtonAdv();
			this.autoLabel3 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel1 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabel2 = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.dateTimePickerAdvStartSlotPeriod = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.dateTimePickerAdvEndSlotPeriod = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.office2007OutlookTimePickerStartSlotPeriod = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.office2007OutlookTimePickerEndSlotPeriod = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.buttonAdvNext = new Syncfusion.Windows.Forms.ButtonAdv();
			this.buttonAdvPrevious = new Syncfusion.Windows.Forms.ButtonAdv();
			this.panel1 = new System.Windows.Forms.Panel();
			this.tabControlSkillResultGrid = new Syncfusion.Windows.Forms.Tools.TabControlAdv();
			this.gradientPanel1 = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.outlookTimePickerEndTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.outlookTimePickerStartTime = new Teleopti.Ccc.Win.Common.Controls.Office2007OutlookTimePicker(this.components);
			this.dateTimePickerAdvEndDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.dateTimePickerAdvStartDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv();
			this.autoLabelEndTime = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.autoLabelStartTime = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.gradientPanelTop = new Syncfusion.Windows.Forms.Tools.GradientPanel();
			this.progressBarLoading = new System.Windows.Forms.ProgressBar();
			this.autoLabelMeetingDate = new Syncfusion.Windows.Forms.Tools.AutoLabel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.imageListSkillTypeIcons = new System.Windows.Forms.ImageList(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).BeginInit();
			this.gradientPanel2.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartSlotPeriod)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartSlotPeriod.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndSlotPeriod)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndSlotPeriod.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerStartSlotPeriod)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerEndSlotPeriod)).BeginInit();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tabControlSkillResultGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).BeginInit();
			this.gradientPanel1.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEndTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStartTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate.Calendar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelTop)).BeginInit();
			this.gradientPanelTop.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 41.56863F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.43137F));
			this.tableLayoutPanel1.Controls.Add(this.gradientPanel2, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.gradientPanel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.gradientPanelTop, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 600);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// gradientPanel2
			// 
			this.gradientPanel2.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanel2.Controls.Add(this.tableLayoutPanel3);
			this.gradientPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel2.Location = new System.Drawing.Point(332, 510);
			this.gradientPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanel2.Name = "gradientPanel2";
			this.gradientPanel2.Size = new System.Drawing.Size(468, 90);
			this.gradientPanel2.TabIndex = 6;
			// 
			// tableLayoutPanel3
			// 
			this.tableLayoutPanel3.ColumnCount = 6;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 93F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 41F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 82F));
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 41F));
			this.tableLayoutPanel3.Controls.Add(this.autoLabelPickResult, 3, 0);
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvPickBest, 4, 1);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel3, 0, 0);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel1, 0, 1);
			this.tableLayoutPanel3.Controls.Add(this.autoLabel2, 0, 2);
			this.tableLayoutPanel3.Controls.Add(this.dateTimePickerAdvStartSlotPeriod, 1, 1);
			this.tableLayoutPanel3.Controls.Add(this.dateTimePickerAdvEndSlotPeriod, 1, 2);
			this.tableLayoutPanel3.Controls.Add(this.office2007OutlookTimePickerStartSlotPeriod, 2, 1);
			this.tableLayoutPanel3.Controls.Add(this.office2007OutlookTimePickerEndSlotPeriod, 2, 2);
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvNext, 5, 1);
			this.tableLayoutPanel3.Controls.Add(this.buttonAdvPrevious, 3, 1);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 3;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(464, 86);
			this.tableLayoutPanel3.TabIndex = 4;
			// 
			// autoLabelPickResult
			// 
			this.autoLabelPickResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel3.SetColumnSpan(this.autoLabelPickResult, 3);
			this.autoLabelPickResult.Location = new System.Drawing.Point(303, 7);
			this.autoLabelPickResult.Name = "autoLabelPickResult";
			this.autoLabelPickResult.Size = new System.Drawing.Size(158, 15);
			this.autoLabelPickResult.TabIndex = 15;
			// 
			// buttonAdvPickBest
			// 
			this.buttonAdvPickBest.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonAdvPickBest.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvPickBest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvPickBest.BeforeTouchSize = new System.Drawing.Size(75, 22);
			this.buttonAdvPickBest.ForeColor = System.Drawing.Color.White;
			this.buttonAdvPickBest.IsBackStageButton = false;
			this.buttonAdvPickBest.Location = new System.Drawing.Point(344, 32);
			this.buttonAdvPickBest.Name = "buttonAdvPickBest";
			this.buttonAdvPickBest.Size = new System.Drawing.Size(75, 22);
			this.buttonAdvPickBest.TabIndex = 9;
			this.buttonAdvPickBest.Text = "xxFind";
			this.buttonAdvPickBest.UseVisualStyle = true;
			this.buttonAdvPickBest.Click += new System.EventHandler(this.buttonAdvPickBestClick);
			// 
			// autoLabel3
			// 
			this.autoLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel3.SetColumnSpan(this.autoLabel3, 2);
			this.autoLabel3.Location = new System.Drawing.Point(3, 7);
			this.autoLabel3.Name = "autoLabel3";
			this.autoLabel3.Size = new System.Drawing.Size(201, 15);
			this.autoLabel3.TabIndex = 3;
			this.autoLabel3.Text = "xxPickBestMeetingSlot";
			// 
			// autoLabel1
			// 
			this.autoLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabel1.Location = new System.Drawing.Point(3, 29);
			this.autoLabel1.Name = "autoLabel1";
			this.autoLabel1.Size = new System.Drawing.Size(61, 29);
			this.autoLabel1.TabIndex = 0;
			this.autoLabel1.Text = "xxStartTimeColon";
			// 
			// autoLabel2
			// 
			this.autoLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabel2.Location = new System.Drawing.Point(3, 58);
			this.autoLabel2.Name = "autoLabel2";
			this.autoLabel2.Size = new System.Drawing.Size(61, 28);
			this.autoLabel2.TabIndex = 0;
			this.autoLabel2.Text = "xxEndTimeColon";
			// 
			// dateTimePickerAdvStartSlotPeriod
			// 
			this.dateTimePickerAdvStartSlotPeriod.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvStartSlotPeriod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvStartSlotPeriod.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvStartSlotPeriod.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartSlotPeriod.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartSlotPeriod.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvStartSlotPeriod.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Size = new System.Drawing.Size(131, 174);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.SizeToFit = true;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TabIndex = 0;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.Size = new System.Drawing.Size(131, 25);
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvStartSlotPeriod.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvStartSlotPeriod.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvStartSlotPeriod.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvStartSlotPeriod.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvStartSlotPeriod.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvStartSlotPeriod.DropDownImage = null;
			this.dateTimePickerAdvStartSlotPeriod.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.EnableNullDate = false;
			this.dateTimePickerAdvStartSlotPeriod.EnableNullKeys = false;
			this.dateTimePickerAdvStartSlotPeriod.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvStartSlotPeriod.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvStartSlotPeriod.Location = new System.Drawing.Point(70, 32);
			this.dateTimePickerAdvStartSlotPeriod.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvStartSlotPeriod.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvStartSlotPeriod.Name = "dateTimePickerAdvStartSlotPeriod";
			this.dateTimePickerAdvStartSlotPeriod.NoneButtonVisible = false;
			this.dateTimePickerAdvStartSlotPeriod.ShowCheckBox = false;
			this.dateTimePickerAdvStartSlotPeriod.Size = new System.Drawing.Size(133, 21);
			this.dateTimePickerAdvStartSlotPeriod.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvStartSlotPeriod.TabIndex = 4;
			this.dateTimePickerAdvStartSlotPeriod.ThemesEnabled = true;
			this.dateTimePickerAdvStartSlotPeriod.Value = new System.DateTime(2009, 10, 28, 16, 45, 2, 975);
			// 
			// dateTimePickerAdvEndSlotPeriod
			// 
			this.dateTimePickerAdvEndSlotPeriod.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvEndSlotPeriod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvEndSlotPeriod.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvEndSlotPeriod.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.BottomHeight = 25;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndSlotPeriod.Calendar.DayNamesColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.Calendar.DayNamesFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.DaysFont = new System.Drawing.Font("Segoe UI", 9F);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndSlotPeriod.Calendar.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.GridLines = Syncfusion.Windows.Forms.Grid.GridBorderStyle.None;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.HeaderEndColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.HeaderStartColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.HeadForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.HighlightColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Iso8601CalenderFormat = false;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Name = "monthCalendar";
			this.dateTimePickerAdvEndSlotPeriod.Calendar.ScrollButtonSize = new System.Drawing.Size(24, 24);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.SelectedDates = new System.DateTime[0];
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Size = new System.Drawing.Size(131, 174);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.SizeToFit = true;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TabIndex = 0;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.WeekFont = new System.Drawing.Font("Verdana", 8F);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.WeekInterior = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, System.Drawing.Color.PeachPuff, System.Drawing.Color.AntiqueWhite);
			// 
			// 
			// 
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.IsBackStageButton = false;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.Location = new System.Drawing.Point(125, 0);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.Size = new System.Drawing.Size(84, 25);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.Text = "None";
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.NoneButton.Visible = false;
			// 
			// 
			// 
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.BeforeTouchSize = new System.Drawing.Size(75, 23);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.ForeColor = System.Drawing.Color.White;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.IsBackStageButton = false;
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.Location = new System.Drawing.Point(0, 0);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.Size = new System.Drawing.Size(131, 25);
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.Text = "Today";
			this.dateTimePickerAdvEndSlotPeriod.Calendar.TodayButton.UseVisualStyle = true;
			this.dateTimePickerAdvEndSlotPeriod.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.dateTimePickerAdvEndSlotPeriod.CalendarSize = new System.Drawing.Size(189, 176);
			this.dateTimePickerAdvEndSlotPeriod.CalendarTitleForeColor = System.Drawing.SystemColors.ControlText;
			this.dateTimePickerAdvEndSlotPeriod.Culture = new System.Globalization.CultureInfo("sv-SE");
			this.dateTimePickerAdvEndSlotPeriod.DropDownImage = null;
			this.dateTimePickerAdvEndSlotPeriod.DropDownNormalColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.DropDownPressedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.DropDownSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(179)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.EnableNullDate = false;
			this.dateTimePickerAdvEndSlotPeriod.EnableNullKeys = false;
			this.dateTimePickerAdvEndSlotPeriod.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this.dateTimePickerAdvEndSlotPeriod.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateTimePickerAdvEndSlotPeriod.Location = new System.Drawing.Point(70, 61);
			this.dateTimePickerAdvEndSlotPeriod.MetroColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.dateTimePickerAdvEndSlotPeriod.MinValue = new System.DateTime(((long)(0)));
			this.dateTimePickerAdvEndSlotPeriod.Name = "dateTimePickerAdvEndSlotPeriod";
			this.dateTimePickerAdvEndSlotPeriod.NoneButtonVisible = false;
			this.dateTimePickerAdvEndSlotPeriod.ShowCheckBox = false;
			this.dateTimePickerAdvEndSlotPeriod.Size = new System.Drawing.Size(133, 21);
			this.dateTimePickerAdvEndSlotPeriod.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.dateTimePickerAdvEndSlotPeriod.TabIndex = 6;
			this.dateTimePickerAdvEndSlotPeriod.ThemesEnabled = true;
			this.dateTimePickerAdvEndSlotPeriod.Value = new System.DateTime(2009, 10, 28, 16, 45, 2, 975);
			// 
			// office2007OutlookTimePickerStartSlotPeriod
			// 
			this.office2007OutlookTimePickerStartSlotPeriod.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerStartSlotPeriod.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.office2007OutlookTimePickerStartSlotPeriod.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerStartSlotPeriod.Location = new System.Drawing.Point(210, 32);
			this.office2007OutlookTimePickerStartSlotPeriod.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerStartSlotPeriod.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerStartSlotPeriod.Name = "office2007OutlookTimePickerStartSlotPeriod";
			this.office2007OutlookTimePickerStartSlotPeriod.Size = new System.Drawing.Size(86, 23);
			this.office2007OutlookTimePickerStartSlotPeriod.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerStartSlotPeriod.TabIndex = 5;
			this.office2007OutlookTimePickerStartSlotPeriod.Text = "00:00";
			// 
			// office2007OutlookTimePickerEndSlotPeriod
			// 
			this.office2007OutlookTimePickerEndSlotPeriod.BackColor = System.Drawing.Color.White;
			this.office2007OutlookTimePickerEndSlotPeriod.BeforeTouchSize = new System.Drawing.Size(86, 23);
			this.office2007OutlookTimePickerEndSlotPeriod.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerEndSlotPeriod.Location = new System.Drawing.Point(210, 61);
			this.office2007OutlookTimePickerEndSlotPeriod.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.office2007OutlookTimePickerEndSlotPeriod.MinValue = System.TimeSpan.Parse("00:00:00");
			this.office2007OutlookTimePickerEndSlotPeriod.Name = "office2007OutlookTimePickerEndSlotPeriod";
			this.office2007OutlookTimePickerEndSlotPeriod.Size = new System.Drawing.Size(86, 23);
			this.office2007OutlookTimePickerEndSlotPeriod.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.office2007OutlookTimePickerEndSlotPeriod.TabIndex = 7;
			this.office2007OutlookTimePickerEndSlotPeriod.Text = "00:00";
			// 
			// buttonAdvNext
			// 
			this.buttonAdvNext.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonAdvNext.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvNext.BeforeTouchSize = new System.Drawing.Size(34, 22);
			this.buttonAdvNext.Enabled = false;
			this.buttonAdvNext.ForeColor = System.Drawing.Color.White;
			this.buttonAdvNext.IsBackStageButton = false;
			this.buttonAdvNext.Location = new System.Drawing.Point(426, 32);
			this.buttonAdvNext.Name = "buttonAdvNext";
			this.buttonAdvNext.Size = new System.Drawing.Size(34, 22);
			this.buttonAdvNext.TabIndex = 10;
			this.buttonAdvNext.Text = ">>";
			this.buttonAdvNext.UseVisualStyle = true;
			this.buttonAdvNext.Click += new System.EventHandler(this.buttonAdvNextClick);
			// 
			// buttonAdvPrevious
			// 
			this.buttonAdvPrevious.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.buttonAdvPrevious.Appearance = Syncfusion.Windows.Forms.ButtonAppearance.Metro;
			this.buttonAdvPrevious.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
			this.buttonAdvPrevious.BeforeTouchSize = new System.Drawing.Size(34, 22);
			this.buttonAdvPrevious.Enabled = false;
			this.buttonAdvPrevious.ForeColor = System.Drawing.Color.White;
			this.buttonAdvPrevious.IsBackStageButton = false;
			this.buttonAdvPrevious.Location = new System.Drawing.Point(303, 32);
			this.buttonAdvPrevious.Name = "buttonAdvPrevious";
			this.buttonAdvPrevious.Size = new System.Drawing.Size(34, 22);
			this.buttonAdvPrevious.TabIndex = 8;
			this.buttonAdvPrevious.Text = "<<";
			this.buttonAdvPrevious.UseVisualStyle = true;
			this.buttonAdvPrevious.Click += new System.EventHandler(this.buttonAdvPreviousClick);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
			this.panel1.Controls.Add(this.tabControlSkillResultGrid);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(3, 26);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(794, 481);
			this.panel1.TabIndex = 4;
			// 
			// tabControlSkillResultGrid
			// 
			this.tabControlSkillResultGrid.ActiveTabColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
			this.tabControlSkillResultGrid.ActiveTabFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControlSkillResultGrid.BackColor = System.Drawing.Color.White;
			this.tabControlSkillResultGrid.BeforeTouchSize = new System.Drawing.Size(794, 481);
			this.tabControlSkillResultGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tabControlSkillResultGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControlSkillResultGrid.Location = new System.Drawing.Point(0, 0);
			this.tabControlSkillResultGrid.Name = "tabControlSkillResultGrid";
			this.tabControlSkillResultGrid.Size = new System.Drawing.Size(794, 481);
			this.tabControlSkillResultGrid.TabIndex = 2;
			this.tabControlSkillResultGrid.TabPanelBackColor = System.Drawing.Color.White;
			this.tabControlSkillResultGrid.TabStop = false;
			this.tabControlSkillResultGrid.TabStyle = typeof(Syncfusion.Windows.Forms.Tools.TabRendererMetro);
			// 
			// gradientPanel1
			// 
			this.gradientPanel1.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanel1.Controls.Add(this.tableLayoutPanel2);
			this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanel1.Location = new System.Drawing.Point(0, 510);
			this.gradientPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanel1.Name = "gradientPanel1";
			this.gradientPanel1.Size = new System.Drawing.Size(332, 90);
			this.gradientPanel1.TabIndex = 5;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 140F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
			this.tableLayoutPanel2.Controls.Add(this.outlookTimePickerEndTime, 2, 2);
			this.tableLayoutPanel2.Controls.Add(this.outlookTimePickerStartTime, 2, 1);
			this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdvEndDate, 1, 2);
			this.tableLayoutPanel2.Controls.Add(this.dateTimePickerAdvStartDate, 1, 1);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelEndTime, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.autoLabelStartTime, 0, 1);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(328, 86);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// outlookTimePickerEndTime
			// 
			this.outlookTimePickerEndTime.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerEndTime.BeforeTouchSize = new System.Drawing.Size(74, 23);
			this.outlookTimePickerEndTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Location = new System.Drawing.Point(251, 61);
			this.outlookTimePickerEndTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerEndTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerEndTime.Name = "outlookTimePickerEndTime";
			this.outlookTimePickerEndTime.Size = new System.Drawing.Size(74, 23);
			this.outlookTimePickerEndTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerEndTime.TabIndex = 3;
			this.outlookTimePickerEndTime.Text = "00:00";
			// 
			// outlookTimePickerStartTime
			// 
			this.outlookTimePickerStartTime.BackColor = System.Drawing.Color.White;
			this.outlookTimePickerStartTime.BeforeTouchSize = new System.Drawing.Size(74, 23);
			this.outlookTimePickerStartTime.BindableTimeValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Location = new System.Drawing.Point(251, 32);
			this.outlookTimePickerStartTime.MaxValue = System.TimeSpan.Parse("2.00:00:00");
			this.outlookTimePickerStartTime.MinValue = System.TimeSpan.Parse("00:00:00");
			this.outlookTimePickerStartTime.Name = "outlookTimePickerStartTime";
			this.outlookTimePickerStartTime.Size = new System.Drawing.Size(74, 23);
			this.outlookTimePickerStartTime.Style = Syncfusion.Windows.Forms.VisualStyle.Metro;
			this.outlookTimePickerStartTime.TabIndex = 2;
			this.outlookTimePickerStartTime.Text = "00:00";
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
			this.dateTimePickerAdvEndDate.Location = new System.Drawing.Point(111, 61);
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
			// dateTimePickerAdvStartDate
			// 
			this.dateTimePickerAdvStartDate.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.dateTimePickerAdvStartDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			// 
			// 
			// 
			this.dateTimePickerAdvStartDate.Calendar.AllowMultipleSelection = false;
			this.dateTimePickerAdvStartDate.Calendar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(211)))), ((int)(((byte)(212)))));
			this.dateTimePickerAdvStartDate.Calendar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
			this.dateTimePickerAdvStartDate.Location = new System.Drawing.Point(111, 32);
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
			// 
			// autoLabelEndTime
			// 
			this.autoLabelEndTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelEndTime.Location = new System.Drawing.Point(3, 64);
			this.autoLabelEndTime.Name = "autoLabelEndTime";
			this.autoLabelEndTime.Size = new System.Drawing.Size(102, 15);
			this.autoLabelEndTime.TabIndex = 0;
			this.autoLabelEndTime.Text = "xxEndTimeColon";
			// 
			// autoLabelStartTime
			// 
			this.autoLabelStartTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.autoLabelStartTime.Location = new System.Drawing.Point(3, 36);
			this.autoLabelStartTime.Name = "autoLabelStartTime";
			this.autoLabelStartTime.Size = new System.Drawing.Size(102, 15);
			this.autoLabelStartTime.TabIndex = 0;
			this.autoLabelStartTime.Text = "xxStartTimeColon";
			// 
			// gradientPanelTop
			// 
			this.gradientPanelTop.Border3DStyle = System.Windows.Forms.Border3DStyle.Flat;
			this.gradientPanelTop.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tableLayoutPanel1.SetColumnSpan(this.gradientPanelTop, 2);
			this.gradientPanelTop.Controls.Add(this.progressBarLoading);
			this.gradientPanelTop.Controls.Add(this.autoLabelMeetingDate);
			this.gradientPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gradientPanelTop.Location = new System.Drawing.Point(0, 0);
			this.gradientPanelTop.Margin = new System.Windows.Forms.Padding(0);
			this.gradientPanelTop.Name = "gradientPanelTop";
			this.gradientPanelTop.Size = new System.Drawing.Size(800, 23);
			this.gradientPanelTop.TabIndex = 7;
			// 
			// progressBarLoading
			// 
			this.progressBarLoading.BackColor = System.Drawing.Color.White;
			this.progressBarLoading.Dock = System.Windows.Forms.DockStyle.Fill;
			this.progressBarLoading.Location = new System.Drawing.Point(0, 0);
			this.progressBarLoading.Name = "progressBarLoading";
			this.progressBarLoading.Size = new System.Drawing.Size(800, 23);
			this.progressBarLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBarLoading.TabIndex = 5;
			this.toolTip1.SetToolTip(this.progressBarLoading, "xxLoading");
			this.progressBarLoading.UseWaitCursor = true;
			this.progressBarLoading.Visible = false;
			// 
			// autoLabelMeetingDate
			// 
			this.autoLabelMeetingDate.AutoSize = false;
			this.autoLabelMeetingDate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoLabelMeetingDate.Location = new System.Drawing.Point(0, 0);
			this.autoLabelMeetingDate.Name = "autoLabelMeetingDate";
			this.autoLabelMeetingDate.Size = new System.Drawing.Size(800, 23);
			this.autoLabelMeetingDate.TabIndex = 0;
			this.autoLabelMeetingDate.Text = "Date";
			this.autoLabelMeetingDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// imageListSkillTypeIcons
			// 
			this.imageListSkillTypeIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageListSkillTypeIcons.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListSkillTypeIcons.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// MeetingImpactView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.Name = "MeetingImpactView";
			this.Size = new System.Drawing.Size(800, 600);
			this.tableLayoutPanel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel2)).EndInit();
			this.gradientPanel2.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartSlotPeriod.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartSlotPeriod)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndSlotPeriod.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndSlotPeriod)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerStartSlotPeriod)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.office2007OutlookTimePickerEndSlotPeriod)).EndInit();
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tabControlSkillResultGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanel1)).EndInit();
			this.gradientPanel1.ResumeLayout(false);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerEndTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.outlookTimePickerStartTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvEndDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate.Calendar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dateTimePickerAdvStartDate)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gradientPanelTop)).EndInit();
			this.gradientPanelTop.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Panel panel1;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel2;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelStartTime;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelEndTime;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvStartDate;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvEndDate;
		private Common.Controls.Office2007OutlookTimePicker outlookTimePickerStartTime;
		private Common.Controls.Office2007OutlookTimePicker outlookTimePickerEndTime;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvPickBest;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel3;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel1;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabel2;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvStartSlotPeriod;
		private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dateTimePickerAdvEndSlotPeriod;
		private Common.Controls.Office2007OutlookTimePicker office2007OutlookTimePickerStartSlotPeriod;
		private Common.Controls.Office2007OutlookTimePicker office2007OutlookTimePickerEndSlotPeriod;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvNext;
		private Syncfusion.Windows.Forms.ButtonAdv buttonAdvPrevious;
		private Syncfusion.Windows.Forms.Tools.GradientPanel gradientPanelTop;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelMeetingDate;
		private Syncfusion.Windows.Forms.Tools.AutoLabel autoLabelPickResult;
		private Syncfusion.Windows.Forms.Tools.TabControlAdv tabControlSkillResultGrid;
        private System.Windows.Forms.ImageList imageListSkillTypeIcons;
        private System.Windows.Forms.ProgressBar progressBarLoading;
	}
}
