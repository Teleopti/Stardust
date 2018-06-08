using System;
using System.Threading;
using log4net;

namespace Teleopti.Wfm.Administration.Core
{
	public class RecurrentEventTimer
	{
		private readonly IPurgeOldSignInAttempts _purgeOldSignInAttempts;
		private readonly IPurgeNoneEmployeeData _purgeNoneEmployeeData;
		private Timer _timer;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RecurrentEventTimer));

		public RecurrentEventTimer(IPurgeOldSignInAttempts purgeOldSignInAttempts, IPurgeNoneEmployeeData purgeNoneEmployeeData)
		{
			_purgeOldSignInAttempts = purgeOldSignInAttempts;
			_purgeNoneEmployeeData = purgeNoneEmployeeData;
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
				_purgeOldSignInAttempts.Purge();
			}
			catch (Exception exception)
			{
				Logger.Error("Purging of login attempts failed", exception);
			}

			try
			{
				_purgeNoneEmployeeData.Purge();
			}
			catch (Exception exception)
			{
				Logger.Error("Purging of person info failed", exception);
			}
		}
	}
}