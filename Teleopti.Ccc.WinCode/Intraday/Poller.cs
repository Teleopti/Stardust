using System;
using System.Timers;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.WinCode.Intraday
{
	public class Poller : IDisposable
	{
		private readonly Timer _timer;

		public Poller()
		{
			_timer = new Timer(Convert.ToInt32(StateHolder.Instance.StateReader.ApplicationScopeData.AppSettings["RtaPollingInterval"]));
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
