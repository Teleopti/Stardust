using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class PortalPageStepDefinitions
	{
		[Then(@"I should see licensed to information")]
		public void ThenIShouldSeeLicensedToInformation()
		{
			Browser.Interactions.AssertContains("#licensed-to-label", UserTexts.Resources.LicensedToColon);
			Browser.Interactions.AssertContains("#licensed-to-text", "Teleopti RD NOT for production use!");
		}
	}
}