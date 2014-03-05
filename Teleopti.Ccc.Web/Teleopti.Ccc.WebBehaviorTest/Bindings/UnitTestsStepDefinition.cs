using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class UnitTestsStepDefinition
	{
		[When(@"I navigate to unit test url (.*)")]
		public void WhenINavigateTo(string url)
		{
			Navigation.GoToWaitForCompleted(url, new BustCache());
		}

		[Then(@"I should see all tests pass")]
		[Scope(Feature = "QUnit test")]
		public void ThenIShouldSeeAllTestsPassQunit()
		{
			Browser.Interactions.AssertExists("#qunit-tests");
			Browser.Interactions.AssertExists(".qunit-pass");
		}

		[Then(@"I should see all tests pass")]
		[Scope(Feature = "Buster test")]
		public void ThenIShouldSeeAllTestsPassBuster()
		{
			Browser.Interactions.AssertNotExists(".success", ".failure");
		}

	}

}
