using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class Mailbox
	{
		public Guid Id { get; set; }
		public string Route { get; set; }
		public IEnumerable<Message> Messages { get { return _messages; }}
		public DateTime ExpiresAt { get; set; }
		
		protected List<Message> _messages;

		public Mailbox()
		{
			_messages = new List<Message>();
		}

		public void AddMessage(Message message)
		{
			_messages.Add(message);
		}

		public IEnumerable<Message> PopAllMessages()
		{
			var r = Messages;
			_messages = new List<Message>();
			return r;
		}

	}
}