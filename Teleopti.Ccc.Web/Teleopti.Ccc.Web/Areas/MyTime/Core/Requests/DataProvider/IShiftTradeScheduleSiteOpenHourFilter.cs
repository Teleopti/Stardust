using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeScheduleSiteOpenHourFilter
	{
		IEnumerable<ShiftTradeAddPersonScheduleViewModel> Filter(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> shiftTradeAddPersonScheduleViews, DatePersons datePersons);
	}
}
