using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    public class IsWorkflowControlSetNullSpecification : ShiftTradeSpecification
    {
        public override string DenyReason
        {
            get { return "WorkflowControlSetNotSetDenyReason"; }
        }

		  public override bool IsSatisfiedBy(IEnumerable<IShiftTradeSwapDetail> obj)
        {
            var detail = obj.FirstOrDefault();
            return detail != null && 
                   detail.PersonFrom.WorkflowControlSet != null &&
                   detail.PersonTo.WorkflowControlSet != null;
        }
    }
}
