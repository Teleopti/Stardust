using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillCombination : IEquatable<SkillCombination>
	{
		private readonly DateOnlyPeriod _period;
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
			return Key + "+" + OriginalKey;
		}

		public SkillCombination ForActivity(Guid activityId)
		{
			return _activityCombinations.GetOrAdd(activityId, id => new SkillCombination(
				Skills.Where(
					x =>
						(x.SkillType != null && x.SkillType.ForecastSource == ForecastSource.MaxSeatSkill) ||
						(x.Activity != null && x.Activity.Id.GetValueOrDefault() == id)).ToArray(), _period,
				SkillEfficiencies,OriginalSkills.Where(
					x =>
						(x.SkillType != null && x.SkillType.ForecastSource == ForecastSource.MaxSeatSkill) ||
						(x.Activity != null && x.Activity.Id.GetValueOrDefault() == id)).ToArray()));
		}

		public string Key { get; }
		public ISkill[] Skills { get; }
		public ISkill[] OriginalSkills { get; }
		public SkillEffiencyResource[] SkillEfficiencies { get; }

		public bool Equals(SkillCombination other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _period.Equals(other._period) && string.Equals(OriginalKey, other.OriginalKey) && string.Equals(Key, other.Key) && Equals(SkillEfficiencies, other.SkillEfficiencies);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((SkillCombination) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _period.GetHashCode();
				hashCode = (hashCode * 397) ^ (OriginalKey?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (Key?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (SkillEfficiencies?.GetHashCode() ?? 0);
				return hashCode;
			}
		}
	}
}