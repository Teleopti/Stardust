﻿using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
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
			DataMaker.Data().Setup(new RequestableAbsenceType(name));
		}

		[When(@"I input absence request values with (\S*)")]
		public void WhenIInputAbsenceRequestValuesWith(string name)
		{
			WhenIInputAbsenceRequestValuesWithForDate(name, DateOnlyForBehaviorTests.TestToday.Date);
		}

		[When(@"I input absence request values with '(.*)' for date '(.*)'")]
        public void WhenIInputAbsenceRequestValuesWithForDate(string name, DateTime date)
        {
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-subject", "The cake is a.. Cake!");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-message", "A message. A very very very short message. Or maybe not.");
			Browser.Interactions.SelectOptionByTextUsingJQuery("#Request-add-section .request-new-absence", name);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-datefrom", date.ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-dateto", date.ToShortDateString(DataMaker.Data().MyCulture));	
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
			DataMaker.Data().Setup(new AgentWithoutAbsenceRequestsAccess());
		}

		[When(@"I click the send button")]
		public void WhenIClickTheSendButton()
		{
			Browser.Interactions.Click("#Request-add-section .request-new-send");
		}

		[When(@"I click the update button on the request at position '(.*)' in the list")]
		public void WhenIClickTheUpdateButtonOnTheRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.Click(string.Format(".request-list .request:nth-child({0}) .request-edit-update", position));
		}
	}
}
