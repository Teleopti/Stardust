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
		IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons, Paging paging, string timeSortOrder = "");
		IEnumerable<IPersonScheduleDayReadModel> RetrieveBulletinTradeSchedules(IEnumerable<string> shiftExchangeOfferIds, Paging paging, string timeSortOrder = "");
		IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedulesWithFilteredTimes(DateOnly date,
			IEnumerable<IPerson> possibleShiftTradePersons, Paging paging,
			TimeFilterInfo filterInfo, string timeSortOrder = "");
		Guid? RetrieveMyTeamId(DateOnly date);
	}
}