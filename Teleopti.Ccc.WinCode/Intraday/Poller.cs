using System;
using System.Timers;

namespace Teleopti.Ccc.WinCode.Intraday
{
	public interface IPoller : IDisposable
	{
		void Poll(Action action);
	}
	public class Poller : IPoller
	{
		private readonly Timer _timer;

		public Poller(int millisecondsInterval)
		{
			_timer = new Timer(millisecondsInterval);
		}

		public void Poll(Action action)
		{
			_timer.Elapsed += (s, e) =>
			{
				action();
				_timer.Enabled = true;
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
