using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
    public abstract class ShiftTradeSpecification : Specification<IList<IShiftTradeSwapDetail>>, IShiftTradeSpecification
    {
       
        public  abstract string DenyReason { get; }

        public ShiftTradeRequestValidationResult Validate(IList<IShiftTradeSwapDetail> obj)
        {
            if (IsSatisfiedBy(obj))
            {
                return new ShiftTradeRequestValidationResult(true);
            }

            return new ShiftTradeRequestValidationResult(false,DenyReason);
        }

        
    }
}
