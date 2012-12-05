using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class ShiftTradeRequestProvider : IShiftTradeRequestProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradeRequestProvider(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public ShiftTradeRequestsPreparationDomainData RetrieveShiftTradePreparationData()
		{
			return new ShiftTradeRequestsPreparationDomainData{ WorkflowControlSet = _loggedOnUser.CurrentUser().WorkflowControlSet};
		}
	}
}