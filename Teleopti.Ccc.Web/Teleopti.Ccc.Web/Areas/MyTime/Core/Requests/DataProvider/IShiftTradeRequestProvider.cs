using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestProvider
	{
		IWorkflowControlSet RetrieveUserWorkflowControlSet();
		IScheduleDay RetrieveMyScheduledDay(DateOnly date);
		IEnumerable<IScheduleDay> RetrievePossibleTradePersonsScheduleDay(DateOnly date);
	}
}