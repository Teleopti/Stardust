using System.Configuration;
using System.Threading;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ThreadPriorityManager
	{
		public static void ResetThreadPriority(ThreadPriority basePrio)
		{
			Thread.CurrentThread.Priority = basePrio;
		}

		public static ThreadPriority SetThreadPriorityFromConfiguration()
		{
			var basePrio = Thread.CurrentThread.Priority;
			var prio = ConfigurationManager.AppSettings["ThreadPriority"];
			switch (prio)
			{
				case "1":
					Thread.CurrentThread.Priority = ThreadPriority.Lowest;
					break;
				case "2":
					Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
					break;
				default:
					Thread.CurrentThread.Priority = basePrio;
					break;
			}
			return basePrio;
		}
	}
}