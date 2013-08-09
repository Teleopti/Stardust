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
			Browser.Interactions.AssertFirstNotContains("body", "VCALENDAR");
		}

		[Then(@"Someone should see ical calendar with")]
		public void ThenSomeoneShouldSeeIcalCalendarWith(Table table)
		{
			Browser.Interactions.AssertFirstContains("body", "VCALENDAR");
			var item = table.CreateInstance<IcsCalendarItem>();
			Browser.Interactions.AssertFirstContains("body", item.SUMMARY);
			Browser.Interactions.AssertFirstContains("body", item.DTSTART);
			Browser.Interactions.AssertFirstContains("body", item.DTEND);
		}

		[Then(@"Someone should not see ical calendar with")]
		public void ThenSomeoneShouldNotSeeIcalCalendarWith(Table table)
		{
			Browser.Interactions.AssertFirstContains("body", "VCALENDAR");
			var item = table.CreateInstance<IcsCalendarItem>();
			Browser.Interactions.AssertFirstNotContains("body", item.DTSTART);
			Browser.Interactions.AssertFirstNotContains("body", item.DTEND);
		}

		public class IcsCalendarItem
		{
			public string SUMMARY { get; set; }
			public string DTSTART { get; set; }
			public string DTEND { get; set; }

		}
	}

}