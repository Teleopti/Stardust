using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public static class SkillTypeIdentifier
	{
		public const string Phone = "SkillTypeInboundTelephony";
		public const string Email = "SkillTypeEmail";
		public const string Chat = "SkillTypeChat";
		public const string Retail = "SkillTypeRetail";
		public const string BackOffice = "SkillTypeBackoffice";
		public const string Project = "SkillTypeProject";
		public const string Fax = "SkillTypeFax";
		public const string Time = "SkillTypeTime";
		public const string Outbound = "SkillTypeOutbound";

		public static bool IsSkillType(this ISkill skill, string skillTypeIdentifier)
		{
			return skill.SkillType.Description.Name.Equals(skillTypeIdentifier, StringComparison.InvariantCulture);
		}
	}
}