using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace NodeTest.Fakes.Timers
{
	public class PingToManagerFake : Timer
	{
		public ManualResetEventSlim Wait = new ManualResetEventSlim();

		public PingToManagerFake()
		{
			Elapsed += OnTimedEvent;

			AutoReset = false;
			Enabled = true;
		}

		private void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			Wait.Set();
		}
	}
}