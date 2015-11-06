using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	class CiscoFinesseStepDefinition
	{
		[When(@"I accesse teleopti page through Cisco Finesse portal")]
		public void WhenIAccesseTeleoptiPageThroughCiscoFinessePortal()
		{
			TestControllerMethods.Logon();
			Navigation.GotoPageCiscoFinesse();
		}

		[Then(@"I should see teleopti logo")]
		public void ThenIShouldSeeTeleoptiLogo()
		{
			Browser.Interactions.AssertExists("#CiscoFinesse-teleopti-logo");
		}

		[Then(@"I should see Asm module")]
		public void ThenIShouldSeeAsmModule()
		{
			Browser.Interactions.AssertExists("#CiscoFinesse-asm-module");
		}

		[Then(@"I should see MyReport module")]
		public void ThenIShouldSeeMyReportModule()
		{
			Browser.Interactions.AssertExists("#CiscoFinesse-myReport-module");
		}
	}
}
