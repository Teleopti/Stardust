using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public sealed class PreferencesPeriodFeedbackMobileStepDefinition
	{
		[Then(@"I should see a warning icon")]
		public void ThenIShouldSeeAWarningIcon()
		{
			Browser.Interactions.AssertExists("div.warning-indicator");
		}

		[When(@"I click the warning icon")]
		public void WhenIClickTheWarningIcon()
		{
			Browser.Interactions.Click("div.warning-indicator");
		}

		[Then(@"I should not see any feedback")]
		public void ThenIShouldNotSeeAnyFeedback()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Preference-period-feedback-view > div :first");
		}

	}
}
