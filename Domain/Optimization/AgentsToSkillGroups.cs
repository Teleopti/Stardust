using System.Collections.Generic;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentsToSkillGroups
	{
		private readonly ISkillGroupInfoProvider _skillGroupInfoProvider;

		public AgentsToSkillGroups(ISkillGroupInfoProvider skillGroupInfoProvider)
		{
			_skillGroupInfoProvider = skillGroupInfoProvider;
		}

		public IEnumerable<IEnumerable<IPerson>> ToSkillGroups()
		{
			return _skillGroupInfoProvider.Fetch().AgentsGroupedBySkillGroup();
		}
	}
}