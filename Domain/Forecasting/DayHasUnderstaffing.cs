using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasUnderstaffing : Specification<IEnumerable<ISkillStaffPeriod>>
    {
        private readonly IntervalHasUnderstaffing _intervalHasUnderstaffing;

        public DayHasUnderstaffing(ISkill skill)
        {
            _intervalHasUnderstaffing = new IntervalHasUnderstaffing(skill);
        }

        public override bool IsSatisfiedBy(IEnumerable<ISkillStaffPeriod> obj)
        {
            return obj.Any(s => _intervalHasUnderstaffing.IsSatisfiedBy(s));
        }
    }
}