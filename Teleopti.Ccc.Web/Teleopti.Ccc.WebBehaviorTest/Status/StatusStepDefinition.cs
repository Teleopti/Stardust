using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;

namespace Teleopti.Ccc.WebBehaviorTest.Status
{
	[Binding]
	public class StatusStepDefinition
	{
		[When(@"I view status page")]
		public void WhenIViewStatusPage()
		{
			Navigation.GoToPage("status");
		}

		[Then(@"Status page should say ok")]
		public void ThemStatusPageShouldSayOk()
		{
			Browser.Interactions.AssertExists(".stepsAreLoadedMarker");
		}
	}
}