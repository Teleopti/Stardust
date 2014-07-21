﻿using System.Collections.Generic;
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
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);
			var personNum = possibleTradePersons.Persons.Count();
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}
			var pageCount = personNum % data.Paging.Take != 0 ? personNum / data.Paging.Take + 1 : personNum / data.Paging.Take;

			ShiftTradeAddPersonScheduleViewModel mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel);
			var possibleTradeSchedule = getPossibleTradeSchedules(possibleTradePersons, data.Paging).ToList();

			IEnumerable<ShiftTradeTimeLineHoursViewModel> timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(
				mySchedule, possibleTradeSchedule, data.ShiftTradeDate);

					
			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours,
				PageCount = pageCount,
			};
		}

		public ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelDataForAllTeams data)
		{
			var paging = new Paging() {Take = data.Paging.Take, Skip = data.Paging.Skip};
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersonsForAllTeams(data);
			var personNum = possibleTradePersons.Persons.Count();
			if (paging == null || paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}
			var pageCount = personNum % paging.Take != 0 ? personNum / paging.Take + 1 : personNum / paging.Take;

			ShiftTradeAddPersonScheduleViewModel mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel);
			var possibleTradeSchedule = getPossibleTradeSchedules(possibleTradePersons, paging).ToList();

			IEnumerable<ShiftTradeTimeLineHoursViewModel> timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(
				mySchedule, possibleTradeSchedule, data.ShiftTradeDate);

					
			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedule,
				TimeLineHours = timeLineHours,
				PageCount = pageCount,
			};
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getPossibleTradeSchedules(DatePersons datePersons, Paging paging)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedules(datePersons.Date, datePersons.Persons, paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}
	}
}