using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommand : ITrackInfo //should maybe not be here...
	{
		public IntradayOptimizationCommand()
		{
			TrackId = Guid.NewGuid();
		}
		public DateOnlyPeriod Period { get; set; }
		public bool RunResolveWeeklyRestRule { get; set; }
		public IEnumerable<IPerson> AgentsToOptimize { get; set; }
		public Guid TrackId { get; set; }
	}
}