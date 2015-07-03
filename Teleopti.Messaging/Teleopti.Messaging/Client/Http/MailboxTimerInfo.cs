using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class MailboxTimerInfo
	{
		public EventHandler<EventMessageArgs> EventMessageHandler { get; set; }
		public object Timer { get; set; }
		public bool IsAlive { get; set; }
	}
}