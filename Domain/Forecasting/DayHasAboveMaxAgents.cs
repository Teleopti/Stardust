using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasAboveMaxAgents : Specification<IEnumerable<ISkillStaffPeriod>>
    {
        private readonly IntervalHasAboveMaxAgents _intervalHasAboveMaxAgents = new IntervalHasAboveMaxAgents();

        public override bool IsSatisfiedBy(IEnumerable<ISkillStaffPeriod> obj)
        {
            return obj.Any(s => _intervalHasAboveMaxAgents.IsSatisfiedBy(s));
        }
    }
}