using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PossibleShiftTradePersonsProvider : IPossibleShiftTradePersonsProvider
	{
		private readonly IPersonRepository _personRepository;
		private readonly IShiftTradeLightValidator _shiftTradeValidator;
		private readonly ILoggedOnUser _loggedOnUser;

		public PossibleShiftTradePersonsProvider(IPersonRepository personRepository, IShiftTradeLightValidator shiftTradeValidator, ILoggedOnUser loggedOnUser)
		{
			_personRepository = personRepository;
			_shiftTradeValidator = shiftTradeValidator;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<IPerson> RetrievePersons(DateOnly dateOnly)
		{
			var me = _loggedOnUser.CurrentUser();
			var possibleTradePersons = new List<IPerson>();

			foreach (var person in _personRepository.FindPossibleShiftTrades(me))
			{
				var result = _shiftTradeValidator.Validate(new ShiftTradeAvailableCheckItem(dateOnly, me, person));
				if (result.Value)
					possibleTradePersons.Add(person);
			}
			
			return possibleTradePersons;
		}
	}
}