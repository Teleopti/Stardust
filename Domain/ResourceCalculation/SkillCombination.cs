using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombination
	{
		private readonly DateOnlyPeriod _period;
		private readonly IList<Guid> _skillKeys;
		private readonly ConcurrentDictionary<Guid,SkillCombination> _activityCombinations = new ConcurrentDictionary<Guid, SkillCombination>();

		public SkillCombination(ISkill[] skills, DateOnlyPeriod period, SkillEffiencyResource[] skillEfficiencies)
		{
			_period = period;
			SkillEfficiencies = skillEfficiencies;
			Skills = skills;
			_skillKeys = Skills.Select(s => s.Id.GetValueOrDefault()).ToArray();
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

		public SkillCombination ForActivity(Guid activityId)
		{
			return _activityCombinations.GetOrAdd(activityId, id => new SkillCombination(
				Skills.Where(
					x =>
						(x.SkillType != null && x.SkillType.ForecastSource == ForecastSource.MaxSeatSkill) ||
						(x.Activity != null && x.Activity.Id.GetValueOrDefault() == id)).ToArray(), _period,
				SkillEfficiencies));
		}

		public string Key { get; private set; }
		public ISkill[] Skills { get; }
		public SkillEffiencyResource[] SkillEfficiencies { get; }
	}
}