using System;
using log4net;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public enum DeadLockVictim
	{
		Yes,
		No
	}

	public class DeadLockRetrier
	{
		private readonly int _totalRetries = 3;

		public void RetryOnDeadlock(Action action)
		{
			// make retry and warn logging an aspect?
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
					if (attempt > _totalRetries)
						throw;
					LogManager.GetLogger(typeof(DeadLockRetrier))
						.Warn($"Transaction deadlocked, running attempt {attempt} of {_totalRetries}.", e);
				}
			}
		}
	}
}