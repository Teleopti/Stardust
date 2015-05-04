using System;
using System.Globalization;

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

		public string GenerateKey()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}|{1}", _activity, _skills.Key);
		}
	}
}