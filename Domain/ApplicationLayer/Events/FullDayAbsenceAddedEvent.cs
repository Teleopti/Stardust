using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	[Serializable]
	public class FullDayAbsenceAddedEvent : RaptorDomainEvent
	{
		public Guid AbsenceId { get; set; }
		public Guid PersonId { get; set; }
		public Guid ScenarioId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}

	[Serializable]
	public class ActivityChangedEvent : RaptorDomainEvent
	{
		public Guid ActivityId { get; set; }
		public string Property { get; set; }
		public string OldValue { get; set; }
		public string NewValue { get; set; }
	}
}