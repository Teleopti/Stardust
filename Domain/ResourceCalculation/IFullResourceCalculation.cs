using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public interface IFullResourceCalculation
	{
		void Execute();
	}
}