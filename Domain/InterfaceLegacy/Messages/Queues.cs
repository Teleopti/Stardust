using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Messages
{
	public class Queues
	{
		// suggestion:
		// default queue = low priority
		// create seperate queues (maybe even with dedicates workers) for features that requires it
		public const string Default = "default";

		[Obsolete("Use CriticalScheduleChangesToday instead")]
		public const string ScheduleChangesToday = "schedule_changes_today";

		public const string CriticalScheduleChangesToday = "critical_schedule_changes_today";

		public static IEnumerable<string> OrderOfPriority()
		{
			// https://discuss.hangfire.io/t/multiple-queue-priority-order-is-not-working/784
			// it's in alphabetical order when using sqlserver backplane for hangfire
			yield return CriticalScheduleChangesToday;
#pragma warning disable 618
			yield return ScheduleChangesToday;
#pragma warning restore 618
			yield return Default;
		}
	}
}