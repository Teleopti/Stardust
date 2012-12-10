using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
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

		public ShiftTradeRequestsPreparationDomainData RetrieveShiftTradePreparationData(DateOnly date)
		{
			IPerson person = _loggedOnUser.CurrentUser();
			return new ShiftTradeRequestsPreparationDomainData
			       	{
			       		WorkflowControlSet = person.WorkflowControlSet,
						MyScheduleDay = _scheduleProvider.GetScheduleForPeriod(new DateOnlyPeriod(date, date)).FirstOrDefault()
			       	};
		}
	}
}