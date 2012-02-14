using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasSeriousUnderstaffing : Specification<IEnumerable<ISkillStaffPeriod>>
    {
        private readonly IntervalHasSeriousUnderstaffing _intervalHasSeriousUnderstaffing;

        public DayHasSeriousUnderstaffing(ISkill skill)
        {
            _intervalHasSeriousUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
        }

        public override bool IsSatisfiedBy(IEnumerable<ISkillStaffPeriod> obj)
        {
            return obj.Any(s => _intervalHasSeriousUnderstaffing.IsSatisfiedBy(s));
        }
    }
}