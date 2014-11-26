﻿using System;
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
		private readonly IUserTimeZone _userTimeZone;

		public ShiftTradeScheduleViewModelMapper(IShiftTradeRequestProvider shiftTradeRequestProvider, 
													IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
													IShiftTradeAddPersonScheduleViewModelMapper shiftTradePersonScheduleViewModelMapper,
													IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper, IUserTimeZone userTimeZone)
		{
			_shiftTradeRequestProvider = shiftTradeRequestProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradePersonScheduleViewModelMapper = shiftTradePersonScheduleViewModelMapper;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
			_userTimeZone = userTimeZone;
		}

		public ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data)
		{
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersons(data);
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			ShiftTradeAddPersonScheduleViewModel mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true);
			List<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedule;
			if (data.TimeFilter == null)
			{
				possibleTradeSchedule = getPossibleTradeSchedules(possibleTradePersons, data.Paging).ToList();
			}
			else
			{
				possibleTradeSchedule = getFilteredTimesPossibleTradeSchedules(possibleTradePersons, data.Paging, data.TimeFilter).ToList();
			}
			var possibleTradeScheduleNum = possibleTradeSchedule.Any() ? possibleTradeSchedule.First().Total : 0;
			var pageCount = possibleTradeScheduleNum % data.Paging.Take != 0 ? possibleTradeScheduleNum / data.Paging.Take + 1 : possibleTradeScheduleNum / data.Paging.Take;

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
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersonsForAllTeams(data);
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			ShiftTradeAddPersonScheduleViewModel mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true);
			List<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedule;
			if (data.TimeFilter == null)
			{
				possibleTradeSchedule = getPossibleTradeSchedules(possibleTradePersons, data.Paging).ToList();
			}
			else
			{
				possibleTradeSchedule = getFilteredTimesPossibleTradeSchedules(possibleTradePersons, data.Paging, data.TimeFilter).ToList();
			}
			var possibleTradeScheduleNum = possibleTradeSchedule.Any() ? possibleTradeSchedule.First().Total : 0;
			var pageCount = possibleTradeScheduleNum % data.Paging.Take != 0 ? possibleTradeScheduleNum / data.Paging.Take + 1 : possibleTradeScheduleNum / data.Paging.Take;

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

		public ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelDataForAllTeams data)
		{
			var myScheduleDayReadModel = _shiftTradeRequestProvider.RetrieveMySchedule(data.ShiftTradeDate);
			var possibleTradePersons = _possibleShiftTradePersonsProvider.RetrievePersonsForAllTeams(data);
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new ShiftTradeScheduleViewModel();
			}

			ShiftTradeAddPersonScheduleViewModel mySchedule = _shiftTradePersonScheduleViewModelMapper.Map(myScheduleDayReadModel, true);

	
			List<ShiftTradeAddPersonScheduleViewModel> possibleTradeSchedule;
			var startUtc = DateTime.SpecifyKind((DateTime) myScheduleDayReadModel.Start, DateTimeKind.Utc);
			var endUtc = DateTime.SpecifyKind((DateTime) myScheduleDayReadModel.End, DateTimeKind.Utc);
			var period = new DateTimePeriod(startUtc, endUtc);
			
			if (data.TimeFilter == null)
			{
				possibleTradeSchedule = getBulletinSchedules(period, possibleTradePersons, data.Paging).ToList();
			}
			else
			{
				possibleTradeSchedule = getBulletinSchedulesWithTimeFilter(period, possibleTradePersons, data.Paging, data.TimeFilter).ToList();
			}
			var possibleTradeScheduleNum = possibleTradeSchedule.Any() ? possibleTradeSchedule.First().Total : 0;
			var pageCount = possibleTradeScheduleNum % data.Paging.Take != 0 ? possibleTradeScheduleNum / data.Paging.Take + 1 : possibleTradeScheduleNum / data.Paging.Take;

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

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getBulletinSchedules(DateTimePeriod mySchedulePeriod, DatePersons datePersons, Paging paging)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrieveBulletinTradeSchedules(datePersons.Date, datePersons.Persons, mySchedulePeriod, paging);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getBulletinSchedulesWithTimeFilter(DateTimePeriod mySchedulePeriod, DatePersons datePersons, Paging paging, TimeFilterInfo filter)
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrieveBulletinTradeSchedulesWithTimeFilter(datePersons.Date, datePersons.Persons, mySchedulePeriod, paging, filter);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
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

		private IEnumerable<ShiftTradeAddPersonScheduleViewModel> getFilteredTimesPossibleTradeSchedules(DatePersons datePersons, Paging paging, TimeFilterInfo timeFilter )
		{
			if (datePersons.Persons.Any())
			{
				var schedules = _shiftTradeRequestProvider.RetrievePossibleTradeSchedulesWithFilteredTimes(datePersons.Date, datePersons.Persons, paging, timeFilter);
				return _shiftTradePersonScheduleViewModelMapper.Map(schedules);
			}

			return new List<ShiftTradeAddPersonScheduleViewModel>();
		}
	}
}