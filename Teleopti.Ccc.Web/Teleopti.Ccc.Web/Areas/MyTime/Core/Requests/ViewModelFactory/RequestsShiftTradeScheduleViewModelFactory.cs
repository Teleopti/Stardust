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
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;
		private readonly IShiftTradePersonScheduleProvider _personScheduleProvider;

		public RequestsShiftTradeScheduleViewModelFactory(
														ILoggedOnUser loggedOnUser, 
														ITeamScheduleProjectionProvider projectionProvider, 
														IPermissionProvider permissionProvider, 
														IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider, 
														IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper, IShiftTradePersonScheduleProvider personScheduleProvider)
		{
			_loggedOnUser = loggedOnUser;
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
			_personScheduleProvider = personScheduleProvider;
		}

		public ShiftTradeScheduleViewModel CreateViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var ret = new ShiftTradeScheduleViewModel();
			ret.MySchedule = makeMyScheduleViewModel(inputData);
			if (ret.MySchedule.IsFullDayAbsence)
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
					.Where(ps => !_projectionProvider.IsFullDayAbsence(ps.Item2));

				var allSortedPossibleSchedules = possiblePersonSchedules
					.OrderBy(pair => TeamScheduleSortingUtil.GetSortedValue(pair.Item2, false, true))
					.ThenBy(pair => pair.Item1.Name.LastName)
					.Select(pair =>
					{
						var person = pair.Item1;
						var schedule = pair.Item2;
						return
							new ShiftTradeAddPersonScheduleViewModel(_projectionProvider.MakeScheduleReadModel(person, schedule,
								_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential)));
					}).ToList();
				ret.PageCount = (int)Math.Ceiling((double)allSortedPossibleSchedules.Count() / inputData.Paging.Take);
				ret.PossibleTradeSchedules = allSortedPossibleSchedules.Skip(inputData.Paging.Skip).Take(inputData.Paging.Take);
			}
			
			ret.TimeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(ret.MySchedule, ret.PossibleTradeSchedules,
				inputData.ShiftTradeDate);
		
			return ret;
		}

		private ShiftTradeAddPersonScheduleViewModel makeMyScheduleViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var myScheduleDay = _permissionProvider.IsPersonSchedulePublished(inputData.ShiftTradeDate,
				_loggedOnUser.CurrentUser())
				? _personScheduleProvider.GetScheduleForPersons(inputData.ShiftTradeDate, new[] {_loggedOnUser.CurrentUser()}).SingleOrDefault()
				: null;
			var myScheduleViewModel = _projectionProvider.MakeScheduleReadModel(_loggedOnUser.CurrentUser(), myScheduleDay, true);
			return new ShiftTradeAddPersonScheduleViewModel(myScheduleViewModel);
		}

	}
}