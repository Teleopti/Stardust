using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCommand
	{
		public IEnumerable<IPerson> AgentsToSchedule { get; set; }
		public DateOnlyPeriod Period { get; set; }
	}
}