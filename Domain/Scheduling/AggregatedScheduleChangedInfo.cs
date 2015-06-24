using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AggregatedScheduleChangedInfo
	{
		public Guid PersonId { get; set; }
		public DateTimePeriod Period { get; set; }
	}
}