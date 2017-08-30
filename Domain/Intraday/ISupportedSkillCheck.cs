using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface ISupportedSkillCheck
	{
		bool IsSupported(ISkill skill);
	}
	
	public class InboundPhoneSkillSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture);
		}
	}

	public class EmailSkillSupported : ISupportedSkillCheck
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

	public class OtherSkillsLikeEmailSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals("SkillTypeBackoffice", StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals("SkillTypeProject", StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals("SkillTypeFax", StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals("SkillTypeTime", StringComparison.InvariantCulture);

		}
	}
}