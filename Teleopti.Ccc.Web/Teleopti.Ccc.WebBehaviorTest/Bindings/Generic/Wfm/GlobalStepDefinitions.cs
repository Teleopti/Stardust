using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	class GlobalStepDefinitions
	{
		[Given(@"I view Advanced forecasting option")]
		public void GivenIViewAdvancedForecastingOption()
		{
			Browser.Interactions.Click("button.next-step-advanced");
		}

		[When(@"I click on the breadcrumb forecasting link")]
		public void WhenIClickOnTheBreadcrumbForecastingLink()
		{
			Browser.Interactions.Click(".breadcrumb a:first-child");
		}

		[Then(@"I should see Forecasting")]
		public void ThenIShouldSeeForecasting()
		{
			Browser.Interactions.AssertUrlContains("forecasting");
		}

		//Help widget
		[Given(@"I view permissions")]
		public void GivenIViewPermissions()
		{
			Browser.Interactions.Click(".sidenav.left > section > div > a > div");
		}

		[When(@"I click on the help widget")]
		public void WhenIClickOnTheHelpWidget()
		{
			Browser.Interactions.Click(".help-widget > .wfm-accordion-head");
		}

		[Then(@"I should see relevant help")]
		public void ThenIShouldSeeRelevantHelp()
		{
			Browser.Interactions.AssertUrlContains("permissions");
			Browser.Interactions.AssertAnyContains(".help-widget", "permissions");
		}

		[Then(@"I should not see Real time adherence in the menu")]
		public void ThenIShouldNotSeeRealTimeAdherenceInTheMenu()
		{
			Browser.Interactions.AssertNotExists(".nav-item", "Real Time Adherence");
		}
	}
}
