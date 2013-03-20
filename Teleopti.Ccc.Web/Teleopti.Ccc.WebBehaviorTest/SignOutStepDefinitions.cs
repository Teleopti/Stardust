using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SignOutStepDefinitions
	{
		[When(@"I sign out")]
		public void WhenISignOut()
		{
			var page = Browser.Current.Page<PortalPage>();
			page.SignOutLink.EventualClick();
		}

		[When(@"I press back in the web browser")]
		public void WhenIPressBackInTheWebBrowser()
		{
			Browser.Current.WaitForComplete();
			Browser.Current.Back();
		}

	}
}