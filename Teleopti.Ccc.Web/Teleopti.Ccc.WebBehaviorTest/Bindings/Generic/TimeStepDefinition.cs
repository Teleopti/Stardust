﻿using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TimeStepDefinition
	{
		[Given(@"Current time is '(.*)'")]
		public void GivenCurrentTimeIs(DateTime time)
		{
			Navigation.GoTo("Test/SetCurrentTime?dateSet=" + time);
		}

		[When(@"Current browser time has changed to '(.*)'")]
		public void WhenCurrentBrowserTimeHasChangedTo(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTeleoptiTime = function () {{ return new Date({0}, {1}, {2}, {3}, {4}, {5}).getTime(); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);

			Browser.Current.Eval(setJsDate);
			//need to set on js date object on popup as well
			var popupConstraint = Find.ByUrl(new Uri(TestSiteConfigurationSetup.Url, "MyTime/Asm"));
			if (WatiN.Core.Browser.Exists<IE>(popupConstraint))
			{
				WatiN.Core.Browser.AttachTo<IE>(popupConstraint).Eval(setJsDate);
			}

			var setJsTimeIndicatorMovement =
			    string.Format(@"Teleopti.MyTimeWeb.Schedule.SetTimeIndicator(new Date({0}, {1}, {2}, {3}, {4}, {5}));",
			                  time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Current.Eval(setJsTimeIndicatorMovement);
		}
	}
}