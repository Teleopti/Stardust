using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Interfaces.Domain;

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
			WhenIInputAbsenceRequestValuesWithForDate(name, DateOnlyForBehaviorTests.TestToday.Date);
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

		[When(@"I input overtime availability with")]
		public void WhenIInputOvertimeAvailabilityWith(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityViewModel>();
			int[] st = overtimeAvailability.StartTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var startTimeSpan = new TimeSpan(st[0], st[1], 0);
			int[] end = overtimeAvailability.EndTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var endTimeSpan = new TimeSpan(end[0], end[1], 0);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .overtime-availability-start-time", TimeHelper.TimeOfDayFromTimeSpan(startTimeSpan, UserFactory.User().Culture));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .overtime-availability-end-time", TimeHelper.TimeOfDayFromTimeSpan(endTimeSpan, UserFactory.User().Culture));
			if (overtimeAvailability.EndTimeNextDay)
				Browser.Interactions.Click(".overtime-availability-next-day");
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

		[When(@"I click the update button on the request at position '(.*)' in the list")]
		public void WhenIClickTheUpdateButtonOnTheRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.Click(string.Format(".request-list .request:nth-child({0}) .request-edit-update", position));
		}
	}
}
