using System;
using log4net;

namespace Teleopti.Ccc.Domain.Infrastructure
{
	public class DeadLockRetrier
	{
		private const int totalRetries = 3;

		public void RetryOnDeadlock(Action action)
		{
			var attempt = 1;
			while (true)
			{
				try
				{
					action.Invoke();
					break;
				}
				catch (DeadLockVictimException e)
				{
					attempt++;
					if (attempt > totalRetries)
						throw;
					LogManager.GetLogger(typeof(DeadLockRetrier))
						.Warn($"Transaction deadlocked, running attempt {attempt} of {totalRetries}.", e);
				}
			}
		}
	}
}