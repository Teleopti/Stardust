using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class SiteNameChangedEvent : IEvent
	{
		public Guid SiteId { get; set; }
		public string Name { get; set; }
	}
}