using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonEmploymentChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateOnly FromDate { get; set; }
	}
}
