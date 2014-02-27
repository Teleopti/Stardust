using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PersonAccountStepDefinition
	{
		[Then(@"I should see the remaining time is '(.*)'")]
		public void ThenIShouldSeeTheRemainingTimeIs(string remainingTime)
		{
			var selector = string.Format(".remainingTime :contains('{0}']", remainingTime);
			Browser.Interactions.AssertExists(selector);
		}

		[Then(@"I should see the used time is '(.*)'")]
		public void ThenIShouldSeeTheUsedTimeIs(string usedTime)
		{
			var selector = string.Format(".remainingTime :contains('{0}']", usedTime);
			Browser.Interactions.AssertExists(selector);
		}
	}
}
