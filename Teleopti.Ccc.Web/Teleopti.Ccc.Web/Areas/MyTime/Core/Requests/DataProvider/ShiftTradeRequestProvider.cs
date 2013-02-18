using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPossibleShiftTradePersonsProvider _possibleShiftTradePersonsProvider;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser, IScheduleProvider scheduleProvider, IPossibleShiftTradePersonsProvider possibleShiftTradePersonsProvider)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleProvider = scheduleProvider;
			_possibleShiftTradePersonsProvider = possibleShiftTradePersonsProvider;
		}

		public IWorkflowControlSet RetrieveUserWorkflowControlSet()
		{
			return _loggedOnUser.CurrentUser().WorkflowControlSet;
		}

		public IScheduleDay RetrieveMyScheduledDay(DateOnly date)
		{
			return _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)).FirstOrDefault();
		}

		public IEnumerable<IScheduleDay> RetrievePossibleTradePersonsScheduleDay(DateOnly date)
		{
			return _scheduleProvider.GetScheduleForPersons(date, _possibleShiftTradePersonsProvider.RetrievePersons(date));
		}
	}
}