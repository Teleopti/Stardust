using System.Diagnostics;

namespace Stardust.Node.Diagnostics
{
	internal class TaskToExecuteStopWatch : Stopwatch
	{
		public TaskToExecuteStopWatch(bool startDirectly = true)
		{
			if (startDirectly)
			{
				Start();
			}
		}

		private void StopIfRunning()
		{
			if (IsRunning)
			{
				Stop();
			}
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
	}
}