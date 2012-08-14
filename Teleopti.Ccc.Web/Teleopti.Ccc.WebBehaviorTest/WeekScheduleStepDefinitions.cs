using System;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using WatiN.Core;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class WeekScheduleStepDefinitions
	{

		private WeekSchedulePage _page { get { return Pages.Pages.WeekSchedulePage; } }

		[When(@"I click on the meeting")]
		public void WhenIClickOnTheMeeting()
		{
			// either we should click or the specs shouldnt say so.
		}

		[When(@"I click on any day of a week")]
		public void WhenIClickOnAnyDayOfAWeek()
		{
			_page.ClickThirdDayOfOtherWeekInWeekPicker(UserFactory.User().Culture);
		}

		[When(@"I click the request symbol")]
		public void WhenIClickTheRequestSymbol()
		{
			Pages.Pages.WeekSchedulePage.DayElementForDate(DateTime.Today).ListItems.First(Find.ById("day-summary")).Div(
				Find.ByClass("text-request", false)).EventualClick();
		}

		[Then(@"I should not see any shifts")]
		public void ThenIShouldNotSeeAnyShifts()
		{
			_page.FirstDay.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
			_page.SecondDay.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
			_page.ThirdDay.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
			_page.FourthDay.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
			_page.Fifith.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
		}

		[Then(@"I should not see any shifts after wednesday")]
		public void ThenIShouldNotSeeAnyShiftsAfterWednesday()
		{
			_page.ThirdDay.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.True();
			_page.FourthDay.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
			_page.Fifith.InnerHtml.Contains(TestData.ShiftCategory.Description.Name).Should().Be.False();
		}

		[Then(@"I should see the start and end dates for current week")]
		public void ThenIShouldSeeTheStartAndEndDatesForCurrentWeek()
		{
			AssertShowingWeekForDay(TestDataSetup.FirstDayOfCurrentWeek(UserFactory.User().Culture));
		}

		[Then(@"the shift should end on monday")]
		public void ThenTheShiftShouldEndOnMonday()
		{
			var contents = _page.FirstDay.InnerHtml;

			var indexForShiftStart = contents.IndexOf(TestData.ActivityPhone.Description.Name);
			var indexForShiftEnd = contents.IndexOf(TestData.ActivityPhone.Description.Name,
													indexForShiftStart +
													TestData.ActivityPhone.Description.Name.Length);

			indexForShiftEnd.Should().Be.GreaterThan(indexForShiftStart);
		}

		[Then(@"I should see the meeting details")]
		public void ThenIShouldSeeTheMeetingDetails()
		{
			var data = UserFactory.User().UserData<MeetingOnThursday>();
			_page.FourthDay.InnerHtml.Contains(data.MeetingSubject).Should().Be.True();
			_page.FourthDay.InnerHtml.Contains(data.MeetingLocation).Should().Be.True();
		}

		[Then(@"I should see the public note on tuesday")]
		public void ThenIShouldSeeThePublicNoteOnTuesday()
		{
			EventualAssert.That(() =>_page.SecondDayComment.Exists, Is.True);
		}

		[Then(@"I should see the selected week")]
		public void ThenIShouldSeeTheSelectedWeek()
		{
			AssertShowingWeekForDay(TestDataSetup.ThirdDayOfOtherThanCurrentWeekInCurrentMonth(UserFactory.User().Culture));
		}

		[Then(@"I should see monday as the first day of week")]
		public void ThenIShouldSeeMondayAsTheFirstDayOfWeek()
		{
			_page.DatePicker.CalendarFirstDayNumbers.Should().Contain(TestDataSetup.FirstDayOfAnyWeekInCurrentMonth(UserFactory.User().Culture).Day);
		}

		[Then(@"I should see sunday as the first day of week")]
		public void ThenIShouldSeeSundayAsTheFirstDayOfWeek()
		{
			_page.DatePicker.CalendarFirstDayNumbers.Should().Contain(TestDataSetup.FirstDayOfAnyWeekInCurrentMonth(UserFactory.User().Culture).Day);
		}

		[Then(@"I should see a number with the request count")]
		public void ThenIShouldSeeANumberWithTheTextRequestCount()
		{
			var date = UserFactory.User().UserData<ExistingTextRequest>().PersonRequest.Request.Period.StartDateTime.Date;
			var textRequest = _page.RequestForDate(date);
			EventualAssert.That(() => textRequest.InnerHtml, Is.StringContaining("1"));
		}

		[Then(@"I should see (.*) with the request count")]
		public void ThenIShouldSee2WithTheRequestCount(int count)
		{
			var date = UserFactory.User().UserData<ExistingTextRequest>().PersonRequest.Request.Period.StartDateTime.Date;
			var textRequest = _page.RequestForDate(date);
			EventualAssert.That(() => textRequest.InnerHtml, Is.StringContaining("2"));
		}


		[Then(@"I should see a symbol at the top of the schedule for the first day")]
		public void ThenIShouldSeeASymbolAtTheTopOfTheScheduleForTheFirstDay()
		{
			var startDate = UserFactory.User().UserData<ExistingTextRequestOver2Days>().PersonRequest.Request.Period.StartDateTime.Date;
			var endDate = UserFactory.User().UserData<ExistingTextRequestOver2Days>().PersonRequest.Request.Period.EndDateTime.Date;
			var iconDay1 = _page.RequestForDate(startDate);
			var iconDay2 = _page.RequestForDate(endDate);
			EventualAssert.That(() => iconDay1.DisplayVisible(), Is.True);
			EventualAssert.That(() => iconDay2.DisplayVisible(), Is.False);
		}

		[Then(@"I should see start timeline and end timeline according to schedule with:")]
		public void ThenIShouldSeeStartTimelineAndEndTimelineAccordingToScheduleWith(Table table)
		{
			var divs = _page.Timelines.Divs;
			EventualAssert.That(() => string.Format("{0}", divs.Count), Is.EqualTo(table.Rows[2][1]));
			EventualAssert.That(() => divs[0].InnerHtml, Is.StringContaining(table.Rows[0][1]));
			EventualAssert.That(() => divs[divs.Count - 1].InnerHtml, Is.StringContaining(table.Rows[1][1]));
		}

		[Then(@"I should see wednesday's activities:")]
		public void ThenIShouldSeeWednesdaySActivities(Table table)
		{
			EventualAssert.That(() => string.Format("{0}", _page.ThirdDay.ListItems[4].Divs[0].OuterHtml), Is.StringContaining(table.Rows[0][1]));
			EventualAssert.That(() => string.Format("{0}", _page.ThirdDay.ListItems[4].Divs[0].Style.Height), Is.StringContaining(table.Rows[0][2]));
			EventualAssert.That(() => string.Format("{0}", _page.ThirdDay.ListItems[4].Divs[0].Style.BackgroundColor), Is.StringContaining(table.Rows[0][3]));

			EventualAssert.That(() => string.Format("{0}", _page.ThirdDay.ListItems[4].Divs[1].OuterHtml), Is.StringContaining(table.Rows[1][1]));
			EventualAssert.That(() => string.Format("{0}", _page.ThirdDay.ListItems[4].Divs[1].Style.Height), Is.StringContaining(table.Rows[1][2]));
			EventualAssert.That(() => string.Format("{0}", _page.ThirdDay.ListItems[4].Divs[1].Style.BackgroundColor), Is.StringContaining(table.Rows[1][3]));
		}

		[Then(@"I should see request page")]
		public void ThenIShouldSeeRequestPage()
		{
			EventualAssert.That(() => Pages.Pages.Current.Document.Uri.AbsoluteUri, Is.StringContaining("Request"));
		}

		[When(@"I click the current week button")]
		public void WhenIClickTheCurrentWeekButton()
		{
			Pages.Pages.WeekSchedulePage.TodayButton.EventualClick();
		}

		private void AssertShowingWeekForDay(DateTime anyDayOfWeek)
		{
			var firstDayOfWeek = DateHelper.GetFirstDateInWeek(anyDayOfWeek, UserFactory.User().Culture);
			var lastDayOfWeek = DateHelper.GetLastDateInWeek(anyDayOfWeek, UserFactory.User().Culture);
			EventualAssert.WhenElementExists(_page.FirstDay, d => d.GetAttributeValue("data-mytime-date"), Is.EqualTo(firstDayOfWeek.ToString("yyyy-MM-dd")));
			EventualAssert.WhenElementExists(_page.SeventhDay, d => d.GetAttributeValue("data-mytime-date"), Is.EqualTo(lastDayOfWeek.ToString("yyyy-MM-dd")));
		}

	}
}
