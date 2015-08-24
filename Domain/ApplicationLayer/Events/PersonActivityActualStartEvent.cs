using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonActivityActualStartEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateOnly? BelongsToDate { get; set; }
	}
}