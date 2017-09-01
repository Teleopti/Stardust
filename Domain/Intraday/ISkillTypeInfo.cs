using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface ISkillTypeInfo
	{
		bool SupportsAbandonRate(ISkill skill);
		bool SupportsReforecastedAgents(ISkill skill);
	}

	public class SkillTypeInfoDefault : ISkillTypeInfo
	{
		private readonly string[] _skillTypesSupportAbandonRate = {
			SkillTypeIdentifier.Phone,
			SkillTypeIdentifier.Chat,
			SkillTypeIdentifier.Retail
		};

		private readonly string[] _skillTypesSupportReforecastedAgents = {
			SkillTypeIdentifier.Phone,
			SkillTypeIdentifier.Chat,
			SkillTypeIdentifier.Retail
		};

		public bool SupportsAbandonRate(ISkill skill)
		{
			return _skillTypesSupportAbandonRate.Any(skill.IsSkillType);
		}

		public bool SupportsReforecastedAgents(ISkill skill)
		{
			return _skillTypesSupportReforecastedAgents.Any(skill.IsSkillType);
		}
	}
}