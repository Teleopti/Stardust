using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TextRequestFromScheduleStepDefinitions
	{
		[When(@"I click on today's summary")]
		public void WhenIClickOnTodaySSummary()
		{
			Pages.Pages.WeekSchedulePage.DayElementForDate(DateTime.Today).ListItems.First(Find.ById("day-summary")).Div(Find.ById("add-request-cell")).EventualClick();
		}

		[When(@"I click on last day of current week's summary")]
		public void WhenIClickOnLastDayOfCurrentWeekSSummary()
		{
			var lastDayOfCurrentWeek = TestDataSetup.LastDayOfCurrentWeek(UserFactory.User().Culture);
			Pages.Pages.WeekSchedulePage.DayElementForDate(lastDayOfCurrentWeek).ListItems.First(Find.ById("day-summary")).Div(Find.ById("add-request-cell")).EventualClick();
		}

		[Then(@"I should see the text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the text request form with last day as default date")]
		public void ThenIShouldSeeTheTextRequestFormWithLastDayAsDefaultDate()
		{
			var lastDayOfCurrentWeek = TestDataSetup.LastDayOfCurrentWeek(UserFactory.User().Culture);
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), Is.EqualTo(lastDayOfCurrentWeek));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value), Is.EqualTo(lastDayOfCurrentWeek));
		}

		[Then(@"I should not see the text request form")]
		public void ThenIShouldNotSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.False);
		}


	}
}
