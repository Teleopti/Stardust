using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings
{
	public class MeetingImpactPresenter : IMeetingDetailPresenter
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IMeetingImpactView _meetingImpactView;
		private readonly IBestSlotForMeetingFinder _bestSlotForMeetingFinder;
		private readonly IMeetingViewModel _meetingViewModel;
		private Lazy<TimeZoneInfo> _timeZone;
		private DateTimePeriod _currentPeriod;
		private IList<BestMeetingSlotResult> _pickBestResult = new List<BestMeetingSlotResult>();
		private BestMeetingSlotResult _currentResult;
		private bool _updatingView;
	    private readonly IMeetingImpactCalculator _meetingImpactCalculator;
        private bool _isInitialized;
	    private readonly IMeetingImpactSkillGridHandler _meetingImpactSkillGridHandler;
	    private readonly IMeetingImpactTransparentWindowHandler _transparentWindowHandler;
	    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
	    private readonly IMeetingStateHolderLoaderHelper _meetingStateHolderLoaderHelper;
	    private DateTimePeriod _searchPeriod;
	    private bool _inSearch;
	    private bool _disposed;

	    public MeetingImpactPresenter(ISchedulerStateHolder schedulerStateHolder, IMeetingImpactView meetingImpactView, IMeetingViewModel meetingViewModel,
            IMeetingStateHolderLoaderHelper meetingStateHolderLoaderHelper, IBestSlotForMeetingFinder bestSlotForMeetingFinder,
            IMeetingImpactCalculator meetingImpactCalculator, IMeetingImpactSkillGridHandler meetingImpactSkillGridHandler, IMeetingImpactTransparentWindowHandler transparentWindowHandler, IUnitOfWorkFactory unitOfWorkFactory)
		{
			if (meetingViewModel == null)
				throw new ArgumentNullException("meetingViewModel");

            if (transparentWindowHandler == null)
                throw new ArgumentNullException("transparentWindowHandler");

			_schedulerStateHolder = schedulerStateHolder;
			_meetingViewModel = meetingViewModel;
			_meetingImpactView = meetingImpactView;
	        _meetingStateHolderLoaderHelper = meetingStateHolderLoaderHelper; 
			_bestSlotForMeetingFinder = bestSlotForMeetingFinder;
			_meetingImpactCalculator = meetingImpactCalculator;
	        _meetingImpactSkillGridHandler = meetingImpactSkillGridHandler;
	        _transparentWindowHandler = transparentWindowHandler;
	        _unitOfWorkFactory = unitOfWorkFactory;
	        _transparentWindowHandler.TransparentWindowMoved += TransparentWindowMoved;
            _meetingStateHolderLoaderHelper.FinishedReloading += MeetingStateHolderLoaderHelperFinishedReloading;
			_timeZone = new Lazy<TimeZoneInfo>(() => _meetingViewModel.TimeZone);
	    }

        void TransparentWindowMoved(object sender, EventArgs e)
        {
             UpdateMeetingDatesTimesInViewFromModel();
             _meetingImpactCalculator.RecalculateResources(_meetingImpactView.StartDate);
            _meetingImpactView.RefreshMeetingControl();
        }

	    private void InitSearchDates()
        {
            if(_isInitialized) return;
            
            var startDate = _meetingViewModel.StartDate;
            _meetingImpactView.SetSearchStartDate(startDate);
            _meetingImpactView.SetSearchEndDate(startDate.AddDays(14));
            _isInitialized = true;
        }

        public void UpdateView()
        {
            if (_updatingView) return;
            _updatingView = true;
            _meetingImpactView.FindButtonEnabled = !_meetingViewModel.IsRecurring;
            InitSearchDates();
            SetPeriodAndLoadSchedulesWhenNeeded();
            _updatingView = false;
        }

	    public void CancelAllLoads()
	    {
            //cancel backgroud worker
            _meetingStateHolderLoaderHelper.CancelEveryReload();
	    }

        public void MeetingStateHolderLoaderHelperFinishedReloading(object sender, ReloadEventArgs reloadEventArgs)
        {
            // only empty this when meeting changed in another view or participants changed
            if (reloadEventArgs.HasReloaded)
            {
                _meetingImpactView.SetSearchInfo("");
                _pickBestResult = new List<BestMeetingSlotResult>();
                SetNextPreviousStatusInView();
            }

            if(_inSearch)
            {
                continueFindingAfterReload();
                return;
            }
            UpdateViewFromModel();
            _meetingImpactView.HideWaiting();
        }

        private void UpdateViewFromModel()
        {
            UpdateMeetingDatesTimesInViewFromModel();
            
           _meetingImpactSkillGridHandler.SetupSkillTabs();

           _meetingImpactSkillGridHandler.DrawSkillGrid(_timeZone.Value);
            _meetingImpactCalculator.RecalculateResources(_meetingImpactView.StartDate);
            UpdateMeetingControl();

            _transparentWindowHandler.ScrollMeetingIntoView();
        }

        public void UpdateMeetingControl()
        {
            TimeSpan endTime = _meetingViewModel.EndTime;

            if (_meetingViewModel.EndDate != _meetingViewModel.StartDate)
                endTime = endTime.Add(TimeSpan.FromDays(1));

            _transparentWindowHandler.DrawMeeting(_meetingViewModel.StartTime, endTime);
        }

        public void SetPeriodAndLoadSchedulesWhenNeeded()
        {
            var startDate = _meetingViewModel.StartDate;
            SetPeriodAndLoadSchedulesWhenNeeded(startDate, 2);
        }

		public void SetPeriodAndLoadSchedulesWhenNeeded(DateOnly dateOnly, int daysForward)
		{
			var endDate = dateOnly.AddDays(daysForward);
			_currentPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly.Date, endDate.Date, _timeZone.Value);
			SetPeriodAndLoadSchedulesWhenNeeded(_currentPeriod);
		}

		private void SetPeriodAndLoadSchedulesWhenNeeded(DateTimePeriod dateTimePeriod)
		{
            SetNextPreviousStatusInView();

            _meetingImpactView.ShowWaiting();
			
            _meetingStateHolderLoaderHelper.ReloadResultIfNeeded(_schedulerStateHolder.RequestedScenario, dateTimePeriod, GetRequiredPersons());
			
		}

		private IList<IPerson> GetRequiredPersons()
		{
			return _meetingViewModel.RequiredParticipants.Select(contactPersonViewModel => contactPersonViewModel.ContainedEntity).ToList();
		}

		public void FindBestMeetingSlot()
		{
            _meetingImpactCalculator.RemoveAndRecalculateResources(_meetingViewModel.Meeting, _meetingImpactView.StartDate);

			var startSearch = TimeZoneHelper.ConvertToUtc(_meetingImpactView.BestSlotSearchPeriodStart, _timeZone.Value);
			var endSearch = TimeZoneHelper.ConvertToUtc(_meetingImpactView.BestSlotSearchPeriodEnd, _timeZone.Value);

			var daysForward = (int)endSearch.Subtract(startSearch).TotalDays + 1;
			if (daysForward > 14 || daysForward < 1)
			{
				endSearch = startSearch.AddDays(14);
				//daysForward = 14;
			}
            if (endSearch <= startSearch)
                endSearch = startSearch.AddDays(1);
			_searchPeriod = new DateTimePeriod(startSearch, endSearch);
		    var periodWithExtraDay = new DateTimePeriod(startSearch, endSearch.AddDays(1));
		    _inSearch = true;
            SetPeriodAndLoadSchedulesWhenNeeded(periodWithExtraDay);

		}

        private void continueFindingAfterReload()
        {
            _inSearch = false;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var required = GetRequiredPersons();
                uow.Reassociate(required);
                _pickBestResult = _bestSlotForMeetingFinder.FindBestSlot(required, _searchPeriod,
                                                                _meetingViewModel.MeetingDuration, 15);
            }
            
            SetNextPreviousStatusInView();
            if (_pickBestResult.IsEmpty())
            {
                _meetingImpactView.HideWaiting();
                _meetingImpactView.SetSearchInfo(UserTexts.Resources.NoGoodSlotsFound);
                return;
            }

            _meetingImpactView.SetSearchInfo(_pickBestResult.Count + UserTexts.Resources.SlotsFound);

            _currentResult = _pickBestResult[0];
            UpdateModelFromResult(_currentResult);

            UpdateViewFromModel();
            _meetingImpactView.HideWaiting();
        }

		public void GoToNextSlot()
		{
			var pos = _pickBestResult.IndexOf(_currentResult);
			if(pos >= _pickBestResult.Count -1)
				return;
			_currentResult = _pickBestResult[pos + 1];
			UpdateModelFromResult(_currentResult);
            
            SetNextPreviousStatusInView();
            UpdateViewFromModel();
		}

		public void GoToPreviousSlot()
		{
			var pos = _pickBestResult.IndexOf(_currentResult);
			if (pos <= 0)
				return;
			_currentResult = _pickBestResult[pos - 1];
			UpdateModelFromResult(_currentResult);
            
            SetNextPreviousStatusInView();
            UpdateViewFromModel();
		}

		private void SetNextPreviousStatusInView()
		{
			if(_pickBestResult.Count < 2)
			{
				_meetingImpactView.SetPreviousState(false);
				_meetingImpactView.SetNextState(false);
				return;
			}
			var pos = _pickBestResult.IndexOf(_currentResult);

			if (pos == -1)
			{
				_currentResult = _pickBestResult[0];
				pos = 0;
			}
			
			if (pos == 0)
			{
				_meetingImpactView.SetPreviousState(false);
				_meetingImpactView.SetNextState(true);
				return;
			}
			if (pos < _pickBestResult.Count-1)
			{
				_meetingImpactView.SetPreviousState(true);
				_meetingImpactView.SetNextState(true);
				return;
			}
			_meetingImpactView.SetPreviousState(true);
			_meetingImpactView.SetNextState(false);
		}

		private void UpdateModelFromResult(BestMeetingSlotResult result)
		{
			var startTimeInMeetingTimeZone = TimeZoneHelper.ConvertFromUtc(result.SlotPeriod.StartDateTime, _timeZone.Value);
			var startDate = new DateOnly(startTimeInMeetingTimeZone);
			//if (!_meetingViewModel.IsRecurring)
			//{
				_meetingViewModel.StartDate = startDate;
				_meetingViewModel.RecurringEndDate = startDate;
				_meetingViewModel.StartTime = startTimeInMeetingTimeZone.TimeOfDay;
				_meetingViewModel.MeetingDuration = result.SlotLength;
			//}
			
		}

        public void MeetingDateChanged()
        {
            UpdateModelFromView();
            UpdateView();
        }

        public void UpdateModelFromView()
        {
            if (_updatingView) return;
            _meetingViewModel.StartDate = _meetingImpactView.StartDate;
            _meetingViewModel.RecurringEndDate = _meetingImpactView.StartDate;
            _meetingViewModel.StartTime = _meetingImpactView.StartTime;
            _meetingViewModel.EndTime = _meetingImpactView.EndTime;
        }

	    private void UpdateMeetingDatesTimesInViewFromModel()
		{
			//if (!_meetingViewModel.IsRecurring)
			//{
				_updatingView = true;
				_meetingImpactView.SetStartDate(_meetingViewModel.StartDate);
				_meetingImpactView.SetStartTime(_meetingViewModel.StartTime);
				_meetingImpactView.SetEndDate(_meetingViewModel.EndDate);
				_meetingImpactView.SetEndTime(_meetingViewModel.EndTime);
				_updatingView = false;
			//}
		}

        public void OnLeftColChanged()
        {
            if(_meetingImpactView.SelectedSkill() == null)
                return;
            UpdateMeetingControl();	
		}

        public void SkillTabChange()
		{
            if(_meetingImpactView.SelectedSkill() == null)
                return;
			//UpdateMeetingDatesTimesInViewFromModel();
            _meetingImpactSkillGridHandler.DrawSkillGrid(_timeZone.Value);
		    _meetingImpactCalculator.RecalculateResources(_meetingImpactView.StartDate);
			UpdateMeetingControl();

            _transparentWindowHandler.ScrollMeetingIntoView();	
		}
        
       public void OnMeetingTimeChange(string inputText)
        {
            TimeSpan? timeSpan;
            if (TimeHelper.TryParse(inputText, out timeSpan))
            {
                if (timeSpan.HasValue)
                {
                    if (timeSpan.Value >= TimeSpan.Zero)
                    {
                        UpdateModelFromView();
                        UpdateMeetingControl();
                        _transparentWindowHandler.ScrollMeetingIntoView();
                    }
                    else
                    {
                        _meetingImpactView.SetStartTime(_meetingViewModel.StartTime);
                        _meetingImpactView.SetEndTime(_meetingViewModel.EndTime);
                    }
                }
            }
            else
            {
                _meetingImpactView.SetStartTime(_meetingViewModel.StartTime);
                _meetingImpactView.SetEndTime(_meetingViewModel.EndTime);
            } 	
        }
        
        public void OnSlotTimeChange(string inputText)
        {
            TimeSpan? timeSpan;

            if (TimeHelper.TryParse(inputText, out timeSpan))
            {
                if (timeSpan.HasValue)
                {
                    if (timeSpan.Value >= TimeSpan.Zero)
                        _meetingViewModel.SlotStartTime = timeSpan.Value;
                    else
                    {
                        _meetingImpactView.SetSlotStartTime(_meetingViewModel.SlotStartTime);
                        _meetingImpactView.SetSlotEndTime(_meetingViewModel.SlotEndTime);
                    }
                }
            }
            else
            {
                _meetingImpactView.SetSlotStartTime(_meetingViewModel.SlotStartTime);
                _meetingImpactView.SetSlotEndTime(_meetingViewModel.SlotEndTime);
            }
        }

		public void OnSlotDateChange(DateTime startDate, DateTime endDate)
		{
			if (startDate > endDate)
				_meetingImpactView.SetSlotEndDate(startDate);
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
            _transparentWindowHandler.TransparentWindowMoved -= TransparentWindowMoved;
            _meetingStateHolderLoaderHelper.FinishedReloading -= MeetingStateHolderLoaderHelperFinishedReloading;
        }

        #endregion
	}
}