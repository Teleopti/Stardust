using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public interface IIslandInfo
	{
		IEnumerable<Guid> AgentsInIsland { get; }
		IEnumerable<Guid> Agents { get; }
		Guid CommandId { get; }
		DateOnly StartDate { get; set; }
		DateOnly EndDate { get; set; }
		IEnumerable<Guid> Skills { get; set; }
	}
}