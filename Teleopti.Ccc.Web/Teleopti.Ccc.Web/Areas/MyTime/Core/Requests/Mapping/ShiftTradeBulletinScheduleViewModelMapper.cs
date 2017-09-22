using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeBulletinScheduleViewModelMapper : IShiftTradeBulletinScheduleViewModelMapper
	{
		private readonly IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradeAddPersonScheduleViewModelMapper _shiftTradePersonScheduleViewModelMapper;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;

		public ShiftTradeBulletinScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider,
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
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);
			if (data.Paging.Equals(Paging.Empty) || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			var mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true);
			var possibleTradeSchedule = data.TimeFilter == null
				? getPossibleTradeSchedules(possibleTradePersons, data.Paging, data.TimeSortOrder).ToList()
				: getFilteredTimesPossibleTradeSchedules(possibleTradePersons, data.Paging, data.TimeFilter, data.TimeSortOrder).ToList();
			var possibleTradeScheduleNum = possibleTradeSchedule.Any() ? possibleTradeSchedule.First().Total : 0;
			var pageCount = possibleTradeScheduleNum%data.Paging.Take != 0
				? possibleTradeScheduleNum/data.Paging.Take + 1
				: possibleTradeScheduleNum/data.Paging.Take;

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedule,
				data.ShiftTradeDate);

			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours,
				PageCount = pageCount
			};
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getPossibleTradeSchedules(DatePersons datePersons, Paging paging, string timeSortOrder)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedules(datePersons.Date, datePersons.Persons, paging, timeSortOrder);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getFilteredTimesPossibleTradeSchedules(DatePersons datePersons, Paging paging, TimeFilterInfo timeFilter, string timeSortOrder)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedulesWithFilteredTimes(datePersons.Date, datePersons.Persons, paging, timeFilter, timeSortOrder);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}
	}
}