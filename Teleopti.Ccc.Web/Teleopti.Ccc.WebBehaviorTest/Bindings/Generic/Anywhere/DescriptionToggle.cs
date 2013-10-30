using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	public static class DescriptionToggle
	{
		public static void EnsureIsOn()
		{
			Browser.Interactions.AssertExists(".toggle-descriptions:enabled");
			if ((string) Browser.Interactions.Javascript("return $('.toggle-descriptions.active').length") == "0")
				Browser.Interactions.Click(".toggle-descriptions:enabled");
		}
	}
}