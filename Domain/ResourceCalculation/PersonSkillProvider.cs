using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillProvider : IPersonSkillProvider
	{
		private readonly PersonalSkills _personalSkills = new PersonalSkills();

		public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			var personPeriod = person.Period(date);
			if (personPeriod == null) return new SkillCombination(new ISkill[0], new DateOnlyPeriod(), new SkillEffiencyResource[]{}, new ISkill[0]);

			var originalPersonSkills = PersonSkills(personPeriod);
			var personSkillCollection = originalPersonSkills.FilteredSkills.ToArray();

			var skills = personSkillCollection.Where(s => s.SkillPercentage.Value > 0)
				.Concat(personPeriod.PersonNonBlendSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Select(s => s.Skill)
				.Distinct()
				.ToList();

			if (personPeriod.MaxSeatSkill != null)
				skills.Add(personPeriod.MaxSeatSkill);

			var skillEfficiencies =
				personSkillCollection.Where(
					s => s.SkillPercentage.Value > 0)
					.GroupBy(g => g.Skill.Id.GetValueOrDefault())
					.Select(k => new SkillEffiencyResource(k.Key, k.First().SkillPercentage.Value)).ToArray();

			return new SkillCombination(skills.ToArray(), personPeriod.Period, skillEfficiencies, originalPersonSkills.OriginalSkills.Where(s => s.SkillPercentage.Value>0).Select(s => s.Skill).ToArray());
		}

		protected virtual OriginalPersonSkills PersonSkills(IPersonPeriod personPeriod)
		{
			var personSkills = _personalSkills.PersonSkills(personPeriod);
			return new OriginalPersonSkills(personSkills,personSkills);
		}
	}

	public class OriginalPersonSkills
	{
		public OriginalPersonSkills(IEnumerable<IPersonSkill> filteredSkills, IEnumerable<IPersonSkill> originalSkills)
		{
			FilteredSkills = filteredSkills;
			OriginalSkills = originalSkills;
		}

		public IEnumerable<IPersonSkill> FilteredSkills { get; }
		public IEnumerable<IPersonSkill> OriginalSkills { get; }
	}
}