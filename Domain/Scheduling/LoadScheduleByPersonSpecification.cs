using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class LoadScheduleByPersonSpecification : Specification<ILoaderDeciderResult>
    {
		public override bool IsSatisfiedBy(ILoaderDeciderResult obj)
        {
            if (obj == null)
                return false;
            return obj.PercentageOfPeopleFiltered <= 70;
        }
    }
}