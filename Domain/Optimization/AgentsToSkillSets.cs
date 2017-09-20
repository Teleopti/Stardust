using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class AgentsToSkillSets
	{
		private readonly SkillSetProvider _skillSetProvider;

		public AgentsToSkillSets(SkillSetProvider skillSetProvider)
		{
			_skillSetProvider = skillSetProvider;
		}

		public IEnumerable<IEnumerable<IPerson>> ToSkillGroups()
		{
			return _skillSetProvider.Fetch().AgentsGroupedBySkillSet();
		}
	}
}