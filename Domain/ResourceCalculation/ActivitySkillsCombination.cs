using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ActivitySkillsCombination : IEquatable<ActivitySkillsCombination>
	{
		private readonly Guid _activity;
		private readonly SkillCombination _skills;

		public ActivitySkillsCombination(Guid activity, SkillCombination skills)
		{
			_activity = activity;
			_skills = skills;
		}

		public bool HasActivity(Guid id)
		{
			return _activity == id;
		}

		//TODO: remove me later!
		public string SkillCombinationKey()
		{
			return _skills.MergedKey();
		}

		public bool ContainsSkill(Guid id)
		{
			return _skills.HasSkill(id);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((ActivitySkillsCombination) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_activity.GetHashCode() * 397) ^ (_skills?.GetHashCode() ?? 0);
			}
		}

		public bool Equals(ActivitySkillsCombination other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _activity.Equals(other._activity) && Equals(_skills, other._skills);
		}
	}
}