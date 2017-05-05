using System;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface ISupportedSkillCheck
	{
		bool IsSupported(ISkill skill);
	}

	public class MultisiteSkillSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return !(skill.GetType() == typeof(MultisiteSkill));
		}
	}
	public class InboundPhoneSkillSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture);
		}
	}

	public class SkillsLikeEmailSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals("SkillTypeEmail", StringComparison.InvariantCulture);
		}
	}

	public class OtherSkillsLikePhoneSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals("SkillTypeChat", StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals("SkillTypeRetail", StringComparison.InvariantCulture);
		}
	}
}