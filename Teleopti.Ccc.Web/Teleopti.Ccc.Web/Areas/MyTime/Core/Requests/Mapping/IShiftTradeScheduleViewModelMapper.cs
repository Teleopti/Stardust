using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeScheduleViewModelMapper
	{
		ShiftTradeScheduleViewModel Map(ShiftTradeScheduleViewModelData data);
		ShiftTradeScheduleViewModel MapForBulletin(ShiftTradeScheduleViewModelData data);
	}
}