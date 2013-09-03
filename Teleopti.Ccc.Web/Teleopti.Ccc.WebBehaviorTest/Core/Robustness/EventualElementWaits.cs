using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Core.Robustness
{
	public static class EventualElementWaits
	{
		public static void WaitUntilHidden<T>(this T element) where T : Element
		{
			element.WaitUntil<T>(e => e.DisplayHidden());
		}

		public static void WaitUntilDisplayed<T>(this T element) where T : Element
		{
			element.WaitUntil<T>(e => e.DisplayVisible());
		}

		public static void WaitUntilEnabled<T>(this T element) where T : Element
		{
			element.WaitUntil<T>(e => e.Enabled);
		}
	}
}