using System;
using System.Threading;
using log4net;

namespace Teleopti.Wfm.Administration.Core
{
	public class RecurrentEventTimer
	{
		private readonly IPurgeOldSignInAttempts _purgeOldSignInAttempts;
		private Timer _timer;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RecurrentEventTimer));

		public RecurrentEventTimer(IPurgeOldSignInAttempts purgeOldSignInAttempts)
		{
			_purgeOldSignInAttempts = purgeOldSignInAttempts;
		}

		public void Init(TimeSpan tickInterval)
		{
			if (_timer != null)
				throw new InvalidOperationException("Duplicate initialization of timer");

			_timer = new Timer(timerCallback, null, 0, (int)tickInterval.TotalMilliseconds);
		}

		private void timerCallback(object state)
		{
			try
			{
				Logger.Error("Purging (this is only information!)");
				_purgeOldSignInAttempts.Purge();
			}
			catch (Exception exception)
			{
				Logger.Error("Purging of login attempts failed", exception);
			}
		}
	}
}