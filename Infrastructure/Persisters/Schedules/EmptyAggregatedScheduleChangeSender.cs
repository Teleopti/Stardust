using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class EmptyAggregatedScheduleChangeSender : IAggregatedScheduleChangeSender
	{
		public void Send(List<AggregatedScheduleChangedInfo> modified, IScenario scenario)
		{
		}
	}
}