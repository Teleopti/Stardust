using System.Collections.Generic;

namespace Teleopti.Interfaces.Messages
{
	public class Queues
	{
		// suggestion:
		// default queue = low priority
		// create seperate queues (maybe even with dedicates workers) for features that requires it
		public const string Default = "default";
		public const string ScheduleChangesToday = "schedule_changes_today";

		public static IEnumerable<string> OrderOfPriority()
		{
			// the order is the queue priority
			yield return ScheduleChangesToday;
			yield return Default;
		}
	}
}