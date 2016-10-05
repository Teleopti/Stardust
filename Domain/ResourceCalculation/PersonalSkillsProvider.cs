using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonalSkillsProvider
	{
		private readonly PersonalSkills personalSkills = new PersonalSkills();
		private readonly CascadingPersonalSkills cascadingPersonSkills = new CascadingPersonalSkills();

		public IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period)
		{
			return cascadingPersonSkills.PersonSkills(period);
		}

		public IEnumerable<IPersonSkill> PersonSkillsBasedOnPrimarySkill(IPersonPeriod period)
		{
			return ResourceCalculationContext.PrimarySkillMode() ? 
				cascadingPersonSkills.PersonSkills(period) : 
				personalSkills.PersonSkills(period);
		}
	}
}