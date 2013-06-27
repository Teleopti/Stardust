using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class ManageRequestStepDefinitions 
	{
		[Given(@"I have a requestable absence called (.*)")]
		public void GivenIHaveARequestableAbsenceCalledVacation(string name)
		{
			UserFactory.User().Setup(new RequestableAbsenceType(name));
		}

		[When(@"I input absence request values with (\S*)")]
		public void WhenIInputAbsenceRequestValuesWith(string name)
		{
			WhenIInputAbsenceRequestValuesWithForDate(name, DateTime.Today);
		}

		[When(@"I input absence request values with '(.*)' for date '(.*)'")]
        public void WhenIInputAbsenceRequestValuesWithForDate(string name, DateTime date)
        {
            Pages.Pages.CurrentEditRequestPage.AbsenceTypesElement.WaitUntilDisplayed();
            var time = date.AddHours(12);
            Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value = "The cake is a.. Cake!";
            Pages.Pages.CurrentEditRequestPage.AbsenceTypesSelectList.Select(name);
            Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value = date.ToShortDateString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value = time.ToShortTimeString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value = date.ToShortDateString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value = time.AddHours(1).ToShortTimeString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value = "A message. A very very very short message. Or maybe not.";
        }

		[Then(@"I should see the text request in the list")]
		public void ThenIShouldSeeTheTextRequestInTheList()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.FirstRequest.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.FirstRequest.InnerHtml, Is.StringContaining("Text"));
		}

		[Given(@"I am an agent without access to absence requests")]
		public void GivenIAmAnAgentWithoutAccessToAbsenceRequests()
		{
			UserFactory.User().Setup(new AgentWithoutAbsenceRequestsAccess());
		}

	}
}
