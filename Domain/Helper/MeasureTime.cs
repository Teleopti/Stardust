using System;
using System.Diagnostics;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class MeasureTime
	{
		public static TimeSpan Do(Action action)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			action();
			stopwatch.Stop();
			return stopwatch.Elapsed;
		}
	}
}