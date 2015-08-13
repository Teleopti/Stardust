using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonActivityStartEvent : IEvent, ILogOnInfo
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime StartTime { get; set; }
		public string Name { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
		public bool InAdherence { get; set; }
		public AdherenceState? Adherence { get; set; }
	}
}