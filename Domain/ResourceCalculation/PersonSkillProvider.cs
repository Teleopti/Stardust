using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillProvider : IPersonSkillProvider
	{
		//TODO: Remove this -> lots of components could be singleinstance.
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination = new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();

		public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			var foundCombinations = _personCombination.GetOrAdd(person, _ => new ConcurrentBag<SkillCombination>());
			foreach (var foundCombination in foundCombinations.Where(foundCombination => foundCombination.IsValidForDate(date)))
			{
				return foundCombination;
			}

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

			var combination = new SkillCombination(skills, personPeriod.Period, skillEfficiencies);
			foundCombinations.Add(combination);
			
			return combination;
		}

		protected virtual IEnumerable<IPersonSkill> PersonSkills(IPersonPeriod personPeriod)
		{
			return new PersonalSkills().PersonSkills(personPeriod);
		}
	}
}