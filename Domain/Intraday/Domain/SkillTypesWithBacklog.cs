using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public static class SkillTypesWithBacklog
	{
		private static readonly HashSet<string> emailLikeSkillTypes = new HashSet<string>()
		{
			SkillTypeIdentifier.BackOffice,
			SkillTypeIdentifier.Project,
			SkillTypeIdentifier.Fax,
			SkillTypeIdentifier.Time,
			SkillTypeIdentifier.Email
		};

		public static bool IsBacklogSkillType(ISkill skill)
		{
			return emailLikeSkillTypes.Contains(skill.SkillType.Description.Name);
		}
	}
}