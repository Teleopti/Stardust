using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentsToSkillGroups
	{
		private readonly VirtualSkillGroupsResultProvider _virtualSkillGroupsResultProvider;

		public AgentsToSkillGroups(VirtualSkillGroupsResultProvider virtualSkillGroupsResultProvider)
		{
			_virtualSkillGroupsResultProvider = virtualSkillGroupsResultProvider;
		}

		public IEnumerable<IEnumerable<IPerson>> ToSkillGroups()
		{
			return _virtualSkillGroupsResultProvider.Fetch().GetSkillGroupTree();
		}
	}
}