using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonStateChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }
		public string StateCode { get; set; }
		public string StateGroupName { get; set; }
		public Guid? StateGroupId { get; set; }
		public EventAdherence AdherenceWithPreviousActivity { get; set; }
		public EventAdherence Adherence { get; set; }
	}
}