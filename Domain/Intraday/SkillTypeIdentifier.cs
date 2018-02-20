using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Intraday
{
	public static class SkillTypeIdentifier
	{
		public const string Phone = nameof(Resources.SkillTypeInboundTelephony);
		public const string Email = nameof(Resources.SkillTypeEmail);
		public const string Chat = nameof(Resources.SkillTypeChat);
		public const string Retail = nameof(Resources.SkillTypeRetail);
		public const string BackOffice = nameof(Resources.SkillTypeBackoffice);
		public const string Project = nameof(Resources.SkillTypeProject);
		public const string Fax = nameof(Resources.SkillTypeFax);
		public const string Time = nameof(Resources.SkillTypeTime);
		public const string Outbound = "SkillTypeOutbound";

		public static bool IsSkillType(this ISkill skill, string skillTypeIdentifier)
		{
			return skill.SkillType.Description.Name.Equals(skillTypeIdentifier, StringComparison.InvariantCulture);
		}
	}
}