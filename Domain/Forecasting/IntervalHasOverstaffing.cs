using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasOverstaffing : Specification<IValidatePeriod>
    {
        private readonly ISkill _skill;

        public IntervalHasOverstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(IValidatePeriod obj)
        {
            return obj.RelativeDifference > _skill.StaffingThresholds.Overstaffing.Value;
        }
    }
}