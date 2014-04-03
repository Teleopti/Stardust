using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class ManageRequestStepDefinitions
	{
		[Given(@"I have a requestable absence called (.*)")]
		public void GivenIHaveARequestableAbsenceCalledVacation(string name)
		{
			DataMaker.Data().Apply(new RequestableAbsenceType(name));
		}

		[Given(@"I have a requestable absence with")]
		public void GivenIHaveARequestableAbsenceWith(Table table)
		{
			var requestableAbsenceFields = table.CreateInstance<RequestableAbsenceFields>();
			DataMaker.Data().Apply(new RequestableAbsenceType(requestableAbsenceFields));
		}

		[When(@"I input absence request values with (\S*)")]
		public void WhenIInputAbsenceRequestValuesWith(string name)
		{
			WhenIInputAbsenceRequestValuesWithForDate(name, DateOnlyForBehaviorTests.TestToday.Date);
		}

		[Given(@"I input absence request values with '(.*)' for date '(.*)'")]
		[When(@"I input absence request values with '(.*)' for date '(.*)'")]
		public void WhenIInputAbsenceRequestValuesWithForDate(string name, DateTime date)
		{
			WhenIInputAbsenceRequestValuesWithFromTo(name, date, date);
		}

		[When(@"I input absence request values with ""(.*)"" from ""(.*)"" to ""(.*)""")]
		public void WhenIInputAbsenceRequestValuesWithFromTo(string absenceName, DateTime dateFrom, DateTime dateTo)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-subject", "The cake is a.. Cake!");
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-message", "A message. A very very very short message. Or maybe not.");
			Browser.Interactions.SelectOptionByTextUsingJQuery("#Request-add-section .request-new-absence", absenceName);

			Browser.Interactions.Javascript(string.Format("$('#Request-add-section .request-new-datefrom').datepicker('set', '{0}');",
							  dateFrom.ToShortDateString(DataMaker.Data().MyCulture)));
			Browser.Interactions.Javascript(string.Format("$('#Request-add-section .request-new-dateto').datepicker('set', '{0}');",
							  dateTo.ToShortDateString(DataMaker.Data().MyCulture)));
		}

		[When(@"I input overtime availability with")]
		public void WhenIInputOvertimeAvailabilityWith(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityFields>();
			Browser.Interactions.Javascript(string.Format("$('#Request-add-section .overtime-availability-start-time').timepicker('setTime', '{0}');", overtimeAvailability.StartTime));
			Browser.Interactions.Javascript(string.Format("$('#Request-add-section .overtime-availability-end-time').timepicker('setTime', '{0}');", overtimeAvailability.EndTime));
			if (overtimeAvailability.EndTimeNextDay)
				Browser.Interactions.Click(".overtime-availability-next-day");
		}

		[Then(@"I should see the text request in the list")]
		public void ThenIShouldSeeTheTextRequestInTheList()
		{
			Browser.Interactions.AssertFirstContains(".request-body .request-data-type", "Text");
		}

		[Given(@"I am an agent without access to absence requests")]
		public void GivenIAmAnAgentWithoutAccessToAbsenceRequests()
		{
			DataMaker.Data().Apply(new AgentWithoutAbsenceRequestsAccess());
		}

		[When(@"I click the send button")]
		public void WhenIClickTheSendButton()
		{
			Browser.Interactions.Click("#Request-add-section .request-new-send");
		}

		[When(@"I submit my changes for the existing text request")]
		public void WhenISubmitMyChangesForTheExistingTextRequest()
		{
			Browser.Interactions.Click(".request-list .request .request-edit-update");
		}
	}
}
