using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class PortalPageStepDefinitions
	{
		[When(@"I sign out")]
		public void WhenISignOut()
		{
			Browser.Interactions.Click(".user-name-link");
			Browser.Interactions.Click("#signout");
			Browser.Interactions.AssertUrlContains("SSO/OpenId/Provider");
		}

		[Then(@"I should see MyTime")]
		public void ThenIShouldSeeMyTime()
		{
			Browser.Interactions.AssertExists(".body-weekview-inner");
		}

		[Then(@"I should see licensed to information")]
		public void ThenIShouldSeeLicensedToInformation()
		{
			Browser.Interactions.AssertFirstContains("#licensed-to-label", UserTexts.Resources.LicensedToColon);
			Browser.Interactions.AssertExists("#licensed-to-text"); 
		}

		[Then(@"I should see an application page")]
		public void ThenIShouldSeeAnApplicationPage()
		{
			Browser.Interactions.AssertExists("#regional-settings, #signout-button");
		}

	}
}