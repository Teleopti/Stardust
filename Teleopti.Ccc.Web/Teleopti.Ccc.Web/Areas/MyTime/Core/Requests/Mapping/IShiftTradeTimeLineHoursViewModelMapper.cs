using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeTimeLineHoursViewModelMapper
	{
		IEnumerable<ShiftTradeTimeLineHoursViewModel> Map(ShiftTradeAddPersonScheduleViewModel mySchedule,
		                                                  IEnumerable<ShiftTradeAddPersonScheduleViewModel>
			                                                  possibleTradeSchedules, DateOnly shiftTradeDate);
	}
}