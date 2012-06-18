using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Find = WatiN.Core.Find;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class QUnitTestsStepDefinition
	{

		[When(@"I navigate to application url (.*)")]
		public void WhenINavigateTo(string url)
		{
			Navigation.GoTo(url);
		}

		[Then(@"I should see the tests run")]
		public void ThenIShouldSeeTheTestsRun()
		{
			var testResults = Browser.Current.Para("qunit-testresult");
			EventualAssert.That(() => testResults.Exists, Is.True);
			EventualAssert.That(() => int.Parse(testResults.Span(Find.ByClass("total", false)).InnerHtml), Is.GreaterThan(0));
		}

		[Then(@"I should see all tests pass")]
		public void ThenIShouldSeeAllTestsPass()
		{
			var testResults = Browser.Current.Para("qunit-testresult");
			EventualAssert.That(() => testResults.Exists, Is.True);
			EventualAssert.That(() => int.Parse(testResults.Span(Find.ByClass("failed", false)).InnerHtml), Is.EqualTo(0));
		}

	}

}
