using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ActivityAssignedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public Guid ActivityId { get; set; }
	}
}