using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PersonSkillProvider : IPersonSkillProvider
	{
		private readonly IDictionary<IPerson,ICollection<SkillCombination>> _personCombination = new Dictionary<IPerson, ICollection<SkillCombination>>();

		public SkillCombination SkillsOnPersonDate(IPerson person, DateOnly date)
		{
			ICollection<SkillCombination> foundCombinations;
			if (_personCombination.TryGetValue(person, out foundCombinations))
			{
				foreach (var foundCombination in foundCombinations)
				{
					if (foundCombination.IsValidForDate(date))
					{
						return foundCombination;
					}
				}
			}

			IPersonPeriod personPeriod = person.Period(date);
			if (personPeriod == null) return new SkillCombination(string.Empty, new ISkill[0], new DateOnlyPeriod());

			var skills = personPeriod.PersonSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0)
				.Concat(personPeriod.PersonMaxSeatSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Concat(personPeriod.PersonNonBlendSkillCollection.Where(s => s.Active && s.SkillPercentage.Value > 0))
				.Select(s => s.Skill)
				.Distinct()
				.ToList();

			var key = string.Join("_", skills.Where(s => !((IDeleteTag)s).IsDeleted).OrderBy(s => s.Name).Select(s => s.Id.GetValueOrDefault()));

			var combination = new SkillCombination(key, skills.ToArray(), personPeriod.Period);
			if (foundCombinations != null)
			{
				foundCombinations.Add(combination);
			}
			else
			{
				_personCombination.Add(person,new Collection<SkillCombination>{combination});
			}

			return combination;
		}
	}

	public class SkillCombination
	{
		private readonly DateOnlyPeriod _period;

		public SkillCombination(string key, ISkill[] skills, DateOnlyPeriod period)
		{
			_period = period;
			Key = key;
			Skills = skills;
		}

		public bool IsValidForDate(DateOnly date)
		{
			return _period.Contains(date);
		}

		public string Key { get; private set; }
		public ISkill[] Skills { get; private set; }
	}
}