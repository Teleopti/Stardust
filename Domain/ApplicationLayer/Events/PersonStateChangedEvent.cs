using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonStateChangedEvent : IEvent, ILogOnInfo, IGoToHangfire
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
		public bool InAdherence { get; set; }
		public bool InAdherenceWithPreviousActivity { get; set; }
		public DateTime ScheduleDate { get; set; }
	}
}