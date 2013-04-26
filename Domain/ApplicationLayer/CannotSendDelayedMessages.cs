using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class CannotSendDelayedMessages : ISendDelayedMessages
	{
		public void DelaySend(DateTime time, object message)
		{
			throw new NotImplementedException();
		}
	}
}