using System;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Robustness
{
	public static class EventualElementActions
	{
		private static int Timeout { get { return Convert.ToInt32(Timeouts.Timeout.TotalSeconds); } }

		public static void EventualClick<T>(this T element) where T : Element
		{
			element.WaitUntilExists(Timeout);
			element.WaitUntilEnabled();
			element.ClickNoWait();
		}

		public static void EventualWait<T>(this T element) where T : Element
		{
			element.WaitUntilExists(Timeout);
			element.WaitUntilEnabled();
		}

		public static T EventualGet<T>(this T element) where T : Element
		{
			element.WaitUntilExists(Timeout);
			return element;
		}
	}
}