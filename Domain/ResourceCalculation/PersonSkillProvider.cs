using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			ConcurrentBag<SkillCombination> foundCombinations;
			if (_personCombination.TryGetValue(person, out foundCombinations))
			{
				foreach (var foundCombination in foundCombinations.ToArray())
				{
					if (foundCombination.IsValidForDate(date))
					{
						return foundCombination;
					}
				}
			}

			IPersonPeriod personPeriod = person.Period(date);
			if (personPeriod == null) return new SkillCombination(string.Empty, new ISkill[0], new DateOnlyPeriod(), new Dictionary<Guid, double>());

			var skills = personPeriod.PersonSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0)
				.Concat(personPeriod.PersonMaxSeatSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Concat(personPeriod.PersonNonBlendSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Select(s => s.Skill)
				.Distinct()
				.ToList();

			var skillEfficiencies =
				personPeriod.PersonSkillCollection.Where(
					s => s.Active && s.SkillPercentage.Value > 0 && s.SkillPercentage.Value != 1d).ToDictionary(k => k.Skill.Id.GetValueOrDefault(),v => v.SkillPercentage.Value);

			var key = SkillCombination.ToKey(skills.Where(s => !((IDeleteTag)s).IsDeleted).Select(s => s.Id.GetValueOrDefault()));

			var combination = new SkillCombination(key, skills.ToArray(), personPeriod.Period, skillEfficiencies);
			if (foundCombinations != null)
			{
				foundCombinations.Add(combination);
			}
			else
			{
				_personCombination.TryAdd(person, new ConcurrentBag<SkillCombination> { combination });
			}

			return combination;
		}
	}

	public class SkillCombination
	{
		private readonly DateOnlyPeriod _period;

		public SkillCombination(string key, ISkill[] skills, DateOnlyPeriod period, IDictionary<Guid, double> skillEfficiencies)
		{
			_period = period;
			SkillEfficiencies = skillEfficiencies;
			Key = key;
			Skills = skills;
		}

		public bool IsValidForDate(DateOnly date)
		{
			return _period.Contains(date);
		}

		public static string ToKey(IEnumerable<Guid> idCollection)
		{
			return string.Join("_", idCollection.OrderBy(s => s));
		}

		public string Key { get; private set; }
		public ISkill[] Skills { get; private set; }
		public IDictionary<Guid, double> SkillEfficiencies { get; private set; }
	}
}