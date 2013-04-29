using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Find = WatiN.Core.Find;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class QUnitTestsStepDefinition
	{

		[When(@"I navigate to unit test url (.*)")]
		public void WhenINavigateTo(string url)
		{
			Navigation.GoToWaitForCompleted(url, new ForceRefresh());
			Browser.Interactions.AssertUrlContains(".html");
			Browser.Interactions.AssertExists("#qunit-tests");
		}

		[Then(@"I should see the tests run")]
		public void ThenIShouldSeeTheTestsRun()
		{
			Browser.Interactions.AssertExists("#qunit-tests");
			Browser.Interactions.AssertNotExists("#qunit-tests", ".running");
			Browser.Interactions.AssertExists("#qunit-testresult");

			EventualAssert.That(() =>
			                    	{
			                    		int total;
			                    		var text = Browser.Current.Element(Find.BySelector("#qunit-testresult .total")).Text;
										return int.TryParse(text, out total) ? total : 0;
			                    	}, Is.GreaterThan(0));
		}

		[Then(@"I should see all tests pass")]
		public void ThenIShouldSeeAllTestsPass()
		{
			Browser.Interactions.AssertExists("#qunit-testresult");

			EventualAssert.That(() =>
			                    	{
			                    		int failed;
										var text = Browser.Current.Element(Find.BySelector("#qunit-testresult .failed")).Text;
			                    		return int.TryParse(text, out failed) ? failed : -1;
			                    	}, Is.EqualTo(0));
		}

	}

}
