using System;

namespace Teleopti.Messaging.SignalR
{
	public class SiteAdherenceMessage
	{
		public Guid SiteId { get; set; }

		public double OutOfAdherence { get; set; }
	}
}
