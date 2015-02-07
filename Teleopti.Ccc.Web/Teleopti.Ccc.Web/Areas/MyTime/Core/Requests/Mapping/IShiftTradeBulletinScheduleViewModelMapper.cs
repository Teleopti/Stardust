using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeBulletinScheduleViewModelMapper
	{
		ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data);
	}
}