using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class DayUnscheduledEvent : EventWithInfrastructureContext, ICommandIdentifier
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid CommandId { get; set; }
	}
}