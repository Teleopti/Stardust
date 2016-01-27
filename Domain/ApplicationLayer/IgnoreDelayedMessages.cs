using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class IgnoreDelayedMessages : IDelayedMessageSender
	{
		public void DelaySend(DateTime time, object message)
		{
		}
	}
}