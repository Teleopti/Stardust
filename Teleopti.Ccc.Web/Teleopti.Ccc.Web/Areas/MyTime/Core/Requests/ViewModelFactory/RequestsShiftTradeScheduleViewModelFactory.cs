using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
			var ret = new ShiftTradeScheduleViewModel();
			ret.MySchedule = _personScheduleViewModelMapper.MakeMyScheduleViewModel(inputData);

			if (ret.MySchedule.IsFullDayAbsence || ret.MySchedule.IsDayOff
				|| (ret.MySchedule.ScheduleLayers.Any() && ret.MySchedule.ScheduleLayers.All(l => l.IsOvertime)))
			{
				ret.PossibleTradeSchedules = new List<ShiftTradeAddPersonScheduleViewModel>();
				ret.PageCount = 0;
			}
			else
			{
				var possibleTradedPersonList = _possibleShiftTradePersonsProvider.RetrievePersons(inputData).Persons.ToList();

				var possibleTradeSchedules = _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, possibleTradedPersonList);

				var possiblePersonSchedules = possibleTradedPersonList.Select(
					p => new Tuple<IPerson, IScheduleDay>(p, possibleTradeSchedules.SingleOrDefault(s => s.Person.Id == p.Id)))
					.Where(ps => !_projectionProvider.IsFullDayAbsence(ps.Item2) && !_projectionProvider.IsOvertimeOnDayOff(ps.Item2)).ToArray();

				var canViewUnpublished = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
				var canViewConfidential = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential);

				possiblePersonSchedules = possiblePersonSchedules.Where (pair => _shiftTradeSiteOpenHourFilter.FilterSchedule (pair.Item2, ret.MySchedule)).ToArray();

				Array.Sort(possiblePersonSchedules, new TeamScheduleComparer(canViewUnpublished, _permissionProvider));

				var allSortedPossibleSchedules = possiblePersonSchedules.Skip (inputData.Paging.Skip).Take (inputData.Paging.Take)
					.Select(pair =>
					{
						var person = pair.Item1;
						var scheduleDay = pair.Item2;
						var scheduleReadModel = _projectionProvider.MakeScheduleReadModel(person, scheduleDay, canViewConfidential);
						return new ShiftTradeAddPersonScheduleViewModel(scheduleReadModel);
					}).ToList();

				ret.PageCount = (int)Math.Ceiling((double)possiblePersonSchedules.Length / inputData.Paging.Take);
				ret.PossibleTradeSchedules = allSortedPossibleSchedules;
			}

			ret.TimeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(ret.MySchedule, ret.PossibleTradeSchedules,
				inputData.ShiftTradeDate);

			return ret;
		}
	}
}