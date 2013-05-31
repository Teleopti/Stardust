﻿using System;
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
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-subject", "The cake is a.. Cake!");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-message", "A message. A very very very short message. Or maybe not.");
			Browser.Interactions.SelectOptionByTextUsingJQuery("#Request-add-section .request-new-absence", name);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-datefrom", date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-dateto", date.ToShortDateString(UserFactory.User().Culture));	
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
			Browser.Interactions.Click("#Request-add-section .request-new-send");
		}

		[When(@"I click the update button")]
		public void WhenIClickTheUpdateButton()
		{
			Browser.Interactions.Click(".request-edit .request-edit-update");
		}
	}
}
