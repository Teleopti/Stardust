using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	//Use c#7 tuple when upgrading to net47 instead.
	public class CrossAgentAndSkillResult
	{
		public CrossAgentAndSkillResult(IEnumerable<Guid> agents, IEnumerable<Guid> skills)
		{
			Agents = agents;
			Skills = skills;
		}
		
		public IEnumerable<Guid> Agents { get; }
		public IEnumerable<Guid> Skills { get; }
	}
}