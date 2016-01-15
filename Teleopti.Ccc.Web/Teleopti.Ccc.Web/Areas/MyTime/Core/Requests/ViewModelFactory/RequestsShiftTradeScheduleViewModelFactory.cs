﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsShiftTradeScheduleViewModelFactory : IRequestsShiftTradeScheduleViewModelFactory
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ICurrentScenario _currentScenario;
		private ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPermissionProvider _permissionProvider;
		private IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;

		public RequestsShiftTradeScheduleViewModelFactory(ILoggedOnUser loggedOnUser, IScheduleRepository scheduleRepository, ICurrentScenario currentScenario, ITeamScheduleProjectionProvider projectionProvider, IPermissionProvider permissionProvider, IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleRepository = scheduleRepository;
			_currentScenario = currentScenario;
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
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


			ret.PossibleTradeSchedules = possibleTradeSchedules.Select(schedule =>
				new ShiftTradeAddPersonScheduleViewModel(_projectionProvider.MakeScheduleReadModel(schedule.Person, schedule, _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential))));
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


	public interface IRequestsShiftTradeScheduleViewModelFactory
	{
		ShiftTradeScheduleViewModel CreateViewModel(ShiftTradeScheduleViewModelData inputData);
	}

}