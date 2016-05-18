using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonalActivityAddedEvent : EventWithInfrastructureContext, ICommandIdentifier
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public Guid ActivityId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid CommandId { get; set; }
	}
}
