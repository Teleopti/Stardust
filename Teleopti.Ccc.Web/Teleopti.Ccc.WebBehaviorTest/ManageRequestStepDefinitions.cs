using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions;
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
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section input[data-bind='value: Subject']", "The cake is a.. Cake!");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section textarea[data-bind='value: Message']", "A message. A very very very short message. Or maybe not.");	
			Browser.Interactions.SelectOptionByTextUsingJQuery("#Request-add-section select[data-bind='value: AbsenceId']", name);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section input[data-bind*='datepicker: DateFrom']", date.ToShortDateString(UserFactory.User().Culture));	
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section input[data-bind*='datepicker: DateTo']", date.ToShortDateString(UserFactory.User().Culture));	
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

		[When(@"I click the send button")]
		public void WhenIClickTheSendButton()
		{
			Browser.Interactions.Click("#Request-add-section button[data-bind*='click: AddRequest']");
		}

		[When(@"I click the update button")]
		public void WhenIClickTheUpdateButton()
		{
			Browser.Interactions.Click(".bdd-request-edit-detail button[data-bind*='click: AddRequest']");
		}
	}
}
