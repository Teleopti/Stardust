using System;
using System.Timers;

namespace Teleopti.Ccc.WinCode.Intraday
{
	public interface IPoller : IDisposable
	{
		void Poll(int millisecondsInterval, Action action);
	}

	public class Poller : IPoller
	{
		private readonly Timer _timer;

		public Poller()
		{
			_timer = new Timer();
		}

		public void Poll(int millisecondsInterval, Action action)
		{
			_timer.Interval = millisecondsInterval;
			_timer.Elapsed += (s, e) =>
			{
				try
				{
					action();
				}
				finally
				{
					_timer.Enabled = true;
				}
			};
			_timer.AutoReset = false;
			_timer.Enabled = true;
			_timer.Start();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer.Dispose();
			}
		}
	}
}
