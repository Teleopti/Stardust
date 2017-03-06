using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Cascading
{

	[RemoveMeWithToggle(Toggles.ResourcePlanner_NotShovelCorrectly_41763)]
	public interface IAddResourceToSubSkillsProvider
	{
		IAddResourcesToSubSkills Fetch(bool primarySkillIsClosed);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_NotShovelCorrectly_41763)]
	public class AddResourceToSubSkillsProviderOldBehavior : IAddResourceToSubSkillsProvider
	{
		public IAddResourcesToSubSkills Fetch(bool primarySkillIsClosed)
		{
			return new AddResourcesToSubSkillsWhenPrimaryIsOpen();
		}
	}
}