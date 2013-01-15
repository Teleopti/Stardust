using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class OpenShiftTradePeriodSpecification : Specification<ShiftTradeAvailableCheckItem>, IShiftTradeLightSpecification
	{
		public override bool IsSatisfiedBy(ShiftTradeAvailableCheckItem obj)
		{
			if (obj.PersonFrom.WorkflowControlSet == null || obj.PersonTo.WorkflowControlSet == null)
				return false;
			var currentDate = DateOnly.Today;
			var openPeriodFrom =
				 new DateOnlyPeriod(
					  currentDate.AddDays(obj.PersonFrom.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum),
					  currentDate.AddDays(obj.PersonFrom.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum));
			var openPeriodTo =
				 new DateOnlyPeriod(
					  currentDate.AddDays(obj.PersonTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Minimum),
					  currentDate.AddDays(obj.PersonTo.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward.Maximum));
			return openPeriodFrom.Contains(obj.DateOnly) && openPeriodTo.Contains(obj.DateOnly);
		}

		public string DenyReason
		{
			get { return "OpenShiftTradePeriodDenyReason"; }
		}
	}
}
