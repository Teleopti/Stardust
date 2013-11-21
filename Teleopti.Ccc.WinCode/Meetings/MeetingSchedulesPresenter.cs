using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.Panels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings
{
	 /// <summary>
	 /// Represents a MeetingSchedulerGridView
	 /// </summary>
	public class MeetingSchedulesPresenter: IMeetingDetailPresenter
	 {
		  private readonly IMeetingSchedulesView _view;
		  private readonly IMeetingViewModel _meetingViewModel;
		  private readonly ISchedulerStateHolder _schedulerStateHolder;
		  private readonly IRangeProjectionService _rangeProjectionService;
		  private readonly ISchedulerStateLoader _schedulerStateLoader;
		  
		  private readonly IDictionary<PersonSchedulePeriodCacheKey, IEnumerable<IVisualLayer>> _projectionCache =
				new Dictionary<PersonSchedulePeriodCacheKey, IEnumerable<IVisualLayer>>();
		  private DateOnly _currentDate;
		  private DateOnlyPeriod _currentPeriod;
		  private IList<ContactPersonViewModel> _participantList;
		  private IList<TimePeriod> _meetingTimeList;
		  private readonly IMeetingSlotFinderService _meetingSlotFinderService;
		  private bool _disposed;
		  public bool IsInitialized { get; private set; }
		  private readonly IMeetingMover _meetingMover;
		  private readonly IMeetingMousePositionDecider _meetingMousePositionDecider;

		  public MeetingSchedulesPresenter(IMeetingSchedulesView view, 
				IMeetingViewModel meetingViewModel, 
				ISchedulerStateHolder schedulerStateHolder, 
				IRangeProjectionService rangeProjectionService,
				ISchedulerStateLoader schedulerStateLoader,
				IMeetingSlotFinderService meetingSlotFinderService,
				IMeetingMover meetingMover,
				IMeetingMousePositionDecider meetingMousePositionDecider)
		  {
				_view = view;
				_meetingViewModel = meetingViewModel;
				_schedulerStateHolder = schedulerStateHolder;
				_rangeProjectionService = rangeProjectionService;
				_schedulerStateLoader = schedulerStateLoader;
				_meetingSlotFinderService = meetingSlotFinderService;
				_meetingMover = meetingMover;
				_meetingMousePositionDecider = meetingMousePositionDecider;
		  }

		  public void GridControlSchedulesMouseDown()
		  {
				if (_meetingMousePositionDecider.MeetingMousePosition.Equals(MeetingMousePosition.OverStart)) _meetingMover.MeetingMoveState = MeetingMoveState.MovingStart;
				if (_meetingMousePositionDecider.MeetingMousePosition.Equals(MeetingMousePosition.OverEnd)) _meetingMover.MeetingMoveState = MeetingMoveState.MovingEnd;
				if (_meetingMousePositionDecider.MeetingMousePosition.Equals(MeetingMousePosition.OverStartAndEnd)) _meetingMover.MeetingMoveState = MeetingMoveState.MovingStartAndEnd; 
		  }

		  public void GridControlSchedulesMouseUp()
		  {
				if (_meetingMover.MeetingMoveState != MeetingMoveState.None)
				{
					 _view.OnMeetingTimeChanged();
				}

				_meetingMover.MeetingMoveState = MeetingMoveState.None;
				_meetingMousePositionDecider.MeetingMousePosition = MeetingMousePosition.None;
		  }

		  public void GridControlSchedulesMouseMove(int mouseGridPositionX, Rectangle cellRect, LengthToTimeCalculator pixelConverter, int mouseCellPosition)
		  {
				if(pixelConverter == null) throw new ArgumentNullException("pixelConverter");

				var period = Model.Meeting.MeetingPeriod(Model.StartDate);

				_meetingMover.Move(pixelConverter, mouseCellPosition, cellRect, _meetingMover.MeetingMoveState, _meetingMousePositionDecider.DiffStart);
				if (!_meetingMover.MeetingMoveState.Equals(MeetingMoveState.None)) return;

				_meetingMousePositionDecider.CheckMousePosition(mouseGridPositionX, cellRect, pixelConverter, period, mouseCellPosition, Model.StartTime);
		  }

		  public RectangleF GetLayerRectangle(LengthToTimeCalculator pixelConverter, DateTimePeriod period, RectangleF clientRect)
		  {
				if(pixelConverter == null) throw new ArgumentNullException("pixelConverter");

				return _meetingMousePositionDecider.GetLayerRectangle(pixelConverter, period, clientRect);
		  }

		  public void Initialize()
		  {
				UpdateView();

				IsInitialized = true;
		  }

		  public void OnParticipantsSet()
		  {
				RecreateParticipantList();
				_view.RefreshGrid();
		  }

		  public void RefreshRecurringDates()
		  {
				_view.SetRecurringDates(_meetingViewModel.Meeting.GetRecurringDates());
		  }

		  public void RecreateParticipantList()
		  {
				_participantList =
					 new List<ContactPersonViewModel>(
						  _meetingViewModel.RequiredParticipants.Concat(_meetingViewModel.OptionalParticipants));
		  }

		  public IList<TimePeriod> SuggestionList
		  {
				get
				{
					 return _meetingTimeList;
				}
		  }

		  public int RowCount
		  {
				get
				{
					 if (_participantList == null) return 0;
					 return _participantList.Count;
				}
		  }

		  public IList<DateOnly> GetAvailableDays
		  {
				get
				{
					 IList<DateOnly> dates = new List<DateOnly>();

					 var date = _currentDate;
					 var week = DateHelper.GetWeekPeriod(date, CultureInfo.CurrentCulture);
					 for (var i = 0; i < 7; i++)
					 {
						  DateOnly weekDates = week.StartDate;
						  var nextDate = new DateOnly(weekDates.AddDays(i));
						  dates.Add(new DateOnly(nextDate));
					 }

					 TimeSpan start = _view.SetSuggestListStartTime;
					 TimeSpan end = _view.SetSuggestListEndTime;
					 var startT = _meetingViewModel.Meeting.StartTime;
					 var endT = _meetingViewModel.Meeting.EndTime;

					 var duration = endT.TotalMinutes - startT.TotalMinutes;

					 var selectedScheduleDictionary = StateHolder.Schedules;
					 var availableDates = new List<DateOnly>();
					 if (!_meetingViewModel.IsRecurring)
					 {
						  var requiredPersons = GetRequiredPersons();
						  availableDates =
								(List<DateOnly>)
							  _meetingSlotFinderService.FindAvailableDays(dates, TimeSpan.FromMinutes(duration),
																						 start,
																						 end,
																						 selectedScheduleDictionary, requiredPersons);
					 }
					 return availableDates;
				}
		  }

		  public int SuggestionsRowCount
		  {
				get
				{
					 var suggestionRows = 0;
					 var date = _currentDate;

					 TimeSpan start = _view.SetSuggestListStartTime;
					 TimeSpan end = _view.SetSuggestListEndTime;
					 var startT = _meetingViewModel.Meeting.StartTime;
					 var endT = _meetingViewModel.Meeting.EndTime;

					 var duration = endT.TotalMinutes - startT.TotalMinutes;
					 var selectedScheduleDictionary = StateHolder.Schedules;
					 
					 _meetingTimeList = new List<TimePeriod>();

					 if (!_meetingViewModel.IsRecurring && (duration != 0))
					 {
						  var requiredPersons = GetRequiredPersons();
						  _meetingTimeList = _meetingSlotFinderService.FindSlots(date, TimeSpan.FromMinutes(duration), start, end,
															  selectedScheduleDictionary, requiredPersons);
						  if (_meetingViewModel.IsRecurring)
						  {
								suggestionRows = 0;
						  }
						  else
						  {
								suggestionRows = _meetingTimeList.Count;
						  }
					 }
					 return suggestionRows;
				}
		  }

		  private IEnumerable<IPerson> GetRequiredPersons()
		  {
				RecreateParticipantList();
				return (
						 from participant in ParticipantList
						 let person = GetPerson(participant)
						 let meetingPerson = (from mp in _meetingViewModel.Meeting.MeetingPersons
										where mp.Person.Id.GetValueOrDefault(Guid.Empty) == person.Id.GetValueOrDefault(Guid.Empty)
													 select mp).Single()
						 where !meetingPerson.Optional
						 select person).ToList();
		  }

		  public ReadOnlyCollection<ContactPersonViewModel> ParticipantList
		  {
				get {return new ReadOnlyCollection<ContactPersonViewModel>(_participantList);}
		  }

		  public IMeetingViewModel Model
		  {
				get {
					 return _meetingViewModel;
				}
		  }

		  public DateOnly CurrentDate
		  {
				get {
					 return _currentDate;
				}
		  }

		  public ISchedulerStateHolder StateHolder
		  {
				get
				{
					 return _schedulerStateHolder;
				}
		  }

		  public IEnumerable<IVisualLayer> GetVisualLayersForPerson(EntityContainer<IPerson> personViewModel)
		  {
				var period = _currentPeriod;
				var cacheKey = new PersonSchedulePeriodCacheKey { Period = period, Person = personViewModel.ContainedEntity };
				IEnumerable<IVisualLayer> projection;
				if (!_projectionCache.TryGetValue(cacheKey, out projection))
				{
					 projection =
						  _rangeProjectionService.CreateProjection(
								_schedulerStateHolder.Schedules[personViewModel.ContainedEntity], period.ToDateTimePeriod(TimeZoneHelper.CurrentSessionTimeZone));
					 _projectionCache.Add(cacheKey,projection);
				}
				return projection;
		  }

		  public DateTimePeriod MergedOrDefaultPeriod()
		  {
				var start = DateTime.MaxValue;
				var end = DateTime.MinValue;

				foreach (var personModel in ParticipantList)
				{
					 var utcDate = Model.StartDate;
					 var totalScheduleRange = _schedulerStateHolder.Schedules[personModel.ContainedEntity];
					 var scheduleDay = totalScheduleRange.ScheduledDay(utcDate);

					 var visualLayerCollection = scheduleDay.ProjectionService().CreateProjection();
					 var period = visualLayerCollection.Period();
					 if (period == null) continue;

					 var p = period.Value;
					 if (p.StartDateTime < start) start = p.StartDateTime;
					 if (p.EndDateTime > end) end = p.EndDateTime;    
				}

				if (start < end)
				{
					 if (start.Minute != 0) start = start.AddMinutes(start.Minute * -1);
					 if (end.Minute != 0) end = end.AddMinutes(60 - end.Minute);
				if (!start.Date.Equals(end.Date)) return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(Model.StartDate, end.AddHours(1), _schedulerStateHolder.TimeZoneInfo);
				}

				return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(Model.StartDate, Model.StartDate.AddDays(1), _schedulerStateHolder.TimeZoneInfo);
		  }

		  public static IPerson GetPerson(EntityContainer<IPerson> personViewModel)
		  {
				return personViewModel.ContainedEntity;
		  }
			
		  public DateOnlyPeriod GetCurrentPeriod()
		  {
				return _currentPeriod;
		  }

		  public void SetCurrentDate(DateOnly currentDate)
		  {
				SetPeriodAndLoadSchedulesWhenNeeded(currentDate);
				_view.RefreshGrid();
		  }

		  private void SetPeriodAndLoadSchedulesWhenNeeded(DateOnly currentDate)
		  {
				_currentDate = currentDate;

			var endDate = currentDate.AddDays(2);
			_currentPeriod = new DateOnlyPeriod(currentDate.AddDays(-1), endDate);

				if (!_schedulerStateHolder.RequestedPeriod.DateOnlyPeriod.Contains(_currentPeriod) || _schedulerStateHolder.Schedules==null)
				{
					var period = _currentPeriod.ToDateTimePeriod(TimeZoneHelper.CurrentSessionTimeZone);
					var scheduleDateTimePeriod = new ScheduleDateTimePeriod(period,
																							  _schedulerStateHolder.SchedulingResultState.
																								PersonsInOrganization,
																							  new MeetingScheduleRangeToLoadCalculator(period));
				_schedulerStateLoader.LoadSchedules(scheduleDateTimePeriod);
				
				}

				_currentPeriod = new DateOnlyPeriod(_currentDate, _currentDate.AddDays(1)); //OBS var 1.5
		  }

		  public void SetStartTime(TimeSpan startTime)
		  {
				_meetingViewModel.StartTime = startTime;
				_view.SetStartTime(_meetingViewModel.StartTime);
		  }

		  public void SetEndTime(TimeSpan endTime)
		  {
				_meetingViewModel.EndTime = endTime;
				_view.SetEndTime(_meetingViewModel.EndTime);
		  }

		  public void SetStartDate(DateOnly startDate)
		  {
				var isRecurring = _meetingViewModel.IsRecurring;
				_meetingViewModel.StartDate = startDate;
				if (!isRecurring)
				{
					 _meetingViewModel.RecurringEndDate = startDate;
				}
				_view.SetEndDate(_meetingViewModel.EndDate);
				_view.SetCurrentDate(_meetingViewModel.StartDate);
		  }

		  public void SetStartDateFromCurrentDate(DateOnly theDate)
		  {
				var isRecurring = _meetingViewModel.IsRecurring;
				_meetingViewModel.StartDate = theDate;
				if (!isRecurring)
				{
					 _meetingViewModel.RecurringEndDate = theDate;
				}
				_view.SetEndDate(_meetingViewModel.EndDate);
				_view.SetStartDate(_meetingViewModel.StartDate);
		  }

		  public void SetTimesFromEditor(TimeSpan startTime, TimeSpan endTime)
		  {
				_meetingViewModel.StartTime = startTime;
				_meetingViewModel.EndTime = endTime;

				_view.SetStartTime(startTime);
				_view.SetEndTime(endTime);
		  }

		private struct PersonSchedulePeriodCacheKey
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public IPerson Person {     get; set; }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public DateOnlyPeriod Period { get; set; }
		}

		  public bool IsDayOff(EntityContainer<IPerson> personViewModel)
		  {
				IScheduleRange scheduleRange = _schedulerStateHolder.Schedules[personViewModel.ContainedEntity];
				IScheduleDay scheduleDay = scheduleRange.ScheduledDay(_currentDate);
				if (scheduleDay != null)
				{
					 SchedulePartView schedulePartView = scheduleDay.SignificantPart();
					 if (schedulePartView == SchedulePartView.DayOff)
					 {
						  return true;
					 }
				}
				return false;
		  }

		public void UpdateView()
		{
			_view.UnBindEvents();
			RecreateParticipantList();
			 SetCurrentDate(_meetingViewModel.StartDate);
			_view.SetStartDate(_meetingViewModel.StartDate);
			_view.SetEndDate(_meetingViewModel.EndDate);
			_view.SetStartTime(_meetingViewModel.StartTime);
			_view.SetEndTime(_meetingViewModel.EndTime);
			_view.SetRecurringDates(_meetingViewModel.Meeting.GetRecurringDates());
			_view.SetCurrentDate(_currentDate);
			_view.BindEvents();
			
		}
		
		public void CancelAllLoads()
		{
				//cancel background worker
		}

		public TimeSpan GetStartTime
		{
			get { return _meetingViewModel.StartTime; }
		}

		public TimeSpan GetEndTime
		{
			get { return _meetingViewModel.EndTime; }
		}

		public void OnOutlookTimePickerStartTimeLeave(string inputText)
		{
			TimeSpan? timeSpan;
			
			if (TimeHelper.TryParse(inputText, out timeSpan))
			{
				if (timeSpan.HasValue && timeSpan.Value >= TimeSpan.Zero && timeSpan != GetStartTime)
				{
					_meetingViewModel.StartTime = timeSpan.Value;
					//the meeting could change the end time when we change the start time
					UpdateView();
					_view.RefreshGrid();	
				}
			}
			else
				_view.SetStartTime(GetStartTime);	
		}

		public void OnOutlookTimePickerEndTimeLeave(string inputText)
		{
			TimeSpan? timeSpan;

			if (TimeHelper.TryParse(inputText, out timeSpan))
			{
					 if (timeSpan.HasValue && timeSpan.Value >= TimeSpan.Zero && timeSpan != GetStartTime)
					 {
						 _meetingViewModel.EndTime = timeSpan.Value;
					 }
			}
			else
				_view.SetEndTime(GetEndTime);

			_view.RefreshGrid();
		}

		public void OnOutlookTimePickerStartTimeKeyDown(Keys keys, string inputText)
		{
			if (keys == Keys.Enter)
				OnOutlookTimePickerStartTimeLeave(inputText);
		}

		public void OnOutlookTimePickerEndTimeKeyDown(Keys keys, string inputText)
		{
			if (keys == Keys.Enter)
				OnOutlookTimePickerEndTimeLeave(inputText);
		}

		  #region IDispose

		  public void Dispose()
		  {
				Dispose(true);
				GC.SuppressFinalize(this);
		  }

		  private void Dispose(bool disposing)
		  {
				if (!_disposed)
				{
					 if (disposing)
					 {
						  ReleaseManagedResources();
					 }
					 ReleaseUnmanagedResources();
					 _disposed = true;
				}
		  }

		  protected virtual void ReleaseUnmanagedResources()
		  {
		  }

		  protected virtual void ReleaseManagedResources()
		  {
		  }

		  #endregion
	 }
}
