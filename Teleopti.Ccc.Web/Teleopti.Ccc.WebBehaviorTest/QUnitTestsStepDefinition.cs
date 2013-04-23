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
			Navigation.GoTo(url, new ForceRefresh(), new WaitUntilAt(".html"));
		}

		[Then(@"I should see the tests run")]
		public void ThenIShouldSeeTheTestsRun()
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector("#qunit-tests")).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".running")).Exists, Is.False);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector("#qunit-testresult")).Exists, Is.True);
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
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector("#qunit-testresult")).Exists, Is.True);
			EventualAssert.That(() =>
			                    	{
			                    		int failed;
										var text = Browser.Current.Element(Find.BySelector("#qunit-testresult .failed")).Text;
			                    		return int.TryParse(text, out failed) ? failed : -1;
			                    	}, Is.EqualTo(0));
		}

	}

}
