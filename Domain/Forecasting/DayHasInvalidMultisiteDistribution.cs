using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class DayHasInvalidMultisiteDistribution : Specification<IMultisiteDay>
    {
        public override bool IsSatisfiedBy(IMultisiteDay obj)
        {
            return obj.MultisitePeriodCollection.Any(m => !m.IsValid);
        }
    }
}