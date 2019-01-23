using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Meetings
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

			dateTimePickerAdvStartDate.ValueChanged += dateTimePickerAdvStartDateValueChanged;

			gridControlSchedules.HScrollPixel = true;
			gridControlSchedules.MouseUp += gridControlSchedulesMouseUp;
			gridControlSchedules.MouseDown += gridControlSchedulesMouseDown;

			if (!gridControlSchedules.CellModels.ContainsKey("TimeLineHeaderCell"))
				gridControlSchedules.CellModels.Add("TimeLineHeaderCell",
													new VisualProjectionColumnHeaderCellModel(gridControlSchedules.Model,
																							  TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));
			if (!gridControlSchedules.CellModels.ContainsKey("ScheduleCell"))
				gridControlSchedules.CellModels.Add("ScheduleCell", new ScheduleCellModel(gridControlSchedules.Model));

			BindEvents();
		}

		public void BindEvents()
		{
			outlookTimePickerStartTime.Leave += outlookTimePickerStartTimeLeave;
			outlookTimePickerEndTime.Leave += outlookTimePickerEndTimeLeave;
			outlookTimePickerStartTime.KeyDown += outlookTimePickerStartTimeKeyDown;
			outlookTimePickerEndTime.KeyDown += outlookTimePickerEndTimeKeyDown;
			outlookTimePickerStartTime.SelectedIndexChanged += outlookTimePickerStartTimeSelectedIndexChanged;
			outlookTimePickerEndTime.SelectedIndexChanged += outlookTimePickerEndTimeSelectedIndexChanged;
		}

		public void UnBindEvents()
		{
			outlookTimePickerStartTime.Leave -= outlookTimePickerStartTimeLeave;
			outlookTimePickerEndTime.Leave -= outlookTimePickerEndTimeLeave;
			outlookTimePickerStartTime.KeyDown -= outlookTimePickerStartTimeKeyDown;
			outlookTimePickerEndTime.KeyDown -= outlookTimePickerEndTimeKeyDown;
			outlookTimePickerStartTime.SelectedIndexChanged -= outlookTimePickerStartTimeSelectedIndexChanged;
			outlookTimePickerEndTime.SelectedIndexChanged -= outlookTimePickerEndTimeSelectedIndexChanged;
		}

		public bool IsRightToLeft => gridControlSchedules.IsRightToLeft();

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

		void gridControlSchedulesMouseDown(object sender, MouseEventArgs e)
		{
			_presenter.GridControlSchedulesMouseDown();
		}

		void gridControlSchedulesMouseUp(object sender, MouseEventArgs e)
		{
			_presenter.GridControlSchedulesMouseUp();
		}

		private void gridControlSchedulesMouseMove(object sender, MouseEventArgs e)
		{
			var cellRect = gridControlSchedules.RangeInfoToRectangle(GridRangeInfo.Cell(0, 1));
			var cellWidth = gridControlSchedules.ColWidths[1];
			var mousePos = cellWidth - cellRect.Width - cellRect.X + e.X;
			var cellPosX = cellRect.X - (cellWidth - cellRect.Width);

			if (gridControlSchedules.IsRightToLeft())
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

			if (gridControlSchedules.IsRightToLeft()) cellRect = new Rectangle(cellRect.X + 1, 0, cellWidth, 0);
			else
			{
				var cellPosX = cellRect.X - (cellWidth - cellRect.Width);
				cellRect = new Rectangle(cellPosX, 0, cellWidth, 0);
			}

			var timePeriod = _presenter.MergedOrDefaultPeriod();
			var pixelConverter = new LengthToTimeCalculator(timePeriod, cellWidth);
			var meetingRect = _presenter.GetLayerRectangle(pixelConverter, period, cellRect);
			var clientMiddle = (gridControlSchedules.ClientRectangle.Width - minScroll) / 2;

			if (gridControlSchedules.IsRightToLeft())
			{
				if (meetingRect.X >= 0) clientMiddle *= -1;
				gridControlSchedules.SetCurrentHScrollPixelPos(Math.Abs((int)meetingRect.X) + clientMiddle);
			}
			else gridControlSchedules.SetCurrentHScrollPixelPos((int)meetingRect.X - clientMiddle);

			gridControlSchedules.EndUpdate();
		}

		public MeetingSchedulesView(IMeetingViewModel meetingViewModel, SchedulingScreenState schedulingScreenState, INotifyComposerMeetingChanged composer)
			: this()
		{
			if (schedulingScreenState == null) throw new ArgumentNullException(nameof(schedulingScreenState));

			_stateHolder = schedulingScreenState.SchedulerStateHolder;
			if (composer == null) throw new ArgumentNullException(nameof(composer));
			_composer = composer;
			if (DesignMode) return;

			monthCalendarAdvDateSelection.Culture = CultureInfo.CurrentCulture;
			var stateHolderLoader = new SchedulerStateLoader(schedulingScreenState, new RepositoryFactory(), UnitOfWorkFactory.Current, new LazyLoadingManagerWrapper(), new ScheduleStorageFactory(new PersonAssignmentRepository(CurrentUnitOfWork.Make())));
			var meetingMover = new MeetingMover(this, meetingViewModel, schedulingScreenState.DefaultSegmentLength, TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.TextInfo.IsRightToLeft);
			var meetingMousePositionDecider = new MeetingMousePositionDecider(this);
			_presenter = new MeetingSchedulesPresenter(this, meetingViewModel, schedulingScreenState.SchedulerStateHolder, stateHolderLoader, new MeetingSlotFinderService(UserTimeZone.Make()), meetingMover, meetingMousePositionDecider);

			SetTexts();
			setTimes();

			gridControlSchedules.AllowSelection = GridSelectionFlags.Row;
			gridControlSchedules.ShowCurrentCellBorderBehavior = GridShowCurrentCellBorder.HideAlways;
			gridControlSchedules.ExcelLikeSelectionFrame = false;
			gridControlSchedules.ExcelLikeCurrentCell = false;
		}

		private void gridControlSchedulesQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
		{
			if (e.RowIndex == 0 && e.ColIndex == 1)
			{
				e.Style.CellType = "TimeLineHeaderCell";
				e.Style.Text = string.Empty;
				e.Style.WrapText = false;
				e.Style.Tag = _presenter.MergedOrDefaultPeriod();
			}

			if (e.RowIndex > _presenter.RowCount) return;
			if (e.RowIndex > 0 && e.ColIndex == 0) e.Style.Text = _presenter.ParticipantList[e.RowIndex - 1].FullName;
			if (e.RowIndex <= 0 || e.ColIndex <= 0) return;

			var agent = (_presenter.ParticipantList.ElementAt(e.RowIndex - 1)).ContainedEntity;
			var utcDate = _presenter.Model.StartDate;
			var totalScheduleRange = _stateHolder.Schedules[agent];
			var scheduleDay = totalScheduleRange.ScheduledDay(utcDate);
			var scheduleBefore = totalScheduleRange.ScheduledDay(utcDate.AddDays(-1));
			var scheduleAfter = totalScheduleRange.ScheduledDay(utcDate.AddDays(1));
			var scheduleDisplayRow = new ScheduleDisplayRow { ScheduleDay = scheduleDay, ScheduleDayBefore = scheduleBefore, ScheduleDayAfter = scheduleAfter };
			scheduleDisplayRow.MeetingPeriod = _presenter.Model.Meeting.MeetingPeriod(_presenter.Model.StartDate);

			e.Style.CellType = "ScheduleCell";
			e.Style.Tag = _presenter.MergedOrDefaultPeriod();
			e.Style.CellValue = scheduleDisplayRow;
		}

		private void setTimes()
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

		private void refreshUpdate()
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

		public IMeetingDetailPresenter Presenter => _presenter;

		public void ResetSelection()
		{
			gridControlSchedules.Select();
		}

		public void SetStartDate(DateOnly startDate)
		{
			dateTimePickerAdvStartDate.Value = startDate.Date;
		}

		public void SetEndDate(DateOnly endDate)
		{
			dateTimePickerAdvEndDate.Value = endDate.Date;
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

		public string GetStartTimeText => outlookTimePickerStartTime.Text;

		public string GetEndTimeText => outlookTimePickerEndTime.Text;

		public void SetCurrentDate(DateOnly currentDate)
		{
			monthCalendarAdvDateSelection.Value = currentDate.Date;
		}

		public void RefreshGrid()
		{
			Invalidate(true);
			refreshUpdate();

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

		private void monthCalendarAdvDateSelectionDateCellQueryInfo(object sender, DateCellQueryInfoEventArgs e)
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
					markAvailableDate(e);
				}
				drawWeekFrame(e, week);
			}
			else if (!_recurringDates.Contains(new DateOnly((DateTime)e.DateValue)) || !_presenter.Model.IsRecurring)
			{
				if ((e.DateValue != null) && (_availableDates != null) && (_availableDates.Count > 0))
				{
					markAvailableDate(e);
				}
				drawWeekFrame(e, week);
			}
			else
			{
				e.Style.BackColor = Color.FromArgb(63, Color.ForestGreen);
				e.Style.Borders.All = new GridBorder(GridBorderStyle.Solid, Color.Black, GridBorderWeight.Thin);
				e.Style.CellTipText = UserTexts.Resources.RecurrentMeeting;
				_availableDates = null;
			}
		}

		private void markAvailableDate(DateCellQueryInfoEventArgs e)
		{
			if (!_availableDates.Contains(new DateOnly((DateTime)e.DateValue))) return;
			e.Style.BackColor = Color.LightSkyBlue;
			e.Style.CellTipText = UserTexts.Resources.Available;
		}

		private static void drawWeekFrame(DateCellQueryInfoEventArgs e, DateOnlyPeriod week)
		{
			if (e.DateValue == null) return;
			if (!week.Contains(new DateOnly((DateTime)e.DateValue))) return;
			var frameStyle = new GridBorder(GridBorderStyle.Solid, Color.Gray, GridBorderWeight.Thin);
			e.Style.Borders.Top = frameStyle;
			e.Style.Borders.Bottom = frameStyle;
			if (week.StartDate.Date == (DateTime)e.DateValue)
			{
				e.Style.Borders.Left = frameStyle;
			}
			if (week.EndDate.Date == (DateTime)e.DateValue)
			{
				e.Style.Borders.Right = frameStyle;
			}
		}

		private void monthCalendarAdvDateSelectionDateSelected(object sender, EventArgs e)
		{
			var theDate = new DateOnly(monthCalendarAdvDateSelection.Value);
			if (!_presenter.Model.IsRecurring)
			{
				_presenter.SetStartDateFromCurrentDate(theDate);
			}
			_presenter.SetCurrentDate(theDate);
			OnMeetingDatesChanged();
			NotifyMeetingDatesChanged();
			findAvailableDays();
			monthCalendarAdvDateSelection.RefreshCalendar(true);
			gridControlSuggestions.Refresh();
		}

		private void gridControlSchedulesQueryColCount(object sender, GridRowColCountEventArgs e)
		{
			e.Count = 1;
			e.Handled = true;
		}

		private void gridControlSchedulesQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			if (_presenter == null) return;
			e.Count = _presenter.RowCount;
			e.Handled = true;
		}

		private void gridControlSuggestionsQueryCellInfo(object sender, GridQueryCellInfoEventArgs e)
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

		private void gridControlSuggestionsSelectionChanged(object sender, GridSelectionChangedEventArgs e)
		{
			if (gridControlSuggestions.IsMousePressed) return;
			string t = gridControlSuggestions[e.Range.Top, 0].CellValue.ToString();
			TimePeriod result;
			if (!TimePeriod.TryParse(t, out result)) return;

			var startTime = result.StartTime;
			var endTime = result.EndTime;

			outlookTimePickerStartTime.SelectedIndexChanged -= outlookTimePickerStartTimeSelectedIndexChanged;
			outlookTimePickerEndTime.SelectedIndexChanged -= outlookTimePickerEndTimeSelectedIndexChanged;
			_presenter.SetTimesFromEditor(startTime, endTime);
			RefreshGrid();
			outlookTimePickerStartTime.SelectedIndexChanged += outlookTimePickerStartTimeSelectedIndexChanged;
			outlookTimePickerEndTime.SelectedIndexChanged += outlookTimePickerEndTimeSelectedIndexChanged;
		}

		private void findAvailableDays()
		{
			_availableDates = _presenter.GetAvailableDays;
		}

		private void gridControlSuggestionsQueryRowCount(object sender, GridRowColCountEventArgs e)
		{
			if (_presenter == null || gridControlSuggestions.IsMousePressed) return;
			_suggestionRows = _presenter.SuggestionsRowCount;
			e.Count = _suggestionRows;
			e.Handled = true;
		}

		private void office2007OutlookTimePickerSpanTextChanged(object sender, EventArgs e)
		{
			if (_presenter == null || !_presenter.IsInitialized) return;
			gridControlSuggestions.Refresh();
			findAvailableDays();
			monthCalendarAdvDateSelection.RefreshCalendar(true);
		}

		private void dateTimePickerAdvStartDatePopupClosed(object sender, Syncfusion.Windows.Forms.PopupClosedEventArgs e)
		{
			dateIsChanged();
		}

		private void dateIsChanged()
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
			findAvailableDays();
			monthCalendarAdvDateSelection.RefreshCalendar(true);
			gridControlSuggestions.Refresh();
		}

		void outlookTimePickerStartTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (outlookTimePickerStartTime.Disposing)
				return;

			_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Text);
			gridControlSuggestions.Refresh();
		}

		void outlookTimePickerEndTimeLeave(object sender, EventArgs e)
		{
			if (_presenter == null) return;
			if (outlookTimePickerEndTime.Disposing)
				return;

			_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Text);
			gridControlSuggestions.Refresh();
		}

		void outlookTimePickerStartTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerStartTimeKeyDown(e.KeyCode, outlookTimePickerStartTime.Text);
		}

		void outlookTimePickerEndTimeKeyDown(object sender, KeyEventArgs e)
		{
			_presenter.OnOutlookTimePickerEndTimeKeyDown(e.KeyCode, outlookTimePickerEndTime.Text);
		}

		void outlookTimePickerStartTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnOutlookTimePickerStartTimeLeave(outlookTimePickerStartTime.Text);
			gridControlSchedules.Refresh();
			gridControlSuggestions.Refresh();
		}

		void outlookTimePickerEndTimeSelectedIndexChanged(object sender, EventArgs e)
		{
			_presenter.OnOutlookTimePickerEndTimeLeave(outlookTimePickerEndTime.Text);
			gridControlSchedules.Refresh();
			gridControlSuggestions.Refresh();
		}

		public void NotifyMeetingDatesChanged()
		{
			_composer.NotifyMeetingDatesChanged(this);
		}

		private void dateTimePickerAdvStartDateValueChanged(object sender, EventArgs e)
		{
			dateIsChanged();
		}

		private void meetingSchedulesViewResize(object sender, EventArgs e)
		{
			gridControlSchedules.Refresh();
		}

		private void gridControlSuggestionsMouseUp(object sender, MouseEventArgs e)
		{
			gridControlSuggestions.Refresh();
		}
	}
}
