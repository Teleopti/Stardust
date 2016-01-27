using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IDelayedMessageSender
	{
		void DelaySend(DateTime time, object message);
	}
}