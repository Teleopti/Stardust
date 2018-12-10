using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradePersonProvider : IShiftTradePersonProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IShiftTradeLightValidator _shiftTradeValidator;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPeopleForShiftTradeFinder _peopleForShiftTradeFinder;
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradePersonProvider(IPersonRepository personRepository, IShiftTradeLightValidator shiftTradeValidator, IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser, IPeopleForShiftTradeFinder peopleForShiftTradeFinder)
		{
			_personRepository = personRepository;
			_shiftTradeValidator = shiftTradeValidator;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
			_peopleForShiftTradeFinder = peopleForShiftTradeFinder;
		}

		public IEnumerable<IPerson> RetrievePeopleOptimized(DateOnly shiftTradeDate, Guid[] teamIds, string personName,
			NameFormatSetting nameFormatSettings)
		{
			var personForShiftTradeList = _peopleForShiftTradeFinder.GetPeople(_loggedOnUser.CurrentUser(), shiftTradeDate,
				teamIds, personName,
				nameFormatSettings);

			return processShiftTradePeople(shiftTradeDate, personForShiftTradeList);
		}

		public IEnumerable<IPerson> RetrievePeopleOptimized(DateOnly shiftTradeDate, Guid[] peopleIds)
		{
			var personForShiftTradeList = _peopleForShiftTradeFinder.GetPeople(_loggedOnUser.CurrentUser(), shiftTradeDate,
				peopleIds);

			return processShiftTradePeople(shiftTradeDate, personForShiftTradeList);
		}

		private IEnumerable<IPerson> processShiftTradePeople(DateOnly shiftTradeDate, IList<IPersonAuthorization> personForShiftTradeList)
		{
			var me = _loggedOnUser.CurrentUser();

			personForShiftTradeList = personForShiftTradeList.Where(
				personGuid => personGuid.PersonId != me.Id &&
							  (_permissionProvider.HasOrganisationDetailPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules,
								   shiftTradeDate, personGuid) ||
							   _permissionProvider.HasApplicationFunctionPermission(
								   DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))).ToList();

			var personGuidList = personForShiftTradeList.Select(item => item.PersonId).ToList();

			var personList = _personRepository.FindPeople(personGuidList);

			return personList.Where(
				person => _shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(shiftTradeDate, me, person))
						.IsOk && (_permissionProvider.IsPersonSchedulePublished(shiftTradeDate, person) ||
								   _permissionProvider.HasApplicationFunctionPermission(
									   DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules)));
		}
	}

	public interface IShiftTradePersonProvider
	{
		IEnumerable<IPerson> RetrievePeopleOptimized(DateOnly shiftTradeDate, Guid[] teamIds, string personName,
			NameFormatSetting nameFormatSettings);

		IEnumerable<IPerson> RetrievePeopleOptimized(DateOnly shiftTradeDate, Guid[] peopleIds);
	}
}