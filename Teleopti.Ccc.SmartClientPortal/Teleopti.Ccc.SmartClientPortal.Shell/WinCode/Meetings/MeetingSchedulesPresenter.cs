using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Panels;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
	 /// <summary>
	 /// Represents a MeetingSchedulerGridView
	 /// </summary>
	public class MeetingSchedulesPresenter: IMeetingDetailPresenter
	 {
		  private readonly IMeetingSchedulesView _view;
		  private readonly IMeetingViewModel _meetingViewModel;
		  private readonly ISchedulerStateHolder _schedulerStateHolder;
		  private readonly ISchedulerStateLoader _schedulerStateLoader;
		  
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
				ISchedulerStateLoader schedulerStateLoader,
				IMeetingSlotFinderService meetingSlotFinderService,
				IMeetingMover meetingMover,
				IMeetingMousePositionDecider meetingMousePositionDecider)
		  {
				_view = view;
				_meetingViewModel = meetingViewModel;
				_schedulerStateHolder = schedulerStateHolder;
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
						  var nextDate = weekDates.AddDays(i);
						  dates.Add(nextDate);
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
					if(start != DateTime.MaxValue) return new DateTimePeriod(start,end);
				}

				return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(Model.StartDate, Model.StartDate.AddDays(1), TimeZoneGuardForDesktop.Instance.CurrentTimeZone());
		  }

		  public static IPerson GetPerson(EntityContainer<IPerson> personViewModel)
		  {
				return personViewModel.ContainedEntity;
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
					var period = _currentPeriod.ToDateTimePeriod(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
					var scheduleDateTimePeriod = new ScheduleDateTimePeriod(period,
																							  _schedulerStateHolder.SchedulingResultState.
																								LoadedAgents,
																							  new MeetingScheduleRangeToLoadCalculator(period));
				_schedulerStateLoader.LoadSchedules(scheduleDateTimePeriod);
				
				}

				_currentPeriod = new DateOnlyPeriod(_currentDate.AddDays(-1), _currentDate.AddDays(1)); //OBS var 1.5
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
