using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using List = WatiN.Core.List;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AbsenceRequestFromScheduleStepDefinitions 
	{

		[Then(@"I should see an absence type called (.*) in droplist")]
		public void ThenIShouldSeeAAbsenceTypeCalledVacationInDroplist(string name)
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceTypesSelectList.InnerHtml, Is.StringContaining(name));
		}

        [Then(@"I should see my existing inputs for date '(.*)'")]
        public void ThenIShouldSeeMyExistingInputsForDate(DateTime date)
        {
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value, Is.StringContaining("The cake is a.. Cake!"));
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value, Is.StringContaining("A message. A very very very short message. Or maybe not."));
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value, Is.StringContaining(date.ToShortDateString(UserFactory.User().Culture)));
		}

		[Then(@"I should see an indication of the amount of agents that can go on holiday on each day of the week")]
		public void ThenIShouldSeeAnIndicationOfTheAmountOfAgentsThatCanGoOnHolidayOnEachDayOfTheWeek()
		{
			var page = Pages.Pages.WeekSchedulePage;
			var days = page.DayElements;

			foreach (List element in days)
			{
				var holidayIndication = element.Divs.First(Find.BySelector("holiday-agents"));
				EventualAssert.That(holidayIndication.IsDisplayed,Is.True);
			}
		}

		[Then(@"I should not see any indication of how many agents can go on holiday")]
		public void ThenIShouldNotSeeAnyIndicationOfHowManyAgentsCanGoOnHoliday()
		{
			var page = Pages.Pages.WeekSchedulePage;
			var days = page.DayElements;

			foreach (List element in days)
			{
				var holidayIndication = element.Divs.First(Find.BySelector("holiday-agents"));
				EventualAssert.That(holidayIndication.IsDisplayed, Is.False);
			}
		}


		[Given(@"the absence period is opened between '(.*)' and '(.*)'")]
		public void GivenTheAbsencePeriodIsOpenedBetweenAnd(DateTime start, DateTime end)
		{
			ScenarioContext.Current.Pending();
		}


	}
}
