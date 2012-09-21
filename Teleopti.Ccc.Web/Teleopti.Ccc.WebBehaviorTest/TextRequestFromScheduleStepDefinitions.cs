using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using WatiN.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TextRequestFromScheduleStepDefinitions
	{
        [When(@"I click on the day symbol area for date '(.*)'")]
        public void WhenIClickOnTheDaySymbolAreaForDate(DateTime date)
        {
            Pages.Pages.WeekSchedulePage.DayElementForDate(date).ListItems.First(Find.ById("day-symbol")).Div(Find.ById("add-request-cell")).EventualClick();
        }

        [When(@"I click on the day summary for date '(.*)'")]
        public void WhenIClickOnTheDaySummaryForDate(DateTime date)
        {
            Pages.Pages.WeekSchedulePage.DayElementForDate(date).ListItems.First(Find.ById("day-summary")).EventualClick();
        }

		[Then(@"I should see the text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
		}

        [Then(@"I should see the text request form with '(.*)' as default date")]
        public void ThenIShouldSeeTheTextRequestFormWithAsDefaultDate(DateTime date)
        {
            EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), Is.EqualTo(date));
            EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value), Is.EqualTo(date));
        }

		[Then(@"I should not see the text request form")]
		public void ThenIShouldNotSeeTheTextRequestForm()
		{
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.False);
		}
	}
}
