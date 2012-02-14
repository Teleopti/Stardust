using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class PreferenceSpecification : Specification<IRestrictionBase>
    {
        public override bool IsSatisfiedBy(IRestrictionBase obj)
        {
            return obj is IPreferenceRestriction;
        }
    }
}
