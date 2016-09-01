using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public interface IPersonalSkillsProvider
	{
		IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period);
		IEnumerable<IPersonSkill> PersonSkillsBasedOnPrimarySkill(IPersonPeriod period);
	}
}