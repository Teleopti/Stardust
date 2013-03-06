using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeRequestPersister
	{
		RequestViewModel Persist(ShiftTradeRequestForm form);
	}
}