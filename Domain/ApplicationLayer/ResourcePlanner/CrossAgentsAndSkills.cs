using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class CrossAgentsAndSkills
	{
		public CrossAgentAndSkillResult Execute(IEnumerable<Island> allIslands, IEnumerable<IPerson> selectedAgents)
		{
			var retAgents = new List<Guid>();
			var retSkills = new List<Guid>();
			foreach (var selectedAgent in selectedAgents)
			{
				foreach (var island in allIslands)
				{
					var islandAgents = island.AgentsInIsland();
					if (islandAgents.Contains(selectedAgent))
					{
						retAgents.AddRange(islandAgents.Select(x => x.Id.Value));
						retSkills.AddRange(island.SkillIds());
						break;
					}
				}
			}
			return new CrossAgentAndSkillResult(retAgents, retSkills);
		}
	}
}