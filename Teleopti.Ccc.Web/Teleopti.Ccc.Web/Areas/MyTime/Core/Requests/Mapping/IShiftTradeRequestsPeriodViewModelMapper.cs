using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public interface IShiftTradeRequestsPeriodViewModelMapper
	{
		ShiftTradeRequestsPeriodViewModel Map(IWorkflowControlSet workflowControlSet);
	}
}