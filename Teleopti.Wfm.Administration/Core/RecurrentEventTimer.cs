using System;
using System.Diagnostics;
using System.Threading;

namespace Teleopti.Wfm.Administration.Core
{
	public class RecurrentEventTimer : IRecurrentEventTimer
	{
		private Timer _timer;

		public void Init(TimeSpan tickInterval)
		{
			if (_timer != null)
				throw new InvalidOperationException("Duplicate initialization of timer");

			_timer = new Timer(timerCallback, null, 0, (int)tickInterval.TotalMilliseconds);
		}

		private static void timerCallback(object state)
		{
			// Create an EventLog instance and assign its source.
			var myLog = new EventLog {Source = "Teleopti Service Bus"};
			// Write an informational entry to the event log.    
			myLog.WriteEntry("1 hour tick test for RecurrentEventTimer",EventLogEntryType.Information);
		}
	}

	public class RecurrentEventTimerNoTick : IRecurrentEventTimer
	{
		public void Init(TimeSpan tickInterval)
		{
		}
	}
}