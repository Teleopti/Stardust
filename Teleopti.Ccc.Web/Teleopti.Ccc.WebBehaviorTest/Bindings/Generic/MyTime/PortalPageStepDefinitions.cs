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
			Browser.Interactions.AssertFirstContains("#licensed-to-label", UserTexts.Resources.LicensedToColon);
			Browser.Interactions.AssertFirstContains("#licensed-to-text", "Teleopti_RD");
		}
	}
}