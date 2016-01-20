using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.FSharp.Core;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;
		private readonly IShiftTradeTimeLineHoursViewModelMapper _shiftTradeTimeLineHoursViewModelMapper;

		public RequestsShiftTradeScheduleViewModelFactory(
														ILoggedOnUser loggedOnUser, 
														IScheduleRepository scheduleRepository, 
														ICurrentScenario currentScenario, 
														ITeamScheduleProjectionProvider projectionProvider, 
														IPermissionProvider permissionProvider, 
														IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider, 
														IShiftTradeTimeLineHoursViewModelMapper shiftTradeTimeLineHoursViewModelMapper
														)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleRepository = scheduleRepository;
			_currentScenario = currentScenario;
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
			_shiftTradeTimeLineHoursViewModelMapper = shiftTradeTimeLineHoursViewModelMapper;
		}

		public ShiftTradeScheduleViewModel CreateViewModel(ShiftTradeScheduleViewModelData inputData)
		{
			var myScheduleDay = _permissionProvider.IsPersonSchedulePublished(inputData.ShiftTradeDate,
				_loggedOnUser.CurrentUser())
				? getScheduleForPersons(inputData.ShiftTradeDate, new[] {_loggedOnUser.CurrentUser()}).SingleOrDefault()
				: null;
			var myScheduleViewModel = _projectionProvider.MakeScheduleReadModel(_loggedOnUser.CurrentUser(), myScheduleDay, true);
			var ret = new ShiftTradeScheduleViewModel();
			ret.MySchedule = new ShiftTradeAddPersonScheduleViewModel(myScheduleViewModel);
			var possibleTradedPersonList = _possibleShiftTradePersonsProvider.RetrievePersons(inputData).Persons.ToList();

			var possibleTradeSchedules = getScheduleForPersons(inputData.ShiftTradeDate, possibleTradedPersonList);

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
			ret.PageCount = (int)Math.Ceiling((double)allSortedPossibleSchedules.Count()/inputData.Paging.Take);
			ret.PossibleTradeSchedules = allSortedPossibleSchedules.Skip(inputData.Paging.Skip).Take(inputData.Paging.Take);
			ret.TimeLineHours = _shiftTradeTimeLineHoursViewModelMapper.Map(ret.MySchedule, ret.PossibleTradeSchedules,
				inputData.ShiftTradeDate);
		
			return ret;
		}


		private IEnumerable<IScheduleDay> getScheduleForPersons(DateOnly date, IEnumerable<IPerson> persons)
		{
			var defaultScenario = _currentScenario.Current();

			var dictionary = _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(
				persons,
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
				defaultScenario);

			return dictionary.SchedulesForDay(date);
		}
	}
}