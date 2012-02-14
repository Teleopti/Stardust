using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    public class IsWorkflowControlSetNullSpecification : ShiftTradeSpecification, IIsWorkflowControlSetNullSpecification
    {
        public override string DenyReason
        {
            get { return "WorkflowControlSetNotSetDenyReason"; }
        }

        public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
        {
            var detail = obj.FirstOrDefault();
            return detail != null && 
                   detail.PersonFrom.WorkflowControlSet != null &&
                   detail.PersonTo.WorkflowControlSet != null;
        }
    }
}
