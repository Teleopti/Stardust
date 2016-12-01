using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public class Island : IIsland
	{
		private readonly IEnumerable<SkillGroup> _skillGroups;

		public Island(IEnumerable<SkillGroup> skillGroups)
		{
			_skillGroups = skillGroups;
		}

		public IEnumerable<IPerson> AgentsInIsland()
		{
			return _skillGroups.SelectMany(x => x.Agents);
		}
	}
}