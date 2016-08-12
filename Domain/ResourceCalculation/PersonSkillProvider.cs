using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillProvider : IPersonSkillProvider
	{
		public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			var personPeriod = person.Period(date);
			if (personPeriod == null) return new SkillCombination(new ISkill[0], new DateOnlyPeriod(), new SkillEffiencyResource[]{});

			var personSkillCollection = PersonSkills(personPeriod).ToArray();

			var skills = personSkillCollection.Where(s => s.SkillPercentage.Value > 0)
				.Concat(personPeriod.PersonMaxSeatSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Concat(personPeriod.PersonNonBlendSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Select(s => s.Skill)
				.Distinct()
				.ToArray();

			var skillEfficiencies =
				personSkillCollection.Where(
					s => s.SkillPercentage.Value > 0)
					.Select(k => new SkillEffiencyResource(k.Skill.Id.GetValueOrDefault(), k.SkillPercentage.Value)).ToArray();

			return new SkillCombination(skills, personPeriod.Period, skillEfficiencies);
		}

		protected virtual IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod personPeriod)
		{
			return new PersonalSkills().PersonSkills(personPeriod);
		}
	}
}