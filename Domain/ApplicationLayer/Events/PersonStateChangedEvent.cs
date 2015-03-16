using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonStateChangedEvent : IEvent, ILogOnInfo, IGoToHangfire
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
		public bool InAdherence { get; set; }
		public bool InAdherenceWithPreviousActivity { get; set; }
		public AdherenceState? Adherence { get; set; }
	}
}