using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalShrinkageHasSeriousUnderstaffing : Specification<ISkillStaffPeriod>
    {
        private readonly ISkill _skill;

        public IntervalShrinkageHasSeriousUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
        {
				//todo change to correct value to compare with
            return obj.RelativeDifference * (1d + obj.Payload.Shrinkage.Value) < _skill.StaffingThresholds.SeriousUnderstaffing.Value;
        }
    }
}