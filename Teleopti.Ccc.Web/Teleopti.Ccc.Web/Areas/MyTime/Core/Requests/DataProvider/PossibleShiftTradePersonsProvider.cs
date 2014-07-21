using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PossibleShiftTradePersonsProvider : IPossibleShiftTradePersonsProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IShiftTradeLightValidator _shiftTradeValidator;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonForShiftTradeRepository _personForShiftTradeRepository;

		public PossibleShiftTradePersonsProvider(IPersonRepository personRepository, 
																					IShiftTradeLightValidator shiftTradeValidator, 
																					ILoggedOnUser loggedOnUser,
																					IPermissionProvider permissionProvider,
																					IPersonForShiftTradeRepository personForShiftTradeRepository)
		{
			_personRepository = personRepository;
			_shiftTradeValidator = shiftTradeValidator;
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
			_personForShiftTradeRepository = personForShiftTradeRepository;
		}

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments)
		{
			var me = _loggedOnUser.CurrentUser();
			var personForShiftTradeList =
				_personForShiftTradeRepository.GetPersonForShiftTrade(shiftTradeArguments.ShiftTradeDate, shiftTradeArguments.TeamId);

			personForShiftTradeList = personForShiftTradeList.Where(
				personGuid => personGuid.PersonId != me.Id &&
				_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
				                                                    shiftTradeArguments.ShiftTradeDate, personGuid)).ToList();

			var personGuidList = personForShiftTradeList.Select(item => item.PersonId).ToList();

			var personList = _personRepository.FindPeople(personGuidList);

			return new DatePersons
				{
					Date = shiftTradeArguments.ShiftTradeDate,
					Persons =
						personList.Where(
							person =>
							_shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(shiftTradeArguments.ShiftTradeDate, me, person))
							                    .Value)
				};
		}

		public DatePersons RetrievePersonsForAllTeams(ShiftTradeScheduleViewModelDataForAllTeams shiftTradeArguments)
		{
			var me = _loggedOnUser.CurrentUser();
			var personForShiftTradeList = new List<IAuthorizeOrganisationDetail>(){};
			foreach (var teamId in shiftTradeArguments.TeamIds)
			{
				personForShiftTradeList = (_personForShiftTradeRepository.GetPersonForShiftTrade(shiftTradeArguments.ShiftTradeDate, teamId)).Union(personForShiftTradeList).ToList();
			}

			personForShiftTradeList = personForShiftTradeList.Where(
				personGuid => personGuid.PersonId != me.Id &&
				_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
																	shiftTradeArguments.ShiftTradeDate, personGuid)).ToList();

			var personGuidList = personForShiftTradeList.Select(item => item.PersonId).ToList();

			var personList = _personRepository.FindPeople(personGuidList);

			return new DatePersons
			{
				Date = shiftTradeArguments.ShiftTradeDate,
				Persons =
					personList.Where(
						person =>
						_shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(shiftTradeArguments.ShiftTradeDate, me, person))
											.Value)
			};
		}
	}
}