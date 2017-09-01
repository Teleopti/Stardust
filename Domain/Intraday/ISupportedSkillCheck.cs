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
			return skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Phone, StringComparison.InvariantCulture);
		}
	}

	public class EmailSkillSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Email, StringComparison.InvariantCulture);
		}
	}

	public class OtherSkillsLikePhoneSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Chat, StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Retail, StringComparison.InvariantCulture);
		}
	}

	public class OtherSkillsLikeEmailSupported : ISupportedSkillCheck
	{
		public bool IsSupported(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.BackOffice, StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Project, StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Fax, StringComparison.InvariantCulture)
				   || skill.SkillType.Description.Name.Equals(SkillTypeIdentifier.Time, StringComparison.InvariantCulture);

		}
	}
}