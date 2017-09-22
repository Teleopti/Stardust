using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeSiteOpenHourFilter
	{
		IEnumerable<ShiftTradeAddPersonScheduleViewModel> FilterScheduleView(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> shiftTradeAddPersonScheduleViews,
			ShiftTradeAddPersonScheduleViewModel fromPersonScheduleView, DatePersons datePersons);

		bool FilterShiftExchangeOffer(IShiftExchangeOffer shiftExchangeOffer,
			ShiftTradeAddPersonScheduleViewModel personFromScheduleView);

		bool FilterSchedule (IScheduleDay toScheduleDay, ShiftTradeAddPersonScheduleViewModel personFromScheduleView);
	}
}
