using System;
using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferenceAlertWhenBrokenMinOrMaxHoursStepDefinition
	{
		[Then(@"I should see min hours per week as '(.*)'")]
		public void ThenIShouldSeeMinHoursPerWeekAs(String minHours)
		{
			Browser.Interactions.AssertAnyContains(".min-hours-per-week", minHours);
		}

		[Then(@"I should see max hours per week as '(.*)'")]
		public void ThenIShouldSeeMaxHoursPerWeekAs(String maxHours)
		{
			Browser.Interactions.AssertAnyContains(".max-hours-per-week", maxHours);
		}


	}
}
