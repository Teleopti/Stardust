using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;

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

		[Then(@"I should not see any indication of how many agents can go on holiday")]
		public void ThenIShouldNotSeeAnyIndicationOfHowManyAgentsCanGoOnHoliday()
		{
			var indicators = Pages.Pages.WeekSchedulePage.AbsenceIndiciators();
			foreach (var indicator in indicators)
			{
				EventualAssert.That(indicator.IsDisplayed, Is.False);
			}
		}

		[Then(@"I should see a '(.*)' indication for chance of absence request on '(.*)'")]
		public void ThenIShouldSeeAIndicationForChanceOfAbsenceRequestOn(string color, DateTime date)
		{
		}

		[Given(@"there is no allowance left for date '(.*)'")]
		public void GivenThereIsNoAllowanceLeftForDate(DateTime	date)
		{
		}
	}
}
