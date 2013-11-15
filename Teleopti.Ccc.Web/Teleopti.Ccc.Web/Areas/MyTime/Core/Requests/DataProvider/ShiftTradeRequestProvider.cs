using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser, IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
		}

		public IWorkflowControlSet RetrieveUserWorkflowControlSet()
		{
			return _loggedOnUser.CurrentUser().WorkflowControlSet;
		}

		public IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date)
		{
			var person = _loggedOnUser.CurrentUser();
			return _scheduleDayReadModelFinder.ForPerson(date, person.Id.Value);
		}

		public IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons, Paging paging)
		{
			IEnumerable<Guid> personIdList = (from person in possibleShiftTradePersons
			                                 select person.Id.Value).ToList();
			return _scheduleDayReadModelFinder.ForPersons(date, personIdList, paging);
		}
	}
}