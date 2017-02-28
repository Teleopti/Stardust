using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonalSkills
	{
		public IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod period)
		{
			return period.PersonSkillCollection.Where(personSkill => !((IDeleteTag)personSkill.Skill).IsDeleted && personSkill.Active);
		}
	}
}