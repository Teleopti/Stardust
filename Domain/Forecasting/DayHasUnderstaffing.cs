using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasUnderstaffing : Specification<IEnumerable<IValidatePeriod>>
    {
        private readonly IntervalHasUnderstaffing _intervalHasUnderstaffing;

        public DayHasUnderstaffing(ISkill skill)
        {
            _intervalHasUnderstaffing = new IntervalHasUnderstaffing(skill);
        }

        public override bool IsSatisfiedBy(IEnumerable<IValidatePeriod> obj)
        {
            return obj.Any(s => _intervalHasUnderstaffing.IsSatisfiedBy(s));
        }
    }
}