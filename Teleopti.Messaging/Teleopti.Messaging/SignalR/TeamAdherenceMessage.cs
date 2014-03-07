using System;

namespace Teleopti.Messaging.SignalR
{
	public class TeamAdherenceMessage
	{
		public Guid TeamId { get; set; }

		public double OutOfAdherence { get; set; }
	}
}
