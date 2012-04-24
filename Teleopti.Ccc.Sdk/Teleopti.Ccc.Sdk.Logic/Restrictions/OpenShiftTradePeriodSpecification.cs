using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class OpenShiftTradePeriodSpecification : Specification<IShiftTradeAvailableCheckItem>
    {
        public override bool IsSatisfiedBy(IShiftTradeAvailableCheckItem obj)
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
    }
}