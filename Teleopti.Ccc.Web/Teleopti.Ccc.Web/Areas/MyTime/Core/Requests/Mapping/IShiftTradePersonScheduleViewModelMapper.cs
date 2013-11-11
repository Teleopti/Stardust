using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradePersonScheduleViewModelMapper
	{
		ShiftTradePersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel);
		IList<ShiftTradePersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels);
	}
}