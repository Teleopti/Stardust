using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeTimeLineHoursViewModelMapper
	{
		IEnumerable<ShiftTradeTimeLineHoursViewModel> Map(ShiftTradePersonScheduleViewModel mySchedule,
		                                                  IEnumerable<ShiftTradePersonScheduleViewModel>
			                                                  possibleTradeSchedules, DateOnly shiftTradeDate);
	}
}