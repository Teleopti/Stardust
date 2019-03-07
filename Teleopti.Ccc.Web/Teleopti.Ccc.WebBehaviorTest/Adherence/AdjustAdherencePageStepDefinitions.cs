using System;
using System.Globalization;
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
			Navigation.GoToPage("wfm/#/rta/adjust-adherence?testMode=true");
		}

		[When(@"I adjust adherence '(.*)' to '(.*)' as neutral adherence")]
		public void AndIAdjustAdherenceBetweenForAllAgents(string from, string to)
		{
			var start = DateTime.Parse(from);
			var end = DateTime.Parse(to);
			Browser.Interactions.Click(".add-period");
			Browser.Interactions.FillWith(".start-date", start.ToString("yyyy-MM-dd HH:mm"));		
			Browser.Interactions.FillWith(".start-time", start.ToString("yyyy-MM-dd HH:mm"));
			Browser.Interactions.FillWith(".end-date", end.ToString("yyyy-MM-dd HH:mm"));
			Browser.Interactions.FillWith(".end-time", end.ToString("yyyy-MM-dd HH:mm"));
			Browser.Interactions.Click(".wfm-btn.adjust-adherence"); // fix
		}
	}
}