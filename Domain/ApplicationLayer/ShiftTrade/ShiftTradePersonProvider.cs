﻿

using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradePersonProvider : IShiftTradePersonProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IShiftTradeLightValidator _shiftTradeValidator;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonForScheduleFinder _personForScheduleFinder;
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradePersonProvider(IPersonRepository personRepository, IShiftTradeLightValidator shiftTradeValidator, IPermissionProvider permissionProvider, IPersonForScheduleFinder personForScheduleFinder, ILoggedOnUser loggedOnUser)
		{
			_personRepository = personRepository;
			_shiftTradeValidator = shiftTradeValidator;
			_permissionProvider = permissionProvider;
			_personForScheduleFinder = personForScheduleFinder;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<IPerson> RetrievePersons (DateOnly shiftTradeDate, Guid[] teamIds, string personName,
			NameFormatSetting nameFormatSettings)
		{
			var personForShiftTradeList = _personForScheduleFinder.GetPersonFor (shiftTradeDate,
				teamIds, personName,
				nameFormatSettings);

			var me = _loggedOnUser.CurrentUser();

			personForShiftTradeList = personForShiftTradeList.Where (
				personGuid => personGuid.PersonId != me.Id &&
							  (_permissionProvider.HasOrganisationDetailPermission (DefinedRaptorApplicationFunctionPaths.ViewSchedules,
								  shiftTradeDate, personGuid) ||
							   _permissionProvider.HasApplicationFunctionPermission (
								   DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))).ToList();

			var personGuidList = personForShiftTradeList.Select (item => item.PersonId).ToList();

			var personList = _personRepository.FindPeople (personGuidList);



			return personList.Where (
				person =>
					_shiftTradeValidator.Validate (new ShiftTradeAvailableCheckItem (shiftTradeDate, me, person))
						.Value && (_permissionProvider.IsPersonSchedulePublished (shiftTradeDate, person) ||
								   _permissionProvider.HasApplicationFunctionPermission (
									   DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)));

		}
	}


	public interface IShiftTradePersonProvider
	{
		IEnumerable<IPerson> RetrievePersons(DateOnly shiftTradeDate, Guid[] teamIds, string personName,
			NameFormatSetting nameFormatSettings);
	}
}
