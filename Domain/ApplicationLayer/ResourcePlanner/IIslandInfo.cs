using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public interface IIslandInfo
	{
		IEnumerable<Guid> AgentsInIsland { get; }
	}
}