using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
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

		public ShiftTradeScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider,
			IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
			IShiftTradeAddPersonScheduleViewModelMapper shiftTradePersonScheduleViewModelMapper,
			IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradePersonScheduleViewModelMapper = shiftTradePersonScheduleViewModelMapper;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
		}

		public ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);
			return getShiftTradeScheduleViewModel(data.Paging, data.ShiftTradeDate, data.TimeFilter, possibleTradePersons);
		}

		public ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelDataForAllTeams data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersonsForAllTeams(data);
			return getShiftTradeScheduleViewModel(data.Paging, data.ShiftTradeDate, data.TimeFilter, possibleTradePersons);
		}

		public ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelDataForAllTeams data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersonsForAllTeams(data);

			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);

			var startUtc = DateTime.SpecifyKind(myScheduleDayReadModel.Start.Value, DateTimeKind.Utc);
			var endUtc = DateTime.SpecifyKind(myScheduleDayReadModel.End.Value, DateTimeKind.Utc);
			var period = new DateTimePeriod(startUtc, endUtc);

			var possibleTradeSchedule = data.TimeFilter == null
				? getBulletinSchedules(period, possibleTradePersons, data.Paging).ToList()
				: getBulletinSchedulesWithTimeFilter(period, possibleTradePersons, data.Paging, data.TimeFilter).ToList();

			var paging = data.Paging;
			var shiftTradeDate = data.ShiftTradeDate;

			return getShiftTradeScheduleViewModel(paging, myScheduleDayReadModel, possibleTradeSchedule, shiftTradeDate);
		}

		private ShiftTradeScheduleViewModel getShiftTradeScheduleViewModel(Paging paging,
			IPersonScheduleDayReadModel myScheduleDayReadModel, List<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedule, DateOnly shiftTradeDate)
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
			TimeFilterInfo timeFilter, DatePersons possibleTradePersons)
		{
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(shiftTradeDate);
			var possibleTradeSchedule = timeFilter == null
				? getPossibleTradeSchedules(possibleTradePersons, paging).ToList()
				: getFilteredTimesPossibleTradeSchedules(possibleTradePersons, paging, timeFilter).ToList();

			return getShiftTradeScheduleViewModel(paging, myScheduleDayReadModel, possibleTradeSchedule, shiftTradeDate);
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getBulletinSchedules(DateTimePeriod mySchedulePeriod,
			DatePersons datePersons, Paging paging)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrieveBulletinTradeSchedules(datePersons.Date, datePersons.Persons,
					mySchedulePeriod, paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getBulletinSchedulesWithTimeFilter(
			DateTimePeriod mySchedulePeriod, DatePersons datePersons, Paging paging, TimeFilterInfo filter)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrieveBulletinTradeSchedulesWithTimeFilter(datePersons.Date,
					datePersons.Persons, mySchedulePeriod, paging, filter);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getPossibleTradeSchedules(DatePersons datePersons,
			Paging paging)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedules(datePersons.Date, datePersons.Persons,
					paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getFilteredTimesPossibleTradeSchedules(
			DatePersons datePersons, Paging paging, TimeFilterInfo timeFilter)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedulesWithFilteredTimes(datePersons.Date,
					datePersons.Persons, paging, timeFilter);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}
	}
}