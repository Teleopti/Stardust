using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleViewModelMapper : IShiftTradeScheduleViewModelMapper
	{
		private readonly IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradeAddPersonScheduleViewModelMapper _shiftTradePersonScheduleViewModelMapper;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradeScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider,
			IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
			IShiftTradeAddPersonScheduleViewModelMapper shiftTradePersonScheduleViewModelMapper,
			IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper,
			IPersonRequestRepository personRequestRepository,
			IScheduleProvider scheduleProvider,
			ILoggedOnUser loggedOnUser
			)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradePersonScheduleViewModelMapper = shiftTradePersonScheduleViewModelMapper;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
			_personRequestRepository = personRequestRepository;
			_scheduleProvider = scheduleProvider;
			_loggedOnUser = loggedOnUser;
		}

		public ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);
			return getShiftTradeScheduleViewModel(data.Paging, data.ShiftTradeDate, data.TimeFilter,data.TimeSortOrder, possibleTradePersons);
		}

		private IEnumerable<IShiftExchangeOffer> getMatchShiftExchangeOffers(IEnumerable<IPerson> persons, DateOnly shiftTradeDate)
		{
			var myScheduleDay =
				_scheduleProvider.GetScheduleForPersons(shiftTradeDate, new List<IPerson> {_loggedOnUser.CurrentUser()})
					.FirstOrDefault();

			if (myScheduleDay == null)
			{
				return new List<IShiftExchangeOffer>();
			}
			else
			{
				var result = _personRequestRepository.FindShiftExchangeOffersForBulletin(persons, shiftTradeDate);
				return result.Where(x => x.IsWantedSchedule(myScheduleDay));
			}			
		}

		private IScheduleDay getScheduleDayForShiftExchangeOffer(IShiftExchangeOffer offer)
		{
			IEnumerable<IPerson> persons = new List<IPerson>() { offer.Person};
			return _scheduleProvider.GetScheduleForPersons(offer.Date, persons).FirstOrDefault();
		}

		private IEnumerable<IShiftExchangeOffer> getMatchShiftExchangeOffers(IEnumerable<IShiftExchangeOffer> shiftExchangeOffers, TimeFilterInfo timeFilter)
		{
			var possibleTargets = shiftExchangeOffers.Select( x => new
			{
				ShiftExchangeOffer = x, 
				ScheduleDay = getScheduleDayForShiftExchangeOffer(x)
			});

			Func<IScheduleDay, bool> filterFunc = (s =>
			{
				if (timeFilter.IsDayOff && s.HasDayOff())
				{
					return true;
				}

				var period = s.ProjectionService().CreateProjection().Period();
				if (period.HasValue)
				{
					bool startMatch = timeFilter.StartTimes.IsEmpty() ||
					                  !timeFilter.StartTimes.Where(start => start.Contains(period.Value.StartDateTime)).IsEmpty();
					bool endMatch = timeFilter.EndTimes.IsEmpty() ||
					                !timeFilter.EndTimes.Where(end => end.Contains(period.Value.EndDateTime)).IsEmpty();
					return startMatch && endMatch;
				}
				else
				{					
					return !s.HasDayOff() && timeFilter.IsEmptyDay;
				}
			});

			return possibleTargets.Where(x => filterFunc(x.ScheduleDay)).Select(x => x.ShiftExchangeOffer);			
		}


		public ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelData data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);					
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);

			var filteredShiftExchangeOffers = getMatchShiftExchangeOffers(possibleTradePersons.Persons, data.ShiftTradeDate);
			if (data.TimeFilter != null)
			{
				filteredShiftExchangeOffers = getMatchShiftExchangeOffers(filteredShiftExchangeOffers, data.TimeFilter);
			}
			
			var possibleTradeSchedule = getBulletinSchedules(filteredShiftExchangeOffers, data.Paging);

			return getShiftTradeScheduleViewModel(data.Paging, myScheduleDayReadModel, possibleTradeSchedule, data.ShiftTradeDate);
		}

		private ShiftTradeScheduleViewModel getShiftTradeScheduleViewModel(Paging paging,
			IPersonScheduleDayReadModel myScheduleDayReadModel, IEnumerable<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedule, DateOnly shiftTradeDate)
		{
			var mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true);
			var possibleTradeScheduleNum = possibleTradeSchedule.Any()
				? possibleTradeSchedule.First().Total
				: 0;
			var pageCount = possibleTradeScheduleNum%paging.Take != 0
				? possibleTradeScheduleNum/paging.Take + 1
				: possibleTradeScheduleNum/paging.Take;

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedule,
				shiftTradeDate);

			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours,
				PageCount = pageCount,
			};
		}

		private ShiftTradeScheduleViewModel getShiftTradeScheduleViewModel(Paging paging, DateOnly shiftTradeDate,
			TimeFilterInfo timeFilter,string timeSortOrder, DatePersons possibleTradePersons)
		{
			var ret = new ShiftTradeScheduleViewModel();
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(shiftTradeDate);
			var mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true);
			ret.MySchedule = mySchedule;
			var possibleTradeSchedule = new List<ShiftTradeAddPersonScheduleViewModel>();
			if (mySchedule == null ||!mySchedule.IsFullDayAbsence)
			{
				possibleTradeSchedule = timeFilter == null
				? getPossibleTradeSchedules(possibleTradePersons, paging, timeSortOrder).ToList()
				: getFilteredTimesPossibleTradeSchedules(possibleTradePersons, paging, timeFilter, timeSortOrder).ToList();
			}
			
			var possibleTradeScheduleNum = possibleTradeSchedule.Any()
				? possibleTradeSchedule.First().Total
				: 0;
			var pageCount = possibleTradeScheduleNum % paging.Take != 0
				? possibleTradeScheduleNum / paging.Take + 1
				: possibleTradeScheduleNum / paging.Take;

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedule,
				shiftTradeDate);

			ret.PageCount = pageCount;
			ret.PossibleTradeSchedules = possibleTradeSchedule;
			ret.TimeLineHours = timeLineHours;

			return ret;
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getBulletinSchedules(IEnumerable<IShiftExchangeOffer> offers , Paging paging)
		{
			var offerList = offers.ToList();
			if (offerList.Any())
			{                                              
				var schedules = _shiftTradeRequestProvider.RetrieveBulletinTradeSchedules(offerList.Select(x => x.ShiftExchangeOfferId), paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getPossibleTradeSchedules(DatePersons datePersons,
			Paging paging,string timeSortOrder)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedules(datePersons.Date, datePersons.Persons,
					paging, timeSortOrder);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getFilteredTimesPossibleTradeSchedules(
			DatePersons datePersons, Paging paging, TimeFilterInfo timeFilter, string timeSortOrder)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedulesWithFilteredTimes(datePersons.Date,
					datePersons.Persons, paging, timeFilter, timeSortOrder);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}		
	}
}