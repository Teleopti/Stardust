using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.MessageBroker
{
	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public ICollection<Message> Messages { get; set; }

		public Mailbox()
		{
			Messages = new Collection<Message>();
		}

		public void AddMessage(Message message)
		{
			Messages.Add(message);
		}

		public IEnumerable<Message> PopAllMessages()
		{
			var r = Messages;
			Messages = new Collection<Message>();
			return r;
		}
	}
}