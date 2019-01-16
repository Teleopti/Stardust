using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class AdjustAdherencePageStepDefinitions
	{
		[When("I view adjust adherence")]
		public void WhenIViewAdjustAdherence()
		{
			TestControllerMethods.Logon();
			Navigation.GoToPage("wfm/#/rta/adjust-adherence");
		}

		[When(@"I adjust adherence '(.*)' to '(.*)' as neutral adherence")]
		public void AndIAdjustAdherenceBetweenForAllAgents(string from, string to)
		{
			var start = DateTime.Parse(from);
			var end = DateTime.Parse(to);
			Browser.Interactions.Click(".add-period");
			Browser.Interactions.FillWith(".start-date", start.ToString("d"));
			Browser.Interactions.FillWith(".start-time", start.ToString("t"));
			Browser.Interactions.FillWith(".end-date", end.ToString("d"));
			Browser.Interactions.FillWith(".end-time", end.ToString("t"));
			Browser.Interactions.Click(".adjust-adherence");
		}
	}
}