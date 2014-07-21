using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeScheduleViewModelMapper
	{
		ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data);
		ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelDataForAllTeams data);
	}
}