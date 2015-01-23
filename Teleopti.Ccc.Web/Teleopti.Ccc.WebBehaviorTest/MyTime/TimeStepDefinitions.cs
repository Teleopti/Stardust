using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	[Scope(Feature = "ASM")]
	[Scope(Feature = "Alert agent activity is changing")]
	[Scope(Feature = "Week schedule time indicator")]
	public class TimeStepDefinitions2
	{
		[When(@"current browser time has changed to '(.*)'")]
		public void WhenCurrentBrowserTimeHasChangedTo(DateTime time)
		{
			fakeTimeByLegacyASMMethod(time);
		}

		private static void fakeTimeByLegacyASMMethod(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTeleoptiTime = function () {{ return new Date({0}, {1}, {2}, {3}, {4}, {5}).getTime(); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute,
				time.Second);

			Browser.Interactions.Javascript(setJsDate);

			var setJsTimeIndicatorMovement =
				string.Format(@"Teleopti.MyTimeWeb.Schedule.SetTimeIndicator(new Date({0}, {1}, {2}, {3}, {4}, {5}));",
					time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(setJsTimeIndicatorMovement);
		}

	}
}