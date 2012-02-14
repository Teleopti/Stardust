using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class MobileSignInStepDefinitions
	{
		[BeforeScenario("mobilesignin")]
		public void BeforeScenarioSignInWindows()
		{
			Navigation.GotoMobileReportsSignInPage();
		}
	}
}