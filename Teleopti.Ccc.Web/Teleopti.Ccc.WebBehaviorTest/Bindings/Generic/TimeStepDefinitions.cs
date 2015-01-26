using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TimeStepDefinitions
	{
		[Given(@"the time is '(.*)'")]
		[When(@"the time is '(.*)'")]
		[SetCulture("sv-SE")]
		public void GivenCurrentTimeIs(DateTime time)
		{
			CurrentTime.Set(time);
		}

		[When(@"the browser local time is '(.*)'")]
		public void WhenTheBrowserLocalTimeIs(DateTime time)
		{
			var localTimeZone = TimeZoneInfo.Local.BaseUtcOffset.Hours;
			var utcTimeZone = TimeZoneInfo.Utc.BaseUtcOffset.Hours;
			var convertTime = time.AddHours(utcTimeZone - localTimeZone);
			const string setJsDateTemplate =
				@"Date.prototype.getTime = function () {{ return new Date(Date.UTC({0}, {1}, {2}, {3}, {4}, {5})); }};";
			var setJsDate = string.Format(setJsDateTemplate, convertTime.Year, convertTime.Month - 1, convertTime.Day,
				convertTime.Hour, convertTime.Minute, convertTime.Second);
			Browser.Interactions.Javascript(setJsDate);
		}
	}
}