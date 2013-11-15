using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeScheduleViewModelMapper : IShiftTradeScheduleViewModelMapper
	{
		private readonly IShiftTradeRequestProvider _shiftTradeRequestProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradePersonScheduleViewModelMapper _shiftTradePersonScheduleViewModelMapper;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;

		public ShiftTradeScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider, 
													IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
													IShiftTradePersonScheduleViewModelMapper shiftTradePersonScheduleViewModelMapper,
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

			ShiftTradePersonScheduleViewModel mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel);
			IEnumerable<ShiftTradePersonScheduleViewModel> possibleTradeSchedule = getPossibleTradeSchedules(possibleTradePersons, data.Paging);

			IEnumerable<ShiftTradeTimeLineHoursViewModel> timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(
				mySchedule, possibleTradeSchedule, data.ShiftTradeDate);

			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours
			};
		}

		private IEnumerable<ShiftTradePersonScheduleViewModel> getPossibleTradeSchedules(DatePersons datePersons, Paging paging)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedules(datePersons.Date, datePersons.Persons, paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradePersonScheduleViewModel>();
		}
	}
}