using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    public class ShouldShiftTradeBeAutoGrantedSpecification : Specification<IShiftTradeRequest>
    {
        public override bool IsSatisfiedBy(IShiftTradeRequest obj)
        {
            return obj.InvolvedPeople().All(p => 
                                            p.WorkflowControlSet != null &&
                                            p.WorkflowControlSet.AutoGrantShiftTradeRequest);
        }
    }
}