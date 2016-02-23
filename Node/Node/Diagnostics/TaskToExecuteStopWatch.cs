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

		public double GetTotalElapsedTimeInMilliseconds()
		{
			Stop();

			return Elapsed.TotalMilliseconds;
		}
	}
}