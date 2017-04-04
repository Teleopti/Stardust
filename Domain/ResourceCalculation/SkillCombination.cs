using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombination
	{
		private readonly DateOnlyPeriod _period;
		private readonly string _mergedKey;
		private readonly IList<Guid> _skillKeys;
		private readonly ConcurrentDictionary<Guid,SkillCombination> _activityCombinations = new ConcurrentDictionary<Guid, SkillCombination>();

		public SkillCombination(ISkill[] skills, DateOnlyPeriod period, SkillEffiencyResource[] skillEfficiencies, ISkill[] originalSkills)
		{
			_period = period;
			SkillEfficiencies = skillEfficiencies;
			Skills = skills;
			OriginalSkills = originalSkills;
			_skillKeys = Skills.Select(s => s.Id.GetValueOrDefault()).ToArray();
			Key = toKey(_skillKeys);
			OriginalKey = toKey(originalSkills.Select(s => s.Id.GetValueOrDefault()));
			_mergedKey = Key + "+" + OriginalKey;
		}

		public string OriginalKey { get; }

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

		public string MergedKey()
		{
			return _mergedKey;
		}

		public SkillCombination ForActivity(Guid activityId)
		{
			return _activityCombinations.GetOrAdd(activityId, id => new SkillCombination(
				Skills.Where(
					x =>
						x.SkillType != null && x.SkillType.ForecastSource == ForecastSource.MaxSeatSkill ||
						x.Activity != null && x.Activity.Id.GetValueOrDefault() == id).ToArray(), _period,
				SkillEfficiencies,OriginalSkills.Where(
					x =>
						x.SkillType != null && x.SkillType.ForecastSource == ForecastSource.MaxSeatSkill ||
						x.Activity != null && x.Activity.Id.GetValueOrDefault() == id).ToArray()));
		}

		public string Key { get; }
		public ISkill[] Skills { get; }
		public ISkill[] OriginalSkills { get; }
		public SkillEffiencyResource[] SkillEfficiencies { get; }

		public override int GetHashCode()
		{
			return _mergedKey.GetHashCode();
		}
	}
}