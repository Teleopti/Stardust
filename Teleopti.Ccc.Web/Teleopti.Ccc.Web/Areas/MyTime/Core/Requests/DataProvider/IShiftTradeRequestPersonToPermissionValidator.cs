using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestPersonToPermissionValidator
	{
		bool IsSatisfied(IShiftTradeRequest request);
	}
}