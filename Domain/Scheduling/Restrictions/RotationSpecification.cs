using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
    public class RotationSpecification : Specification<IRestrictionBase>
    {
        public override bool IsSatisfiedBy(IRestrictionBase obj)
        {
            return obj is IRotationRestriction;
        }
    }
}
