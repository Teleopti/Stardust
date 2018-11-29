using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public class RequestDetailsShiftTradePresenter
	{
		private readonly IPersonRequestViewModel _personRequestViewModel;
		private readonly IScheduleDictionary _schedules;
		private readonly IRangeProjectionService _rangeProjectionService;

		private readonly IDictionary<PersonSchedulePeriodCacheKey, IEnumerable<IVisualLayer>> _projectionCache =
			new Dictionary<PersonSchedulePeriodCacheKey, IEnumerable<IVisualLayer>>();
		private IList<DateTimePeriod> _currentPeriods;
		private IList<ContactPersonViewModel> _participantList;
		private ReadOnlyCollection<IShiftTradeSwapDetail> _shiftTradeSwapDetails;
		private DateTimePeriod _displayPeriod;
		private TimeSpan _startSpan = TimeSpan.MaxValue;
		private TimeSpan _endSpan = TimeSpan.MinValue;

		public RequestDetailsShiftTradePresenter(IPersonRequestViewModel personRequestViewModel, IScheduleDictionary schedules, IRangeProjectionService rangeProjectionService)
		{
			_personRequestViewModel = personRequestViewModel;
			_schedules = schedules;
			_rangeProjectionService = rangeProjectionService;
		}

		public void Initialize()
		{
			var shiftTradeRequest = _personRequestViewModel.PersonRequest.Request as IShiftTradeRequest;
			if(shiftTradeRequest != null)
				_shiftTradeSwapDetails = shiftTradeRequest.ShiftTradeSwapDetails;
			RecreateParticipantList();
			InitializeCurrentPeriods();
			_displayPeriod = DisplayedPeriod();
			GetVisiblePeriods();
		}

		private void InitializeCurrentPeriods()
		{
			_currentPeriods = new List<DateTimePeriod>();
			foreach (var shiftTradeSwapDetail in _shiftTradeSwapDetails.OrderBy(m => m.DateFrom))
			{
				var timeZoneInfo = _personRequestViewModel.PersonRequest.Request.PersonFrom.PermissionInformation.DefaultTimeZone();
				var dateFrom = shiftTradeSwapDetail.DateFrom;
				var dateTo = shiftTradeSwapDetail.DateTo;

				_currentPeriods.Add(new DateOnlyPeriod(dateFrom, dateTo).ToDateTimePeriod(timeZoneInfo));
				_currentPeriods.Add(new DateOnlyPeriod(dateFrom, dateTo).ToDateTimePeriod(timeZoneInfo));
			}
		}

		private void RecreateParticipantList()
		{
			_participantList = new List<ContactPersonViewModel>();
			foreach (var shiftTradeSwapDetail in _shiftTradeSwapDetails)
			{
				_participantList.Add(new ContactPersonViewModel(shiftTradeSwapDetail.PersonFrom));
				_participantList.Add(new ContactPersonViewModel(shiftTradeSwapDetail.PersonTo));
			}
		}

		public int RowCount
		{
			get
			{
				return _participantList.Count;
			}
		}

		public IList<DateTimePeriod> CurrentPeriods
		{
			get{ return _currentPeriods;}
		}

		public DateTimePeriod DisplayPeriod
		{
			get { return _displayPeriod; }
		}

		private DateTimePeriod DisplayedPeriod()
        {
			if (_currentPeriods == null) return new DateTimePeriod();
			for (var i = 0; i < _currentPeriods.Count; i++)
			{
                var periodListWithAdjacentLayers = GetPeriodListWithAdjacentLayers(i);
				if (periodListWithAdjacentLayers == null) continue;

				var period = GetMergedPeriod(periodListWithAdjacentLayers);
				var lengthFromStart = period.StartDateTime - _currentPeriods[i].StartDateTime;
				if (lengthFromStart < _startSpan)
					_startSpan = lengthFromStart;
				var lengthFromEnd = period.EndDateTime - _currentPeriods[i].StartDateTime;
				if (lengthFromEnd > _endSpan)
					_endSpan = lengthFromEnd;
			}
			if (_startSpan == TimeSpan.MaxValue && _endSpan == TimeSpan.MinValue)
			{
				_startSpan = TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultStartHour);
				_endSpan = TimeSpan.FromHours(DefaultSchedulePeriodProvider.DefaultEndHour);
			}
			var firstReqeustedStartDateTime = _currentPeriods[0].StartDateTime;
			var startDateTime = (firstReqeustedStartDateTime + _startSpan).AddHours(-1);
			var endDateTime = (firstReqeustedStartDateTime + _endSpan).AddHours(1);
			return new DateTimePeriod(startDateTime, endDateTime);
		}

		private IEnumerable<IVisualLayer> GetPeriodListWithAdjacentLayers(int i)
		{
			var periodListWithAdjacentLayers = new List<IVisualLayer>();
			var visualLayers = GetVisualLayersForPerson(ParticipantList[i], _currentPeriods[i].ChangeEndTime(TimeSpan.FromDays(1)));
			var periodList = visualLayers.ToList();
			if (periodList.Count == 0 || periodList[0].Period.StartDateTime >= _currentPeriods[i].EndDateTime) return null;
			periodListWithAdjacentLayers.Add(periodList[0]);
			if (periodList.Count > 1)
			{
				for (int j = 0; j < periodList.Count; j++)
				{
					if ((j + 1) < periodList.Count)
					{
						if (periodList[j + 1].Period.AdjacentTo(periodList[j].Period))
						{
							periodListWithAdjacentLayers.Add(periodList[j + 1]);
						}
						else
						{
							break;
						}
					}
				}
			}
			return periodListWithAdjacentLayers;
		}

		private void GetVisiblePeriods()
		{
			var visiblePeriods = new List<DateTimePeriod>();
			foreach (var currentPeriod in _currentPeriods)
			{
				var requestedStartDateTime = currentPeriod.StartDateTime;
				var startDateTime = requestedStartDateTime + _startSpan;
				var endDateTime = requestedStartDateTime + _endSpan;
				visiblePeriods.Add(new DateTimePeriod(startDateTime, endDateTime));
			}
			_currentPeriods = visiblePeriods;
		}
		
		public ReadOnlyCollection<ContactPersonViewModel> ParticipantList
		{
			get{return _participantList != null ? new ReadOnlyCollection<ContactPersonViewModel>(_participantList) : null;}
		}

		private static DateTimePeriod GetMergedPeriod(IEnumerable<IVisualLayer> visualLayers)
		{
			if (visualLayers == null) throw new ArgumentNullException("visualLayers");
			var min = DateTime.MaxValue;
			var max = DateTime.MinValue;
			foreach (var layer in visualLayers)
			{
				if (layer.Period.StartDateTime < min)
					min = layer.Period.StartDateTime;

				if (layer.Period.EndDateTime > max)
					max = layer.Period.EndDateTime;
			}
			return new DateTimePeriod(min, max);
		}

		public IEnumerable<IVisualLayer> GetVisualLayersForPerson(EntityContainer<IPerson> personViewModel, DateTimePeriod currentPeriod)
		{
			if (personViewModel == null) throw new ArgumentNullException("personViewModel");
			var cacheKey = new PersonSchedulePeriodCacheKey
			               	{Period = currentPeriod, Person = personViewModel.ContainedEntity};
			IEnumerable<IVisualLayer> projection;
			if (!_projectionCache.TryGetValue(cacheKey, out projection))
			{
				projection = _rangeProjectionService.CreateProjection(_schedules[personViewModel.ContainedEntity], currentPeriod);
				_projectionCache.Add(cacheKey,projection);
			}
			return projection;
		}

		internal struct PersonSchedulePeriodCacheKey
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			internal IPerson Person { get; set; }

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			internal DateTimePeriod Period { get; set; }
		}

		public bool IsDayOff(EntityContainer<IPerson> personViewModel, DateOnly currentDate)
		{
			if (personViewModel == null) throw new ArgumentNullException("personViewModel");
			var scheduleRange = _schedules[personViewModel.ContainedEntity];
			var scheduleDay = scheduleRange.ScheduledDay(currentDate);
			if (scheduleDay != null)
			{
				var schedulePartView = scheduleDay.SignificantPartForDisplay();
				if (schedulePartView == SchedulePartView.DayOff)
				{
					return true;
				}
			}
			return false;
		}

		public TimeZoneInfo TimeZone
		{
			get { return _personRequestViewModel.PersonRequest.Request.PersonFrom.PermissionInformation.DefaultTimeZone(); }
		}
	}
}
