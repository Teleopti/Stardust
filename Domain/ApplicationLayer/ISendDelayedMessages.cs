using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ISendDelayedMessages
	{
		void DelaySend(DateTime time, object message);
	}
}