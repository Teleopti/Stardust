using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillTypeInfoProvider : ISkillTypeInfoProvider
	{
		private readonly List<ISkillTypeInfo> _skillTypeInfo;

		public SkillTypeInfoProvider(IEnumerable<ISkillTypeInfo> skillTypeInfo)
		{
			_skillTypeInfo = skillTypeInfo.ToList();
		}

		public SkillTypeInfo GetSkillTypeInfo(ISkill skill)
		{
			var supportsAbandonRate = _skillTypeInfo.Any(x => x.SupportsAbandonRate(skill));
			var supportsReforcastedAgents = _skillTypeInfo.Any(x => x.SupportsReforecastedAgents(skill));

			return new SkillTypeInfo(supportsAbandonRate, supportsReforcastedAgents);
		}
	}
}