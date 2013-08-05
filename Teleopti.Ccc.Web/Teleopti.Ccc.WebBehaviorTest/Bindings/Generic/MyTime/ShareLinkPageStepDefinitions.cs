using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class ShareLinkPageStepDefinitions
	{

		[Then(@"Someone should not see ical calendar")]
		public void ThenSomeoneShouldNotSeeIcalCalendar()
		{
			Browser.Current.Html.Contains("VCALENDAR");
		}

		[Then(@"Someone should see ical calendar with")]
		public void ThenSomeoneShouldSeeIcalCalendarWith(Table table)
		{
			var item = table.CreateInstance<IcsCalendarItem>();
			Browser.Interactions.AssertContains("body", item.SUMMARY);
			Browser.Interactions.AssertContains("body", item.DTSTART);
			Browser.Interactions.AssertContains("body", item.DTEND);
		}

		[Then(@"Someone should not see ical calendar with")]
		public void ThenSomeoneShouldNotSeeIcalCalendarWith(Table table)
		{
			var item = table.CreateInstance<IcsCalendarItem>();
			Browser.Interactions.AssertNotContains("body", item.DTSTART);
			Browser.Interactions.AssertNotContains("body", item.DTEND);
		}

		public class IcsCalendarItem
		{
			public string SUMMARY { get; set; }
			public string DTSTART { get; set; }
			public string DTEND { get; set; }

		}
	}

}