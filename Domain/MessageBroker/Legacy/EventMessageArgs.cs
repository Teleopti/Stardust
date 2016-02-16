using System;

namespace Teleopti.Ccc.Domain.MessageBroker.Legacy
{
	public class EventMessageArgs : EventArgs
	{
		private readonly IEventMessage _eventMessage;

		public EventMessageArgs(IEventMessage message)
		{
			_eventMessage = message;
		}

		public IEventMessage Message
		{
			get { return _eventMessage; }
		}

		public Message InternalMessage;
	}
}