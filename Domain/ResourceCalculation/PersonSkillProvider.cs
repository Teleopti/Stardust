using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillProvider : IPersonSkillProvider
	{
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination = new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();

		public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			ConcurrentBag<SkillCombination> foundCombinations = _personCombination.GetOrAdd(person, _ => new ConcurrentBag<SkillCombination>());
			foreach (var foundCombination in foundCombinations)
			{
				if (foundCombination.IsValidForDate(date))
				{
					return foundCombination;
				}
			}

			IPersonPeriod personPeriod = person.Period(date);
			if (personPeriod == null) return new SkillCombination(new ISkill[0], new DateOnlyPeriod(), new SkillEffiencyResource[]{});

			var personSkillCollection =
				personPeriod.PersonSkillCollection.Where(personSkill => !((IDeleteTag) personSkill.Skill).IsDeleted).ToArray();

			var skills = personSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0)
				.Concat(personPeriod.PersonMaxSeatSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Concat(personPeriod.PersonNonBlendSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Select(s => s.Skill)
				.Distinct()
				.ToArray();

			var skillEfficiencies =
				personSkillCollection.Where(
					s => s.Active && s.SkillPercentage.Value > 0)
					.Select(k => new SkillEffiencyResource(k.Skill.Id.GetValueOrDefault(), k.SkillPercentage.Value)).ToArray();

			var combination = new SkillCombination(skills, personPeriod.Period, skillEfficiencies);
			foundCombinations.Add(combination);
			
			return combination;
		}
	}

	public class SkillCombination
	{
		private readonly DateOnlyPeriod _period;
		private readonly IList<Guid> _skillKeys;

		public SkillCombination(ISkill[] skills, DateOnlyPeriod period, SkillEffiencyResource[] skillEfficiencies)
		{
			_period = period;
			SkillEfficiencies = skillEfficiencies;
			Skills = skills;
			_skillKeys = Skills.Select(s => s.Id.GetValueOrDefault()).ToList();
			Key = toKey(_skillKeys);
		}

		public bool IsValidForDate(DateOnly date)
		{
			return _period.Contains(date);
		}

		private static string toKey(IEnumerable<Guid> idCollection)
		{
			return string.Join("_", idCollection.OrderBy(s => s));
		}

		public bool HasSkill(Guid skill)
		{
			return _skillKeys.Contains(skill);
		}

		public string Key { get; private set; }
		public ISkill[] Skills { get; private set; }
		public SkillEffiencyResource[] SkillEfficiencies { get; private set; }
	}
}