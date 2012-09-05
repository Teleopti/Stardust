using System;
using System.Globalization;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WeekScheduleTimeIndicatorStepDefinitions
	{
		private WeekSchedulePage _page { get { return Pages.Pages.WeekSchedulePage; } }

		[Then(@"I should see the time indicator at time '(.*)'")]
		public void ThenIShouldSeeTheTimeIndicatorAtTime(DateTime date)
		{
			var localDate = TimeZoneHelper.ConvertFromUtc(date, UserFactory.User().Person.PermissionInformation.DefaultTimeZone());
			var dateString = _page.DayElementForDate(date).GetAttributeValue("data-mytime-date");

			DateTime dateFromPage;
			if (DateTime.TryParse(dateString, UserFactory.User().Culture, DateTimeStyles.None, out dateFromPage))
			{
				dateFromPage.Date.Should().Be.EqualTo(localDate.Date);
			}
			dateFromPage.Should().Not.Be.EqualTo(null);

			var positionPercentage = (decimal)(localDate.TimeOfDay - TimeSpan.Zero).Ticks / (new TimeSpan(23, 59, 59) - TimeSpan.Zero).Ticks;
			const int heightOfDay = 668;
			var heightOfTimeIndicator = Math.Round(positionPercentage * heightOfDay, 0);

			_page.DayElementForDate(localDate).ListItems[4].Divs.Count.Should().Be.EqualTo(1);
			_page.DayElementForDate(localDate).ListItems[4].Divs[0].Style.GetAttributeValue("Top").Should().Be.EqualTo(
				Math.Round(heightOfTimeIndicator, 0).ToString(CultureInfo.InvariantCulture) + "px");
		}

		
	}
}