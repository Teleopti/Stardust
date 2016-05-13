using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class NonDeletedPersonSkillReducer : IPersonSkillReducer
	{
		public IEnumerable<IPersonSkill> Reduce(IPersonPeriod personPeriod)
		{
			return personPeriod.PersonSkillCollection.Where(personSkill => !((IDeleteTag)personSkill.Skill).IsDeleted);
		}
	}
}