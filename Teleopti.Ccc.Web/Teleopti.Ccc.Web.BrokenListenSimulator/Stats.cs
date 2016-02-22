using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Web.BrokenListenSimulator
{
	public class Stats
	{
		public int NumberOfCallbacks;
		public int NumberExpected;
		private readonly int tenPercent;
		private readonly Action _action;
		public Stopwatch Stopwatch = new Stopwatch();
		private readonly object thisLock = new object();

		public Stats(int expected, Action action)
		{
			NumberExpected = expected;
			_action = action;
			tenPercent = expected/10;
		}

		public void Callback(object sender, EventMessageArgs e)
		{
			lock (thisLock)
			{
				NumberOfCallbacks++;
				if (NumberOfCallbacks % tenPercent == 0)
					Console.Write("{0}%-", 100 * NumberOfCallbacks / NumberExpected);
				if (NumberOfCallbacks == NumberExpected)
					Stopwatch.Stop();
			}
			_action();
		}
	}
}