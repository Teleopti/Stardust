using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestProvider
	{
		ShiftTradeRequestsPreparationDomainData RetrieveShiftTradePreparationData();
	}
}