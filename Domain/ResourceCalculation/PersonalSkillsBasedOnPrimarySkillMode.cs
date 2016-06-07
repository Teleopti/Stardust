using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonalSkillsBasedOnPrimarySkillMode : IPersonalSkillsBasedOnPrimarySkillMode
	{
		public IEnumerable<IPersonSkill> PersonalSkills(IPersonPeriod personPeriod)
		{
			var personalSkills = ResourceCalculationContext.PrimarySkillMode()
				? (IPersonalSkills) new CascadingPersonalSkills()
				: new PersonalSkills();
			return personalSkills.PersonSkills(personPeriod);
		}
	}
}