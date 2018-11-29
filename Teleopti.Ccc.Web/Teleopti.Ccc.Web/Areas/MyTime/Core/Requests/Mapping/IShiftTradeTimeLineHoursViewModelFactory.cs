using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeTimeLineHoursViewModelFactory
	{
		IEnumerable<ShiftTradeTimeLineHoursViewModel> CreateTimeLineHours(DateTimePeriod timeLinePeriod);
	}
}