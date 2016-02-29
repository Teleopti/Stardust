using System.Diagnostics;

namespace Stardust.Manager.Diagnostics
{
	public class ManagerStopWatch : Stopwatch
	{
		public ManagerStopWatch(bool startDirectly = true)
		{
			if (startDirectly)
			{
				Start();
			}
		}

		public double GetTotalElapsedTimeInDays()
		{
			StopIfRunning();

			return Elapsed.TotalDays;
		}

		private void StopIfRunning()
		{
			if (IsRunning)
			{
				Stop();
			}
		}

		public double GetTotalElapsedTimeInHours()
		{
			StopIfRunning();

			return Elapsed.TotalHours;
		}

		public double GetTotalElapsedTimeInMinutes()
		{
			StopIfRunning();

			return Elapsed.TotalMinutes;
		}

		public double GetTotalElapsedTimeInSeconds()
		{
			StopIfRunning();

			return Elapsed.TotalSeconds;
		}

		public double GetTotalElapsedTimeInMilliseconds()
		{
			StopIfRunning();

			return Elapsed.TotalMilliseconds;
		}
	}
}