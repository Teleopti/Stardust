using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IMultisiteSkillSupportedCheck
	{
		bool Check(ISkill skill);
	}

	public class MultisiteSkillSupportedCheckAlwaysTrue : IMultisiteSkillSupportedCheck
	{
		public bool Check(ISkill skill)
		{
			return true;
		}
	}

	public class MultisiteSkillSupportedCheck : IMultisiteSkillSupportedCheck
	{
		public bool Check(ISkill skill)
		{
			var isMultisiteSkill = skill.GetType() == typeof(MultisiteSkill);
			return !isMultisiteSkill;
		}
	}
}