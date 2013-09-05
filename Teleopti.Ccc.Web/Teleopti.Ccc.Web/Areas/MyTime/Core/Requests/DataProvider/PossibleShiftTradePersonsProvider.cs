using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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

		public PossibleShiftTradePersonsProvider(IPersonRepository personRepository, 
																					IShiftTradeLightValidator shiftTradeValidator, 
																					ILoggedOnUser loggedOnUser,
																					IPermissionProvider permissionProvider)
		{
			_personRepository = personRepository;
			_shiftTradeValidator = shiftTradeValidator;
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<IPerson> RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments)
		{
			var me = _loggedOnUser.CurrentUser();

			return _personRepository.FindPossibleShiftTrades(me, shiftTradeArguments.LoadOnlyMyTeam, shiftTradeArguments.ShiftTradeDate)
				.Where(person => _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, shiftTradeArguments.ShiftTradeDate, person) &&
							_shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(shiftTradeArguments.ShiftTradeDate, me, person)).Value);
		}
	}
}