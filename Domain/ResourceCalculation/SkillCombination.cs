using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
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