﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsShiftTradeScheduleViewModelFactory : IRequestsShiftTradeScheduleViewModelFactory
	{
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;
		private readonly IShiftTradePersonScheduleProvider _personScheduleProvider;
		private readonly IShiftTradePersonScheduleViewModelMapper _personScheduleViewModelMapper;
		private readonly IShiftTradeSiteOpenHourFilter _shiftTradeSiteOpenHourFilter;

		public RequestsShiftTradeScheduleViewModelFactory(ITeamScheduleProjectionProvider projectionProvider,
			IPermissionProvider permissionProvider,
			IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider,
			IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper,
			IShiftTradePersonScheduleProvider personScheduleProvider,
			IShiftTradePersonScheduleViewModelMapper personScheduleViewModelMapper,
			IShiftTradeSiteOpenHourFilter shiftTradeSiteOpenHourFilter)
		{
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
			_personScheduleProvider = personScheduleProvider;
			_personScheduleViewModelMapper = personScheduleViewModelMapper;
			_shiftTradeSiteOpenHourFilter = shiftTradeSiteOpenHourFilter;
		}

		public ShiftTradeScheduleViewModel CreateViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var pageCount = 0;
			var possibleTradeSchedules = new List<ShiftTradeAddPersonScheduleViewModel>();

			var mySchedule = _personScheduleViewModelMapper.MakeMyScheduleViewModel(inputData);
			var isDayOffWithOvertimeOnly = mySchedule.IsDayOff && mySchedule.ScheduleLayers.Any() &&
									   mySchedule.ScheduleLayers.All(l => l.IsOvertime);

			if (!mySchedule.IsFullDayAbsence && !isDayOffWithOvertimeOnly)
			{
				var allPossibleTradedPersonList = _possibleShiftTradePersonsProvider.RetrievePersons(inputData).Persons.ToList();
				var allPossibleTradeSchedules = _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate,
					allPossibleTradedPersonList);
				var allPossiblePersonSchedules = allPossibleTradedPersonList.Select(
						p => new Tuple<IPerson, IScheduleDay>(p, allPossibleTradeSchedules.SingleOrDefault(s => s.Person.Id == p.Id)))
					.Where(ps => !_projectionProvider.IsFullDayAbsence(ps.Item2)
								 && !_projectionProvider.IsOvertimeOnDayOff(ps.Item2)
								 && _shiftTradeSiteOpenHourFilter.FilterSchedule(ps.Item2, mySchedule))
					.ToArray();
				pageCount = (int)Math.Ceiling((double)allPossiblePersonSchedules.Length / inputData.Paging.Take);

				var canViewUnpublished =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
				var canViewConfidential =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential);
				Array.Sort(allPossiblePersonSchedules, new TeamScheduleComparer(canViewUnpublished, _permissionProvider));

				possibleTradeSchedules = allPossiblePersonSchedules
					.Skip(inputData.Paging.Skip).Take(inputData.Paging.Take)
					.Select(pair =>
					{
						var person = pair.Item1;
						var scheduleDay = pair.Item2;
						var scheduleReadModel = _projectionProvider.MakeScheduleReadModel(person, scheduleDay, canViewConfidential);
						return new ShiftTradeAddPersonScheduleViewModel(scheduleReadModel);
					}).ToList();
			}

			var timeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(mySchedule, possibleTradeSchedules,
				inputData.ShiftTradeDate);

			return new ShiftTradeScheduleViewModel
			{
				MySchedule = mySchedule,
				PossibleTradeSchedules = possibleTradeSchedules,
				PageCount = pageCount,
				TimeLineHours = timeLineHours
			};
		}
	}
}