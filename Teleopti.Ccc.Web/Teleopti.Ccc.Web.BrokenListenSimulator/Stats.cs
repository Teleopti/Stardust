using System;
using System.Diagnostics;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Web.BrokenListenSimulator
{
	public class Stats
	{
		public int NumberOfCallbacks;
		private readonly Action _action;
		public Stopwatch Stopwatch = new Stopwatch();
		private readonly object thisLock = new object();

		public Stats(Action action)
		{
			_action = action;
		}

		public void Callback(object sender, EventMessageArgs e)
		{
			lock (thisLock)
			{
				NumberOfCallbacks++;
			}
			_action();
		}
	}
}