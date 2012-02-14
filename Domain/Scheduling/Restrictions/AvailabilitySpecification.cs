using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class AvailabilitySpecification : Specification<IRestrictionBase>
    {
        public override bool IsSatisfiedBy(IRestrictionBase obj)
        {
            return obj is IAvailabilityRestriction;
        }
    }
}
