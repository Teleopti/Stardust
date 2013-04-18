using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class IgnoreDelayedMessages : ISendDelayedMessages
	{
		public void DelaySend(DateTime time, object message)
		{
		}
	}
}