using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    public class IsShiftTradeRequestNotNullSpecification : Specification<IShiftTradeRequest>
    {
        public override bool IsSatisfiedBy(IShiftTradeRequest obj)
        {
            return obj != null;
        }
    }
}
