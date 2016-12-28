using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasUnderstaffing : Specification<IValidatePeriod>
    {
        private readonly ISkill _skill;

        public IntervalHasUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(IValidatePeriod obj)
        {
            return obj.RelativeDifference < _skill.StaffingThresholds.Understaffing.Value;
        }

    }
}