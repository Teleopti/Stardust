using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeAddPersonScheduleViewModelMapper
	{
		ShiftTradeAddPersonScheduleViewModel Map(IPersonScheduleDayReadModel scheduleReadModel, bool isMySchedule);
		IList<ShiftTradeAddPersonScheduleViewModel> Map(IEnumerable<IPersonScheduleDayReadModel> scheduleReadModels);
	}
}