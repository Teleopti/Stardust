using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
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
			Browser.Interactions.AssertContains(".request-new-absence option", name);
		}

        [Then(@"I should see my existing inputs for date '(.*)'")]
        public void ThenIShouldSeeMyExistingInputsForDate(DateTime date)
        {
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value, Is.StringContaining("The cake is a.. Cake!"));
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value, Is.StringContaining("A message. A very very very short message. Or maybe not."));
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value, Is.StringContaining(date.ToShortDateString(UserFactory.User().Culture)));
		}

		[Given(@"the absence period is opened between '(.*)' and '(.*)'")]
		public void GivenTheAbsencePeriodIsOpenedBetweenAnd(DateTime start, DateTime end)
		{
			ScenarioContext.Current.Pending();
		}
	}
}
