using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class TeamNameChangedEvent : IEvent
	{
		public Guid TeamId { get; set; }
		public string Name { get; set; }
	}
}