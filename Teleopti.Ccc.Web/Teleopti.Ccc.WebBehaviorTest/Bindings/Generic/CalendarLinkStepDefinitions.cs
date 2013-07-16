using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class CalendarLinkStepDefinitions
	{
		[Given(@"I have shared calendar")]
		public void GivenIHaveSharedCalendar()
		{
			var calendarLinkConfigurable = new CalendarLinkConfigurable
				{
					IsActive = true
				};
			UserFactory.User().Setup(calendarLinkConfigurable);
		}

		[Then(@"I should see '(.*)' active")]
		public void ThenIShouldSeeActive(CssClass cssClass)
		{
			Browser.Interactions.AssertExists(string.Format(".{0}.active", cssClass.Name));
		}

		[Then(@"I should see '(.*)' inactive")]
		public void ThenIShouldSeeInactive(CssClass cssClass)
		{
			Browser.Interactions.AssertNotExists(string.Format(".{0}", cssClass.Name), ".share-my-calendar.active");
		}

		[Then(@"I should see a sharing link")]
		public void ThenIShouldSeeASharingLink()
		{
			Browser.Interactions.AssertExists(".calendar-url");
			ScenarioContext.Current.Add("CalendarLink", Browser.Interactions.Value(".calendar-url"));
		}

		[Then(@"I should not see a sharing link")]
		public void ThenIShouldNotSeeASharingLink()
		{
			Browser.Interactions.AssertNotExists(".share-my-calendar", ".calendar-url");
		}

		[Then(@"I should not see '(.*)' in settings")]
		public void ThenIShouldNotSeeInSettings(CssClass cssClass)
		{
			Browser.Interactions.AssertNotExists("#settings", "." + cssClass.Name);
		}

		[When(@"Someone is viewing sharing link")]
		public void WhenSomeoneIsViewingSharingLink()
		{
			var url = ScenarioContext.Current.Get<string>("CalendarLink");
			Navigation.GotoRaw(url);
		}

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


	}

	public class IcsCalendarItem
	{
		public string SUMMARY { get; set; }
		public string DTSTART { get; set; }
		public string DTEND { get; set; }

	}
}