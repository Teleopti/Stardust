namespace Teleopti.Ccc.Domain.Cascading
{
	public class AddResourceToSubSkillsProvider : IAddResourceToSubSkillsProvider
	{
		public IAddResourcesToSubSkills Fetch(bool primarySkillIsClosed)
		{
			return primarySkillIsClosed ? 
				new AddResourcesToSubSkillsWhenPrimaryIsClosed() : 
				new AddResourcesToSubSkills();
		}
	}
}