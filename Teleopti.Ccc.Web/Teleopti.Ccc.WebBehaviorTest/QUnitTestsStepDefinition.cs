using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Find = WatiN.Core.Find;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class QUnitTestsStepDefinition
	{

		[When(@"I navigate to url (.*)")]
		public void WhenINavigateTo(string url)
		{
			Navigation.GoTo(url);
		}

		[Then(@"I should see the tests run")]
		public void ThenIShouldSeeTheTestsRun()
		{
			EventualAssert.That(() => int.Parse(Browser.Current.Para("qunit-testresult").Span(Find.ByClass("total", false)).InnerHtml), Is.GreaterThan(0));
		}

		[Then(@"I should see all tests pass")]
		public void ThenIShouldSeeAllTestsPass()
		{
			EventualAssert.That(() => int.Parse(Browser.Current.Para("qunit-testresult").Span(Find.ByClass("failed", false)).InnerHtml), Is.EqualTo(0));
		}

	}

}
