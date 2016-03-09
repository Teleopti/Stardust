using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
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
		private readonly IPersonForScheduleFinder _personForScheduleFinder;

		public PossibleShiftTradePersonsProvider(IPersonRepository personRepository, 
																					IShiftTradeLightValidator shiftTradeValidator, 
																					ILoggedOnUser loggedOnUser,
																					IPermissionProvider permissionProvider,
																					IPersonForScheduleFinder personForScheduleFinder)
		{
			_personRepository = personRepository;
			_shiftTradeValidator = shiftTradeValidator;
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
			_personForScheduleFinder = personForScheduleFinder;
		}

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments)
		{
			var me = _loggedOnUser.CurrentUser();

			var personForShiftTradeList = _personForScheduleFinder.GetPersonFor(shiftTradeArguments.ShiftTradeDate, shiftTradeArguments.TeamIdList,shiftTradeArguments.SearchNameText);

			personForShiftTradeList = personForShiftTradeList.Where(
				personGuid => personGuid.PersonId != me.Id &&
				(_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules,shiftTradeArguments.ShiftTradeDate, personGuid) ||
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))).ToList();

			var personGuidList = personForShiftTradeList.Select(item => item.PersonId).ToList();

			var personList = _personRepository.FindPeople(personGuidList);


			var temp =  new DatePersons
				{
					Date = shiftTradeArguments.ShiftTradeDate,
					Persons =
						personList.Where(
							person =>
							_shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(shiftTradeArguments.ShiftTradeDate, me, person))
												.Value && (_permissionProvider.IsPersonSchedulePublished(shiftTradeArguments.ShiftTradeDate, person) ||
												_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)))
				};

			return temp;
		}
	}
}