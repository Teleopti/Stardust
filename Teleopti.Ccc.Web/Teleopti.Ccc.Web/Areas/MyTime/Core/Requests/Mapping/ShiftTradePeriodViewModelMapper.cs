using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradePeriodViewModelMapper : IShiftTradePeriodViewModelMapper
	{
		public ShiftTradeRequestsPeriodViewModel Map(IWorkflowControlSet workflowControlSet)
		{
			var vm = new ShiftTradeRequestsPeriodViewModel { HasWorkflowControlSet = workflowControlSet != null };

			if (workflowControlSet != null)
			{
				vm.OpenPeriodRelativeStart = workflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum;
				vm.OpenPeriodRelativeEnd = workflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum;
			}

			return vm;
		}
	}
}