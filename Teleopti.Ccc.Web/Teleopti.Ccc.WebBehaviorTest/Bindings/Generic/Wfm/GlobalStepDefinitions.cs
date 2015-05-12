using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	}
}
