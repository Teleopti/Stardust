using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonalSkills : IPersonalSkills
	{
		public IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period)
		{
			return period.PersonSkillCollection.Where(personSkill => !((IDeleteTag)personSkill.Skill).IsDeleted && personSkill.Active);
		}
	}
}