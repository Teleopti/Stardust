using System.Diagnostics;

namespace Stardust.Manager.Diagnostics
{
	public class ManagerStopWatch : Stopwatch
	{
		public ManagerStopWatch()
		{
			Start();
		}

		private void StopIfRunning()
		{
			if (IsRunning)
			{
				Stop();
			}
		}

		public double GetTotalElapsedTimeInMilliseconds()
		{
			StopIfRunning();

			return Elapsed.TotalMilliseconds;
		}
	}
}