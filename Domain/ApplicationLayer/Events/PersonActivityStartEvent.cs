using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonActivityStartEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime StartTime { get; set; }
		public string Name { get; set; }
		public EventAdherence Adherence { get; set; }
	}
}