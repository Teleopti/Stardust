using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class QUnitTestsStepDefinition
	{
		[When(@"I navigate to unit test url (.*)")]
		public void WhenINavigateTo(string url)
		{
			Navigation.GoToWaitForCompleted(url, new BustCache());
			Browser.Interactions.AssertExists("#qunit-tests");
		}

		[Then(@"I should see all tests pass")]
		public void ThenIShouldSeeAllTestsPass()
		{
			Browser.Interactions.AssertExists(".qunit-pass");
		}

	}

}
