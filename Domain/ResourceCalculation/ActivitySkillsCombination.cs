using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ActivitySkillsCombination
	{
		private readonly IPayload _activity;
		private readonly SkillCombination _skills;

		public ActivitySkillsCombination(IPayload activity, SkillCombination skills)
		{
			_activity = activity;
			_skills = skills;
		}

		public string GenerateKey()
		{
			return _activity == null
				       ? string.Empty
				       : _activity.Id.GetValueOrDefault() + "|" + _skills.Key;
		}
	}
}