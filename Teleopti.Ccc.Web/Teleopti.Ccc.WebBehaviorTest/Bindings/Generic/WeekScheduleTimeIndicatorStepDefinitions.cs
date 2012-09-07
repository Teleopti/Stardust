using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
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

		[Given(@"I should see the time indicator at time '(.*)'")]
		[Then(@"I should see the time indicator at time '(.*)'")]
		public void ThenIShouldSeeTheTimeIndicatorAtTime(DateTime date)
		{
			//var dateString = _page.DayElementForDate(date).GetAttributeValue("data-mytime-date");
			//DateTime dateFromPage;
			//if (DateTime.TryParse(dateString, UserFactory.User().Culture, DateTimeStyles.None, out dateFromPage))
			//    dateFromPage.Date.Should().Be.EqualTo(date.Date);
			//dateFromPage.Should().Not.Be.EqualTo(null);

			const int heightOfDay = 668;
			const int timeLineOffset = 203;
			const int timeIndicatorHeight = 2;
			var positionPercentage = (decimal)(date.TimeOfDay - TimeSpan.Zero).Ticks / (new TimeSpan(23, 59, 59) - TimeSpan.Zero).Ticks;
			var heightOfTimeIndicator = Math.Round(positionPercentage * heightOfDay, 0) - Math.Round((decimal) (timeIndicatorHeight / 2), 0);

			EventualAssert.That(() => _page.TimeIndicatorForDate(date).Style.GetAttributeValue("Top"), Is.EqualTo(Math.Round(heightOfTimeIndicator, 0).ToString(CultureInfo.InvariantCulture) + "px"));
			EventualAssert.That(() => _page.TimeIndicatorInTimeLine.Style.GetAttributeValue("Top"),
			                    Is.EqualTo((Math.Round(heightOfTimeIndicator, 0) + timeLineOffset).ToString(CultureInfo.InvariantCulture) + "px"));
		}

		[Then(@"I should not see the time indicator for date '(.*)'")]
		public void ThenIShouldNotSeeTheTimeIndicatorForDate(DateTime date)
		{
			_page.TimeIndicatorForDate(date).Style.GetAttributeValue("display").Should().Be.EqualTo("none");
			_page.TimeIndicatorInTimeLine.Style.GetAttributeValue("display").Should().Be.EqualTo("none");
		}

		[Then(@"I should not see the time indicator")]
		public void ThenIShouldNotSeeTheTimeIndicator()
		{
			EventualAssert.That(() => _page.FirstDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.SecondDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.ThirdDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.FourthDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.FifthDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.SixthDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.SeventhDay.ListItems[4].Divs[0].Style.GetAttributeValue("display"), Is.EqualTo("none"));
			EventualAssert.That(() => _page.TimeIndicatorInTimeLine.Style.GetAttributeValue("display"), Is.EqualTo("none"));
		}

	}
}