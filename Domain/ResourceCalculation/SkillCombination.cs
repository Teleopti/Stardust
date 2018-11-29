using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombination
	{
		private DateOnlyPeriod _period;
		private DoubleGuidCombinationKey _mergedKey;
		private readonly ConcurrentDictionary<Guid,SkillCombination> _activityCombinations = new ConcurrentDictionary<Guid, SkillCombination>();

		public SkillCombination(ISkill[] skills, DateOnlyPeriod period, SkillEffiencyResource[] skillEfficiencies, ISkill[] originalSkills)
		{
			_period = period;
			SkillEfficiencies = skillEfficiencies;
			Skills = skills;
			OriginalSkills = originalSkills;
			Key = toKey(Skills.Select(s => s.Id.GetValueOrDefault()));
			OriginalKey = toKey(originalSkills.Select(s => s.Id.GetValueOrDefault()));
			_mergedKey = new DoubleGuidCombinationKey(Key, OriginalKey);
		}

		public Guid[] OriginalKey { get; }

		public bool IsValidForDate(DateOnly date)
		{
			return _period.Contains(date);
		}

		private static Guid[] toKey(IEnumerable<Guid> idCollection)
		{
			return idCollection.OrderBy(s => s).ToArray();
		}

		public bool HasSkill(Guid skill)
		{
			return Array.IndexOf(Key, skill) > -1;
		}

		public DoubleGuidCombinationKey MergedKey()
		{
			return _mergedKey;
		}

		public SkillCombination ForActivity(Guid activityId)
		{
			return _activityCombinations.GetOrAdd(activityId, id => new SkillCombination(
				Skills.Where(
					x =>
						x.SkillType?.ForecastSource == ForecastSource.MaxSeatSkill ||
						x.Activity?.Id.GetValueOrDefault() == id).ToArray(), _period,
				SkillEfficiencies,OriginalSkills.Where(
					x =>
						x.SkillType?.ForecastSource == ForecastSource.MaxSeatSkill ||
						x.Activity?.Id.GetValueOrDefault() == id).ToArray()));
		}

		public Guid[] Key { get; }
		public ISkill[] Skills { get; }
		public ISkill[] OriginalSkills { get; }
		public SkillEffiencyResource[] SkillEfficiencies { get; }

		public override int GetHashCode()
		{
			return _mergedKey.GetHashCode();
		}
	}
}