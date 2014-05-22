using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls.Cells;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Panels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Meetings
{
	 public partial class MeetingSchedulesView : BaseUserControl, IMeetingSchedulesView
	 {
		 private readonly INotifyComposerMeetingChanged _composer;
		 private readonly MeetingSchedulesPresenter _presenter;
		 private IList<DateOnly> _recurringDates;
		 private int _suggestionRows;
		 private IList<DateOnly> _availableDates;
		 private readonly ISchedulerStateHolder _stateHolder;
		 private bool _timeFocused;

		 protected MeetingSchedulesView()
		 {
			 InitializeComponent();
			 if (DesignMode) return;

			 outlookTimePickerStartTime.CreateAndBindList();
			 outlookTimePickerEndTime.CreateAndBindList();

			 office2007OutlookTimePickerStartSpan.CreateAndBindList();
			 office2007OutlookTimePickerEndSpan.CreateAndBindList();

			 outlookTimePickerStartTime.MaxValue = TimeSpan.FromDays(1);
			 outlookTimePickerEndTime.MaxValue = TimeSpan.FromDays(1);

			 office2007OutlookTimePickerEndSpan.MaxValue = TimeSpan.FromDays(1);
			 office2007OutlookTimePickerStartSpan.MaxValue = TimeSpan.FromDays(1);

			 dateTimePickerAdvEndDate.SetCultureInfoSafe(CultureInfo.CurrentCulture);
			 dateTimePickerAdvStartDate.SetCultureInfoSafe(CultureInfo.CurrentCulture);

			 dateTimePickerAdvStartDate.SetSafeBoundary();
			 dateTimePickerAdvEndDate.SetSafeBoundary();

			 dateTimePickerAdvStartDate.ValueChanged += dateTimePickerAdvStartDate_ValueChanged;

			 gridControlSchedules.HScrollPixel = true;
			 gridControlSchedules.MouseUp += GridControlSchedulesMouseUp;
			 gridControlSchedules.MouseDown += GridControlSchedulesMouseDown;

			 if (!gridControlSchedules.CellModels.ContainsKey("TimeLineHeaderCell"))
				 gridControlSchedules.CellModels.Add("TimeLineHeaderCell",
				                                     new VisualProjectionColumnHeaderCellModel(gridControlSchedules.Model,
				                                                                               TimeZoneHelper.CurrentSessionTimeZone));
			 if (!gridControlSchedules.CellModels.ContainsKey("ScheduleCell"))
				 gridControlSchedules.CellModels.Add("ScheduleCell", new ScheduleCellModel(gridControlSchedules.Model));

			 BindEvents();
		 }

		public void BindEvents()
		{
				outlookTimePickerStartTime.Leave += OutlookTimePickerStartTimeLeave;
				outlookTimePickerEndTime.Leave += OutlookTimePickerEndTimeLeave;
				outlookTimePickerStartTime.KeyDown += OutlookTimePickerStartTimeKeyDown;
				outlookTimePickerEndTime.KeyDown += OutlookTimePickerEndTimeKeyDown;
				outlookTimePickerStartTime.SelectedIndexChanged += OutlookTimePickerStartTimeSelectedIndexChanged;
				outlookTimePickerEndTime.SelectedIndexChanged += OutlookTimePickerEndTimeSelectedIndexChanged;
		 }

		public void UnBindEvents()
		{
			outlookTimePickerStartTime.Leave -= OutlookTimePickerStartTimeLeave;
			outlookTimePickerEndTime.Leave -= OutlookTimePickerEndTimeLeave;
			outlookTimePickerStartTime.KeyDown -= OutlookTimePickerStartTimeKeyDown;
			outlookTimePickerEndTime.KeyDown -= OutlookTimePickerEndTimeKeyDown;
			outlookTimePickerStartTime.SelectedIndexChanged -= OutlookTimePickerStartTimeSelectedIndexChanged;
			outlookTimePickerEndTime.SelectedIndexChanged -= OutlookTimePickerEndTimeSelectedIndexChanged;
		}

		public bool IsRightToLeft
		{
			get { return gridControlSchedules.IsRightToLeft(); }
		}

		public bool TimeFocused
		{
			get { return outlookTimePickerStartTime.Focused || outlookTimePickerEndTime.Focused || _timeFocused; }
			set { _timeFocused = value; }
		}

		public void SetSizeWECursor()
		  {
				gridControlSchedules.Cursor = Cursors.SizeWE;
		  }

		  public void SetDefaultCursor()
		  {
				gridControlSchedules.Cursor = Cursors.Default;   
		  }

		  public void SetHandCursor()
		  {
				gridControlSchedules.Cursor = Cursors.Hand;
		  }

		  public void RefreshGridSchedules()
		  {
				gridControlSchedules.Refresh();
		  }

		  void GridControlSchedulesMouseDown(object sender, MouseEventArgs e)
		  {
				_presenter.GridControlSchedulesMouseDown();
		  }

		  void GridControlSchedulesMouseUp(object sender, MouseEventArgs e)
		  {
				_presenter.GridControlSchedulesMouseUp();
		  }

		  private void GridControlSchedulesMouseMove(object sender, MouseEventArgs e)
		  {
				var cellRect = gridControlSchedules.RangeInfoToRectangle(GridRangeInfo.Cell(0, 1));
				var cellWidth = gridControlSchedules.ColWidths[1];
				var mousePos = cellWidth - cellRect.Width - cellRect.X + e.X;
				var cellPosX = cellRect.X - (cellWidth - cellRect.Width);
				
				if(gridControlSchedules.IsRightToLeft())
				{
					 mousePos = cellRect.Width - (cellRect.Width - Math.Abs(cellRect.X) - e.X);
					 cellRect = new Rectangle(cellRect.X + 1, 0, cellWidth, 0);   
				}
				else
				{
					 cellRect = new Rectangle(cellPosX, 0, cellWidth, 0);   
				}

				var timePeriod = _presenter.MergedOrDefaultPeriod();
				var pixelConverter = new LengthToTimeCalculator(timePeriod, cellWidth);

				_presenter.GridControlSchedulesMouseMove(e.X, cellRect, pixelConverter, mousePos);    
		  }

		  public void ScrollMeetingIntoView()
		  {
				gridControlSchedules.BeginUpdate();

				var minScroll = gridControlSchedules.GetHScrollPixelMinimum();
				gridControlSchedules.SetCurrentHScrollPixelPos(minScroll);

				var period = _presenter.Model.Meeting.MeetingPeriod(_presenter.Model.StartDate);
				var cellRect = gridControlSchedules.RangeInfoToRectangle(GridRangeInfo.Cell(0, 1));
				var cellWidth = gridControlSchedules.ColWidths[1];
	  
				if(gridControlSchedules.IsRightToLeft()) cellRect = new Rectangle(cellRect.X + 1, 0, cellWidth, 0);   
				else
				{
					 var cellPosX = cellRect.X - (cellWidth - cellRect.Width);
					 cellRect = new Rectangle(cellPosX, 0, cellWidth, 0);   
				}
				
				var timePeriod = _presenter.MergedOrDefaultPeriod();
				var pixelConverter = new LengthToTimeCalculator(timePeriod, cellWidth);
				var meetingRect = _presenter.GetLayerRectangle(pixelConverter, period, cellRect);
				var clientMiddle = (gridControlSchedules.ClientRectangle.Width - minScroll) / 2;

				if(gridControlSchedules.IsRightToLeft())
				{
					 if (meetingRect.X >= 0) clientMiddle *= -1;
					 gridControlSchedules.SetCurrentHScrollPixelPos(Math.Abs((int) meetingRect.X) + clientMiddle);
				}
				else gridControlSchedules.SetCurrentHScrollPixelPos((int)meetingRect.X - clientMiddle);   
			  
				gridControlSchedules.EndUpdate();
		  }

		  public MeetingSchedulesView(IMeetingViewModel meetingViewModel, ISchedulerStateHolder schedulerStateHolder, INotifyComposerMeetingChanged composer) : this()
		  {
				if(schedulerStateHolder == null) throw new ArgumentNullException("schedulerStateHolder");

				_stateHolder = schedulerStateHolder;
				if (composer == null) throw new ArgumentNullException("composer");
				_composer = composer;
				if (DesignMode) return;

				monthCalendarAdvDateSelection.Culture = CultureInfo.CurrentCulture;
				var stateHolderLoader = new SchedulerStateLoader(schedulerStateHolder);
				var meetingMover = new MeetingMover(this, meetingViewModel, schedulerStateHolder.DefaultSegmentLength, TeleoptiPrincipal.Current.Regional.UICulture.TextInfo.IsRightToLeft);
				var meetingMousePositionDecider = new MeetingMousePositionDecider(this);
				_presenter = new MeetingSchedulesPresenter(this, meetingViewModel, schedulerStateHolder, new RangeProjectionService(), stateHolderLoader, new MeetingSlotFinderService(), meetingMover, meetingMousePositionDecider);

				SetTexts();
				SetTimes();

				GridHelper.GridStyle(gridControlSchedules);
				gridControlSchedules.AllowSelection = GridSelectionFlags.Row;
				gridControlSchedules.ShowCurrentCellBorderBehavior = GridShowCurrentCellBorder.HideAlways;
				gridControlSchedules.ExcelLikeSelectionFrame = false;
				gridControlSchedules.ExcelLikeCurrentCell = false;
		  }

		  private void GridControlSchedulesQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		  {
				if (e.RowIndex == 0 && e.ColIndex == 1)
				{
					 e.Style.CellType = "TimeLineHeaderCell";
					 e.Style.Text = string.Empty;
					 e.Style.WrapText = false;
					 e.Style.Tag = _presenter.MergedOrDefaultPeriod();
				}
	 
				if (e.RowIndex > _presenter.ParticipantList.Count) return;
				if (e.RowIndex > 0 && e.ColIndex == 0) e.Style.Text = _presenter.ParticipantList[e.RowIndex - 1].FullName;
				if (e.RowIndex <= 0 || e.ColIndex <= 0) return;

				var agent = (_presenter.ParticipantList.ElementAt(e.RowIndex - 1)).ContainedEntity;
				var utcDate = _presenter.Model.StartDate;
				var totalScheduleRange = _stateHolder.Schedules[agent];
				var scheduleDay = totalScheduleRange.ScheduledDay(utcDate);
				var scheduleBefore = totalScheduleRange.ScheduledDay(utcDate.AddDays(-1));
				var scheduleAfter = totalScheduleRange.ScheduledDay(utcDate.AddDays(1));
				var scheduleDisplayRow = new ScheduleDisplayRow{ScheduleDay = scheduleDay, ScheduleDayBefore = scheduleBefore, ScheduleDayAfter = scheduleAfter};
				scheduleDisplayRow.MeetingPeriod = _presenter.Model.Meeting.MeetingPeriod(_presenter.Model.StartDate);

				e.Style.CellType = "ScheduleCell";
				e.Style.Tag = _presenter.MergedOrDefaultPeriod();
				e.Style.CellValue = scheduleDisplayRow;
		  }

		  private void SetTimes()
		  {
				office2007OutlookTimePickerStartSpan.SelectedIndex = 16;
				office2007OutlookTimePickerEndSpan.SelectedIndex = 34;
		  }

		  protected override void SetCommonTexts()
		  {
				base.SetCommonTexts();
				dateTimePickerAdvEndDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
				dateTimePickerAdvStartDate.Calendar.TodayButton.Text = UserTexts.Resources.Today;
				monthCalendarAdvDateSelection.TodayButton.Text = UserTexts.Resources.Today;
		  }

		  protected override void OnLoad(EventArgs e)
		  {
				base.OnLoad(e);
				if (DesignMode) return;

				_presenter.Initialize();

				//gridControlSchedules.RowCount = _presenter.RowCount;
				//gridControlSchedules.ColCount = 1;
				gridControlSchedules.AllowScrollCurrentCellInView = GridScrollCurrentCellReason.None;

				ScrollMeetingIntoView();
		  }

		  private void RefreshUpdate()
		  {
				gridControlSchedules.BeginUpdate();
				gridControlSchedules.RowCount = _presenter.RowCount;
				gridControlSchedules.EndUpdate();
		  }

		  public void OnParticipantsSet()
		  {
				gridControlSchedules.RowCount = 0;
				_presenter.OnParticipantsSet();
			gridControlSchedules.RowCount = _presenter.RowCount;
		  }

		  public void OnDisableWhileLoadingStateHolder()
		  {
		  }

		  public void OnEnableAfterLoadingStateHolder()
		  {
		  }

		  public void OnMeetingDatesChanged()
		  {
				if (!_presenter.IsInitialized) return;
			SetStartDate(_presenter.Model.StartDate);
		  }

		  public void OnMeetingTimeChanged()
		  {
				if (!_presenter.IsInitialized) return;
				SetStartTime(_presenter.Model.StartTime);
			SetEndTime(_presenter.Model.EndTime);
				SetEndDate(_presenter.Model.EndDate);
		  }

		public IMeetingDetailPresenter Presenter
		{
			get { return _presenter; }
		}

		 public void ResetSelection()
		 {
			 gridControlSchedules.Select();
		 }

		 public void SetStartDate(DateOnly startDate)
		  {
				dateTimePickerAdvStartDate.Value = startDate;
		  }

		  public void SetEndDate(DateOnly endDate)
		  {
				dateTimePickerAdvEndDate.Value = endDate;
		  }

		  public void SetStartTime(TimeSpan startTime)
		  {
				outlookTimePickerStartTime.SetTimeValue(startTime);
				ScrollMeetingIntoView();
		  }

		  public void SetEndTime(TimeSpan endTime)
		  {
				outlookTimePickerEndTime.SetTimeValue(endTime);
				ScrollMeetingIntoView();
		  }

		  public TimeSpan SetSuggestListStartTime
		  {
				get { return office2007OutlookTimePickerStartSpan.TimeValue(); }
				set { office2007OutlookTimePickerStartSpan.SelectedValue = value; }
		  }

		  public TimeSpan SetSuggestListEndTime
		  {
				get { return office2007OutlookTimePickerEndSpan.TimeValue(); }
				set { office2007OutlookTimePickerEndSpan.SelectedValue = value; }
		  }

		public string GetStartTimeText
		{
			get { return outlookTimePickerStartTime.Text; }
		}

		public string GetEndTimeText
		{
			get { return outlookTimePickerEndTime.Text; }
		}

		public void SetCurrentDate(DateOnly currentDate)
		  {
				monthCalendarAdvDateSelection.Value = currentDate;
		  }

		  public void RefreshGrid()
		  {
				Invalidate(true);
				RefreshUpdate();

				gridControlSuggestions.Refresh();
				monthCalendarAdvDateSelection.Refresh();
				autoLabel2.Refresh();
				autoLabelStartSpan.Refresh();
				autoLabelEndSpan.Refresh();
				office2007OutlookTimePickerStartSpan.Refresh();
				office2007OutlookTimePickerEndSpan.Refresh();
		  }

		  public void SetRecurringDates(IList<DateOnly> recurringDates)
		  {
				_recurringDates = recurringDates;
				monthCalendarAdvDateSelection.RefreshCalendar(true);
		  }

		  private void MonthCalendarAdvDateSelectionDateCellQueryInfo(object sender, DateCellQueryInfoEventArgs e)
		  {
				var date = new DateOnly(monthCalendarAdvDateSelection.Value);
				var week = new DateOnlyPeriod();
				if (_availableDates != null)
				{
					 week = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
				}
				if (_recurringDates == null)
				{
					 if ((e.DateValue != null) && (_availableDates != null))
					 {
						  MarkAvailableDate(e);
					 }
					 DrawWeekFrame(e, week);
				}
				else if (!_recurringDates.Contains(new DateOnly((DateTime)e.DateValue)) || !_presenter.Model.IsRecurring)
				{
					 if ((e.DateValue != null) && (_availableDates != null) && (_availableDates.Count > 0))
					 {
						  MarkAvailableDate(e);
					 }
					 DrawWeekFrame(e, week);
				}
				else
				{
					 e.Style.BackColor = Color.FromArgb(63, Color.ForestGreen);
					 e.Style.Borders.All = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.Thin);
					 e.Style.CellTipText = UserTexts.Resources.RecurrentMeeting;
					 _availableDates = null;
				}
		  }

		  private void MarkAvailableDate(DateCellQueryInfoEventArgs e)
		  {
				if (!_availableDates.Contains(new DateOnly((DateTime)e.DateValue))) return;
				e.Style.BackColor = Color.LightSkyBlue;
				e.Style.CellTipText = UserTexts.Resources.Available;
		  }

		  private static void DrawWeekFrame(DateCellQueryInfoEventArgs e, DateOnlyPeriod week)
		  {
				if (e.DateValue == null) return;
				if (!week.Contains(new DateOnly((DateTime)e.DateValue))) return;
				var frameStyle = new GridBorder(GridBorderStyle.Solid, Color.Gray, GridBorderWeight.Thin);
				e.Style.Borders.Top = frameStyle;
				e.Style.Borders.Bottom = frameStyle;
				if (week.StartDate == (DateTime)e.DateValue)
				{
					 e.Style.Borders.Left = frameStyle;
				}
				if (week.EndDate == (DateTime)e.DateValue)
				{
					 e.Style.Borders.Right = frameStyle;
				}
		  }

		  private void MonthCalendarAdvDateSelectionDateSelected(object sender, EventArgs e)
		  {
				var theDate = new DateOnly(monthCalendarAdvDateSelection.Value);
				if (!_presenter.Model.IsRecurring)
				{
					 _presenter.SetStartDateFromCurrentDate(theDate);
				}
				_presenter.SetCurrentDate(theDate);
				OnMeetingDatesChanged();
				NotifyMeetingDatesChanged();
				FindAvailableDays();
				monthCalendarAdvDateSelection.RefreshCalendar(true);
				gridControlSuggestions.Refresh();
		  }

		  private void GridControlSchedulesQueryColCount(object sender, GridRowColCountEventArgs e)
		  {
				e.Count = 1;
				e.Handled = true;
		  }

		  private void GridControlSchedulesQueryRowCount(object sender, GridRowColCountEventArgs e)
		  {
				if (_presenter == null) return;
				e.Count = _presenter.RowCount;
				e.Handled = true;
		  }

		  

		  private void GridControlSchedulesScrollPixelPosChanged(object sender, GridScrollPositionChangedEventArgs e)
		  {
				if (_presenter == null) return;
		  }

		  private void GridControlSuggestionsQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		  {
				if (_presenter == null || gridControlSuggestions.IsMousePressed) return;

				_suggestionRows = _presenter.SuggestionsRowCount;

				IList<TimePeriod> timeList = new List<TimePeriod>();
				if (_suggestionRows > 0)
				{
					 timeList = _presenter.SuggestionList;
					 _suggestionRows = timeList.Count;
					 gridControlSuggestions.RowCount = _suggestionRows;
				}

				var i = e.RowIndex;

				if (e.ColIndex == 0 && e.RowIndex == 0)
				{
					 if (_suggestionRows > 0)
					 { e.Style.CellValue = UserTexts.Resources.SuggestedTimesColon; }
					 else
					 {
						  e.Style.CellValue = !_presenter.Model.IsRecurring ? UserTexts.Resources.NoSuggestions : UserTexts.Resources.FunctionNotAvailable;
					 }
				}
				else if (e.ColIndex == 0 && e.RowIndex > 0)
				{
					 if (i <= timeList.Count)
					 {
						  var tp = timeList[i - 1].ToShortTimeString(CultureInfo.CurrentCulture);
						  e.Style.CellValue = tp;
					 }
				}
		  }

		  private void GridControlSuggestionsSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		  {
				if (gridControlSuggestions.IsMousePressed) return;
				string t = gridControlSuggestions[e.Range.Top, 0].CellValue.ToString();
				TimePeriod result;
				if (!TimePeriod.TryParse(t, out result)) return;

				var startTime = result.StartTime;
			var endTime = result.EndTime;

				outlookTimePickerStartTime.SelectedIndexChanged -= OutlookTimePickerStartTimeSelectedIndexChanged;
				outlookTimePickerEndTime.SelectedIndexChanged -= OutlookTimePickerEndTimeSelectedIndexChanged;
				_presenter.SetTimesFromEditor(startTime, endTime);
				RefreshGrid();
				outlookTimePickerStartTime.SelectedIndexChanged += OutlookTimePickerStartTimeSelectedIndexChanged;
				outlookTimePickerEndTime.SelectedIndexChanged += OutlookTimePickerEndTimeSelectedIndexChanged;
		  }

		  private void FindAvailableDays()
		  {
				_availableDates = _presenter.GetAvailableDays;
		  }

		  private void GridControlSuggestionsQueryRowCount(object sender, GridRowColCountEventArgs e)
		  {
				if (_presenter == null || gridControlSuggestions.IsMousePressed) return;
				_suggestionRows = _presenter.SuggestionsRowCount;
				e.Count = _suggestionRows;
				e.Handled = true;
		  }

		  private void Office2007OutlookTimePickerSpanTextChanged(object sender, EventArgs e)
		  {
				if (_presenter == null || !_presenter.IsInitialized) return;
				gridControlSuggestions.Refresh();
				FindAvailableDays();
				monthCalendarAdvDateSelection.RefreshCalendar(true);
		  }

		private void DateTimePickerAdvStartDatePopupClosed(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
		{
			 DateIsChanged();
		}	

		  private void DateIsChanged()
		  {
				var theDate = new DateOnly(dateTimePickerAdvStartDate.Value);
				if (!_presenter.Model.IsRecurring)
				{
					 _presenter.SetStartDateFromCurrentDate(theDate);
				}
				_presenter.SetCurrentDate(theDate);
				SetCurrentDate(theDate);
				OnMeetingDatesChanged();
				NotifyMeetingDatesChanged();
				FindAvailableDays();
				monthCalendarAdvDateSelection.RefreshCalendar(true);
				gridControlSuggestions.Refresh();
		  }
		void OutlookTimePickerStartTimeLeave(object sender, EventArgs e)
		{
				if (_presenter == null) return;
				if (outlookTimePickerStartTime.Disposing)
					 return;
			
			_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Text);
			gridControlSuggestions.Refresh();
		}

		void OutlookTimePickerEndTimeLeave(object sender, EventArgs e)
		{
				if (_presenter == null) return;
				if (outlookTimePickerEndTime.Disposing)
					 return;
			
			_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Text);
			gridControlSuggestions.Refresh();
		}

		void OutlookTimePickerStartTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerStartTimeKeyDown(e.KeyCode, outlookTimePickerStartTime.Text);
		}

		void OutlookTimePickerEndTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerEndTimeKeyDown(e.KeyCode, outlookTimePickerEndTime.Text);
		}

		void OutlookTimePickerStartTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Text);
			gridControlSchedules.Refresh();
			gridControlSuggestions.Refresh();      
		}

		void OutlookTimePickerEndTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Text);
			gridControlSchedules.Refresh();
			gridControlSuggestions.Refresh();
		}

		public void NotifyMeetingDatesChanged()
		{
			_composer.NotifyMeetingDatesChanged(this);
		}

		public void NotifyMeetingTimeChanged()
		{
			//_composer.NotifyMeetingTimeChanged(this);
		}

		  private void dateTimePickerAdvStartDate_ValueChanged(object sender, EventArgs e)
		  {
				DateIsChanged();
		  }

		  private void MeetingSchedulesView_Resize(object sender, EventArgs e)
		  {
				gridControlSchedules.Refresh();
		  }

		  private void gridControlSuggestionsMouseUp(object sender, MouseEventArgs e)
		  {
				gridControlSuggestions.Refresh();
		  }
	 }
}
