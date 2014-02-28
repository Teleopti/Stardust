using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.MyTime;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class EditRequestPageStepDefinitions
	{
		[Then(@"I should not see the New Absence Request menu item")]
		public void ThenIShouldNotSeeTheNewAbsenceRequestMenuItem()
		{
			Browser.Interactions.AssertNotExists(".text-request-add", ".absence-request-add");
		}

		[Then(@"I should not see the New Shift Trade Request menu item")]
		public void ThenIShouldNotSeeTheNewShiftTradeRequestMenuItem()
		{
			Browser.Interactions.AssertNotExists(".text-request-add", ".shifttrade-request-add");
		}

		[When(@"I click to add a new text request")]
		public void WhenIClickToAddANewTextRequest()
		{
			Browser.Interactions.Click(".text-request-add");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[Given(@"I click to add a new absence request")]
		[When(@"I click to add a new absence request")]
		public void WhenIClickToAddANewAbsenceRequest()
		{
			Browser.Interactions.Click(".absence-request-add");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I click overtime availability")]
		[When(@"I click add new overtime availability")]
		public void WhenIClickAddNewOvertimeAvailability()
		{
			Browser.Interactions.Click(".overtime-availability-add");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I unchecked the full day checkbox")]
		public void WhenIUncheckedTheFullDayCheckbox()
		{
			TextRequestStepDefinitions.UncheckFullDayCheckbox();
		}

		[When(@"I checked the full day checkbox")]
		public void WhenIClickFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("return $('#Request-add-section .request-new-fullday:enabled').prop('checked')").ToString() == "false")
				Browser.Interactions.Click("#Request-add-section .request-new-fullday");
		}

		[Then(@"I should not be able to input values for text request at position '(.*)' in the list")]
		public void ThenIShouldNotBeAbleToInputValuesForTextRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format(".request:nth-child({0}) .request-edit-subject", position));
 
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-datefrom:not(:enabled)", position));
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-timefrom:not(:enabled)", position));

			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-dateto:not(:enabled)", position));
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-timeto:not(:enabled)", position));

			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-fullday:not(:enabled)", position));
		}

		[Then(@"I should see (.*) - (.*) as the default times")]
		public void ThenIShouldSee800_1700AsTheDefaultTimes(string startTime, string endTime)
		{
			int[] st = startTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var startTimeSpan = new TimeSpan(st[0], st[1], 0);
			int[] end = endTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var endTimeSpan = new TimeSpan(end[0], end[1], 0);

			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-timefrom",
												TimeHelper.TimeOfDayFromTimeSpan(startTimeSpan, DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-timeto",
												TimeHelper.TimeOfDayFromTimeSpan(endTimeSpan, DataMaker.Data().MyCulture));
		}

		[Then(@"I should see add overtime availability form with")]
		public void ThenIShouldSeeAddOvertimeAvailabilityFormWith(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityFields>();
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .overtime-availability-start-date",
			                                                 overtimeAvailability.StartDate.ToShortDateString(
				                                                 DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .overtime-availability-end-date",
															 overtimeAvailability.EndDate.ToShortDateString(
																 DataMaker.Data().MyCulture));

			var st = overtimeAvailability.StartTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var startTimeSpan = new TimeSpan(st[0], st[1], 0);
			var end = overtimeAvailability.EndTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var endTimeSpan = new TimeSpan(end[0], end[1], 0);

			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .overtime-availability-start-time",
												TimeHelper.TimeOfDayFromTimeSpan(startTimeSpan, DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .overtime-availability-end-time",
												TimeHelper.TimeOfDayFromTimeSpan(endTimeSpan, DataMaker.Data().MyCulture));
			Browser.Interactions.AssertExistsUsingJQuery(overtimeAvailability.EndTimeNextDay
				                                             ? ".overtime-availability-next-day:checked"
				                                             : ".overtime-availability-next-day:not(:checked)");
		}

		[Then(@"I should see the request form with '(.*)' as default date")]
		public void ThenIShouldSeeTheTextRequestFormWithAsDefaultDate(DateTime date)
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-datefrom", date.ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-dateto", date.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"I should see the detail form for request at position '(.*)' in the list")]
		public void ThenIShouldSeeTheDetailFormForRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format(".request-edit:lt({0})", position));
		}

		[Then(@"I should see the add text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .request-new-subject");
		}
		
		[Then(@"I should see the add absence request form")]
		public void ThenIShouldSeeTheAbsenceRequestForm()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .request-new-absence");
		}

		[Then(@"I should not see the add absence button")]
		public void ThenIShouldNotSeeTheAddAbsenceButton()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".absence-request-add");
		}

		[Then(@"I should not see add overtime availability button")]
		public void ThenIShouldNotSeeAddOvertimeAvailabilityButton()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".overtime-availability-add");
		}

		[Then(@"I should see that request at position '(.*)' in the list was denied with reason '(.*)'")]
		public void ThenIShouldSeeThatRequestAtPositionInTheListWasDeniedWithReason(int position, string reason)
		{
			Browser.Interactions.AssertFirstContains(string.Format(".request:nth-child({0}) .request-denyreason", position), reason);
		}

		[Then(@"I should see request form with subject '(.*)'")]
		public void ThenIShouldSeeRequestFormWithSubject(string subject)
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#Schedule-addRequest-subject-input", subject);
		}

		[When(@"I change absence to '(.*)'")]
		public void WhenIChangeAbsenceTo(string absence)
		{
			Browser.Interactions.ClickContaining(string.Format(".request-body:nth-child() .request-data-type"), absence);
		}

	}
}
