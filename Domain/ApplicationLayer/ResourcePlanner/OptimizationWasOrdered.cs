using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class OptimizationWasOrdered : IEvent
	{
		public DateOnlyPeriod Period { get; set; }
	}
}