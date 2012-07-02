using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;

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

		[When(@"I click add request button in the toolbar")]
		public void WhenIClickAddRequestButtonInTheToolbar()
		{
			Pages.Pages.CurrentEditRequestPage.AddRequestButton.EventualClick();
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
		}

		[When(@"I click absence request tab")]
		public void WhenIClickAbsenceRequestTab()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.EventualClick();
		}

		[When(@"I input absence request values with (.*)")]
		public void WhenIInputAbsenceRequestValuesWithVacation(string name)
		{
			Pages.Pages.CurrentEditRequestPage.AbsenceTypesElement.WaitUntilDisplayed();
			var date = DateTime.Today;
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

		[Then(@"I should not see the absence request tab")]
		public void ThenIShouldNotSeeTheAbsenceRequestTab()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.Exists, Is.False);
		}

		[Then(@"I should not see the absence request tab \(invisible\)")]
		public void ThenIShouldNotSeeTheAbsenceRequestTabInvisible()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.DisplayHidden(), Is.True);
		}

	}
}
