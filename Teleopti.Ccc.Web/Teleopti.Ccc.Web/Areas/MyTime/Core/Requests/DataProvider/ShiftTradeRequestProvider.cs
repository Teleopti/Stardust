using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
		}

		public IWorkflowControlSet RetrieveUserWorkflowControlSet()
		{
			return _loggedOnUser.CurrentUser().WorkflowControlSet;
		}

		public IScheduleDay RetrieveMyScheduledDay(DateOnly date)
		{
			return _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)).FirstOrDefault();
		}

		public IPersonScheduleDayReadModel RetrieveMySchedule(DateOnly date)
		{
			throw new System.NotImplementedException();
		}

		public IEnumerable<IScheduleDay> RetrievePossibleTradePersonsScheduleDay(DateOnly date, IEnumerable<IPerson> possibleShiftTradePersons)
		{
			return _scheduleProvider.GetScheduleForPersons(date, possibleShiftTradePersons);
		}
	}
}