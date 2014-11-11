using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonInAdherenceEvent : IEvent, ILogOnInfo
	{
		public Guid PersonId { get; set; }
		public DateTime Timestamp { get; set; }
		public Guid InitiatorId { get; set; }
		public string Datasource { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}