using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SignInStepDefinitions
	{
		private static SignInPageBase Page
		{
			get { return Pages.Pages.Current as SignInPageBase; }
		}

		[BeforeScenario("signin")]
		public void BeforeScenarioSignInWindows()
		{
			Navigation.GotoGlobalSignInPage();
		}


		[When(@"I sign in by Windows credentials")]
		public void WhenISignInByWindowsAuthentication()
		{
			var page = Pages.Pages.Current as SignInPage;
			if (page.SingleBusinessUnit)
			{
				ScenarioContext.Current.Pending();
				return;
			}
			page.SignInWindows();
		}
	}
}