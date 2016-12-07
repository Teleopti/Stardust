using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public DateTime ExpiresAt { get; set; }
	}
}