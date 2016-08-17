using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface IShiftTradeSiteOpenHourFilter
	{
		IEnumerable<ShiftTradeAddPersonScheduleViewModel> FilterScheduleView(
			IEnumerable<ShiftTradeAddPersonScheduleViewModel> shiftTradeAddPersonScheduleViews,
			ShiftTradeAddPersonScheduleViewModel fromPersonScheduleView, DatePersons datePersons);

		IEnumerable<IShiftExchangeOffer> FilterShiftExchangeOffer(IEnumerable<IShiftExchangeOffer> shiftExchangeOffers,
			ShiftTradeAddPersonScheduleViewModel personFromScheduleView);
	}
}
