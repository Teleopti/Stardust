using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AbsenceRequestFromScheduleStepDefinitions
	{
		[When(@"I click absence request tab")]
		public void WhenIClickAbsenceRequestTab()
		{
			Pages.Pages.CurrentEditTextRequestPage.AbsenceRequestTab.EventualClick();
		}


		[When(@"I input absence request values")]
		public void WhenIInputAbsenceRequestValues()
		{
			Pages.Pages.CurrentEditTextRequestPage.RequestDetailSection.WaitUntilDisplayed();
			var date = DateTime.Today;
			var time = date.AddHours(12);
			Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailSubjectInput.Value = "The cake is a.. Cake!";
			Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailFromDateInput.Value = date.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailFromTimeTextField.Value = time.ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailToDateTextField.Value = date.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailToTimeTextField.Value = time.AddHours(1).ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailMessageTextField.Value = "A message. A very very very short message. Or maybe not.";
		}

		[Then(@"I should see 00:00 - 23:59 as the default times")]
		public void ThenIShouldSee0000_2359AsTheDefaultTimes()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailFromTimeTextField.Value, Is.EqualTo("00:00"));
			EventualAssert.That(() => Pages.Pages.CurrentEditTextRequestPage.TextRequestDetailToTimeTextField.Value, Is.EqualTo("23:59"));
		}

		[Given(@"I have a requestable absence called (.*)")]
		public void GivenIHaveARequestableAbsenceCalledVacation(string name)
		{
			UserFactory.User().Setup(new RequestableAbsenceType(name));
		}

		[Then(@"I should see a absence type called (.*) in droplist")]
		public void ThenIShouldSeeAAbsenceTypeCalledVacationInDroplist(string name)
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditTextRequestPage.AbsenceTypesSelectList.InnerHtml, Is.StringContaining(name));
		}


		[Then(@"I should not see the absence request tab")]
		public void ThenIShouldNotSeeTheAbsenceRequestTab()
		{
			ScenarioContext.Current.Pending();
		}

		[Given(@"I am an agent without access to absence requests")]
		public void GivenIAmAnAgentWithoutAccessToAbsenceRequests()
		{
			ScenarioContext.Current.Pending();
		}
	}
}
