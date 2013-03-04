using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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

		public IEnumerable<IPerson> RetrievePersons(DateOnly dateOnly)
		{
			var me = _loggedOnUser.CurrentUser();

			return _personRepository.FindPossibleShiftTrades(me)
				.Where(person => _permissionProvider.HasPersonPermission(DefinedRaptorApplicationFunctionPaths.ViewSchedules, dateOnly, person) &&
							_shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(dateOnly, me, person)).Value);
		}
	}
}