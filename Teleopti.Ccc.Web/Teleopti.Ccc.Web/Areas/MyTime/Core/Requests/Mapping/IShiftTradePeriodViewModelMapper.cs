using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradePeriodViewModelMapper
	{
		ShiftTradeRequestsPeriodViewModel Map(IWorkflowControlSet workflowControlSet, INow now, TimeZoneInfo timeZone);
	}
}