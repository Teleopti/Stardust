using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestProvider
	{
		IWorkflowControlSet RetrieveUserWorkflowControlSet();
		IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date);
		IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons, Paging paging);

		IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedulesWithFilteredTimes(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons,
		                                                                                         Paging paging, IEnumerable<TimePeriod> filteredStartTimes);
		Guid? RetrieveMyTeamId(DateOnly date);
	}
}