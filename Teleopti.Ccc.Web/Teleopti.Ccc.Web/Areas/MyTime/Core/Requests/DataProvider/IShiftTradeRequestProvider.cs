using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestProvider
	{
		IWorkflowControlSet RetrieveUserWorkflowControlSet();
		IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date);
		IScheduleDictionary RetrieveTradeMultiSchedules(DateOnlyPeriod period, IList<IPerson> personList);
		IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedules(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons, Paging paging, string timeSortOrder = "");
		IEnumerable<IPersonScheduleDayReadModel> RetrieveBulletinTradeSchedules(IEnumerable<string> shiftExchangeOfferIds, Paging paging, string timeSortOrder = "");
		IEnumerable<IPersonScheduleDayReadModel> RetrievePossibleTradeSchedulesWithFilteredTimes(DateOnly date,
			IEnumerable<IPerson> possibleShiftTradePersons, Paging paging,
			TimeFilterInfo filterInfo, string timeSortOrder = "");
		Guid? RetrieveMyTeamId(DateOnly date);
		Guid? RetrieveMySiteId(DateOnly date);
	}
}