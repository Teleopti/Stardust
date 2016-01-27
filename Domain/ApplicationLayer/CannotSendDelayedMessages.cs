using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class CannotSendDelayedMessages : IDelayedMessageSender
	{
		public void DelaySend(DateTime time, object message)
		{
			throw new NotImplementedException();
		}
	}
}