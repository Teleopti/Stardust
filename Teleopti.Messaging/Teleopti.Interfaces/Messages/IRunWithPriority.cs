using System.Collections.Generic;

namespace Teleopti.Interfaces.Messages
{
	public class QueueName
	{
		// suggestion:
		// default queue = low priority
		// create seperate queues (maybe even with dedicates workers) for features that requires it
		public const string Default = "default";

		public static IEnumerable<string> All()
		{
			yield return Default;
		}
	}
}