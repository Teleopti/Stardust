using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_FasterLoading_46307)]
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