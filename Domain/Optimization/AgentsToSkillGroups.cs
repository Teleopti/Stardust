using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentsToSkillGroups
	{
		private readonly SkillGroupInfoProvider _skillGroupInfoProvider;

		public AgentsToSkillGroups(SkillGroupInfoProvider skillGroupInfoProvider)
		{
			_skillGroupInfoProvider = skillGroupInfoProvider;
		}

		public IEnumerable<IEnumerable<IPerson>> ToSkillGroups()
		{
			return _skillGroupInfoProvider.Fetch().AgentsGroupedBySkillGroup();
		}
	}
}