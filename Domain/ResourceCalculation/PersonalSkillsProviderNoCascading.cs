using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class PersonalSkillsProviderNoCascading : IPersonalSkillsProvider
	{
		private readonly PersonalSkills personalSkills = new PersonalSkills();

		public IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period)
		{
			return personalSkills.PersonSkills(period);
		}

		public IEnumerable<IPersonSkill> PersonSkillsBasedOnPrimarySkill(IPersonPeriod period)
		{
			return personalSkills.PersonSkills(period);
		}
	}
}