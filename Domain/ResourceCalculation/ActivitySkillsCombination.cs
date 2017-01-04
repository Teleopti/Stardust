using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ActivitySkillsCombination
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
			var that = obj as ActivitySkillsCombination;
			if (that == null)
				return false;
			return _activity == that._activity &&
				_skills.MergedKey() == that._skills.MergedKey();
		}

		public override int GetHashCode()
		{
			return _activity.GetHashCode() ^ _skills.MergedKey().GetHashCode();
		}
	}
}