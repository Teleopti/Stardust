using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasBelowMinAgents : Specification<IEnumerable<ISkillStaffPeriod>>
    {
        private readonly IntervalHasBelowMinAgents _intervalHasBelowMinAgents = new IntervalHasBelowMinAgents();

        public override bool IsSatisfiedBy(IEnumerable<ISkillStaffPeriod> obj)
        {
            return obj.Any(s => _intervalHasBelowMinAgents.IsSatisfiedBy(s));
        }
    }
}