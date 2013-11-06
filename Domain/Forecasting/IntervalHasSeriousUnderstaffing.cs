using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class IntervalHasSeriousUnderstaffing : Specification<ISkillStaffPeriod>
    {
        private readonly ISkill _skill;

        public IntervalHasSeriousUnderstaffing(ISkill skill)
        {
            _skill = skill;
        }

        public override bool IsSatisfiedBy(ISkillStaffPeriod obj)
        {
            return obj.RelativeDifference < _skill.StaffingThresholds.SeriousUnderstaffing.Value;
        }
    }
}