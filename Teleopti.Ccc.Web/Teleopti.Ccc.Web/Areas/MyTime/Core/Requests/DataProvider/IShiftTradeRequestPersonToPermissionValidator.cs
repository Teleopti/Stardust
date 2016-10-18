using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestPersonToPermissionValidator
	{
		bool IsSatisfied(IShiftTradeRequest request);
	}
}