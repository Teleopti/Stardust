using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasOverstaffing : Specification<ISkillStaffPeriod>
    {
        private readonly ISkill _skill;

        public IntervalHasOverstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
        {
            return obj.RelativeDifference > _skill.StaffingThresholds.Overstaffing.Value;
        }
    }
}