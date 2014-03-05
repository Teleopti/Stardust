using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TimeStepDefinitions
	{
		[Given(@"the current time is '(.*)'")]
		[Given(@"the time is '(.*)'")]
		[When(@"the current time is '(.*)'")]
		[When(@"the time is '(.*)'")]
		public void GivenCurrentTimeIs(DateTime time)
		{
			CurrentTime.Set(time);
		}

		[When(@"the current browser time is '(.*)'")]
		public void GivenTheCurrentBrowserTimeIs(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTeleoptiTime = function () {{ return new Date({0}, {1}, {2}, {3}, {4}, {5}).getTime(); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(setJsDate);
		}


		[When(@"current browser time has changed to '(.*)'")]
		public void WhenCurrentBrowserTimeHasChangedTo(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTeleoptiTime = function () {{ return new Date({0}, {1}, {2}, {3}, {4}, {5}).getTime(); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);

			Browser.Interactions.Javascript(setJsDate);
			//need to set on js date object on popup as well
			var popupConstraint = Find.ByUrl(new Uri(TestSiteConfigurationSetup.Url, "MyTime/Asm"));
			if (WatiN.Core.Browser.Exists<IE>(popupConstraint))
			{
				WatiN.Core.Browser.AttachTo<IE>(popupConstraint).Eval(setJsDate);
			}

			var setJsTimeIndicatorMovement =
			    string.Format(@"Teleopti.MyTimeWeb.Schedule.SetTimeIndicator(new Date({0}, {1}, {2}, {3}, {4}, {5}));",
			                  time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(setJsTimeIndicatorMovement);
		}
	}
}