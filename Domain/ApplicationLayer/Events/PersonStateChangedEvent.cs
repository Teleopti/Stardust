using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonStateChangedEvent : IEvent, ILogOnInfo
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
		public bool InOrNeutralAdherenceWithPreviousActivity { get; set; }
		public EventAdherence Adherence { get; set; }
	}

	public enum EventAdherence
	{
		In,
		Out,
		Neutral
	}
}