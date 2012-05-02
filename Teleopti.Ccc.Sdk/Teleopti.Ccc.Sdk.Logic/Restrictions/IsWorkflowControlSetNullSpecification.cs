using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Sdk.Logic.Restrictions
{
    public class IsWorkflowControlSetNullSpecification : Specification<IShiftTradeAvailableCheckItem>
    {
        public override bool IsSatisfiedBy(IShiftTradeAvailableCheckItem obj)
        {
            return obj.PersonFrom.WorkflowControlSet != null && obj.PersonTo.WorkflowControlSet != null;
        }
    }
}