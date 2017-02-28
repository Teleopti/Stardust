using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasSeriousUnderstaffing : Specification<IEnumerable<IValidatePeriod>>
    {
        private readonly IntervalHasSeriousUnderstaffing _intervalHasSeriousUnderstaffing;

        public DayHasSeriousUnderstaffing(ISkill skill)
        {
            _intervalHasSeriousUnderstaffing = new IntervalHasSeriousUnderstaffing(skill);
        }

        public override bool IsSatisfiedBy(IEnumerable<IValidatePeriod> obj)
        {
            return obj.Any(s => _intervalHasSeriousUnderstaffing.IsSatisfiedBy(s));
        }
    }
}