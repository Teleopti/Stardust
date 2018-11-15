using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationCommand : ICommandIdentifier
	{
		public DayOffOptimizationCommand()
		{
			CommandId = Guid.NewGuid();
		}
		
		public DateOnlyPeriod Period { get; set; }
		public IEnumerable<IPerson> AgentsToOptimize { get; set; }
		public Guid CommandId { get; set; }
	}
}