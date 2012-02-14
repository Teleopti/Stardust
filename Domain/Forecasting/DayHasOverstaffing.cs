using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasOverstaffing : Specification<IEnumerable<ISkillStaffPeriod>>
    {
        private readonly IntervalHasOverstaffing _intervalHasOverstaffing;

        public DayHasOverstaffing(ISkill skill)
        {
            _intervalHasOverstaffing = new IntervalHasOverstaffing(skill);
        }

        public override bool IsSatisfiedBy(IEnumerable<ISkillStaffPeriod> obj)
        {
            return obj.Any(s => _intervalHasOverstaffing.IsSatisfiedBy(s));
        }
    }
}