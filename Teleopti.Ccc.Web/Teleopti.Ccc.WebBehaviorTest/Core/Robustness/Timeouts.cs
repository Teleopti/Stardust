using System;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Robustness
{
	public static class Timeouts
	{
		public static TimeSpan Timeout { get; private set; }
		public static TimeSpan Poll { get; private set; }

		static Timeouts() { Set(TimeSpan.FromSeconds(5)); }

		public static void Set(TimeSpan timeout)
		{
			Timeout = timeout;
			Poll = TimeSpan.FromMilliseconds(25);
			Settings.WaitForCompleteTimeOut = Convert.ToInt32(timeout.TotalSeconds);
			Settings.WaitUntilExistsTimeOut = Convert.ToInt32(timeout.TotalSeconds);
		}
	}
}