using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface ISingleSkillLoadedDecider
	{
		bool IsSingleSkill(IList<ISkill> loadedSkills);
	}

	public class SingleSkillLoadedDecider : ISingleSkillLoadedDecider
	{
		public bool IsSingleSkill(IList<ISkill> loadedSkills)
		{
			if (loadedSkills.Count != 1)
				return false;

			if (loadedSkills[0].SkillType.GetType() != typeof(SkillTypePhone))
				return false;

			return true;
		}
	}
}