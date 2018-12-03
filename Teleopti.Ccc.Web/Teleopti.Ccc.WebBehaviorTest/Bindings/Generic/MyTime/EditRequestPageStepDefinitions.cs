using System;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.MyTime;

using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;

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

		[Then(@"I should not be able to make a new shift trade request")]
		public void ThenIShouldNotBeAbleToMakeANewShiftTradeRequest()
		{
			Browser.Interactions.AssertNotExists(".text-request-add", ".shifttrade-request-add");
		}

		[When(@"I click to add a new text request")]
		public void WhenIClickToAddANewTextRequest()
		{
			Browser.Interactions.Click("#addTextRequest");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[Given(@"I click to add a new absence request")]
		[When(@"I click to add a new absence request")]
		public void WhenIClickToAddANewAbsenceRequest()
		{
			Browser.Interactions.Click("#addAbsenceRequest");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I cancel to add absence request")]
		public void WhenICancelToAddAbsenceRequest()
		{
			Browser.Interactions.Click(".request-new-cancel");
		}

		[When(@"I click to shift trade bulletin board")]
		public void WhenIClickToShiftTradeBulletinBoard()
		{
			Browser.Interactions.Click("#addShiftTradeRequestFromBulletinBoard");
			Browser.Interactions.AssertExists("#Request-shift-trade-bulletin-board");
		}

		[When(@"I click overtime availability")]
		[When(@"I click add new overtime availability")]
		public void WhenIClickAddNewOvertimeAvailability()
		{
			Browser.Interactions.Click(".overtime-availability-add");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I add new shift exchange offer for current day")]
		public void WhenIClickAddNewShiftExchangeOffer()
		{
			Browser.Interactions.Click(".shift-exchange-offer-add");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I am looking for an empty day")]
		public void WhenIAmLookingForAnEmptyDay()
		{
			Browser.Interactions.AssertNotExistsUsingJQuery("select#wish-list option:first", "#ELEMENT_NOT_EXISTS");
			Browser.Interactions.Javascript_IsFlaky("$('select#wish-list option:last').prop('selected', 'selected');");
			Browser.Interactions.Javascript_IsFlaky("$('select#wish-list').change();");						
		}

		[Then(@"no shift detail is needed")]
		public void ThenNoShiftDetailIsNeeded()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery( ".shift-exchange-offer-start-time");
		}

		[When(@"I unchecked the full day checkbox")]
		public void WhenIUncheckedTheFullDayCheckbox()
		{
			TextRequestStepDefinitions.UncheckFullDayCheckbox();
		}

		[When(@"I checked the full day checkbox")]
		public void WhenIClickFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript_IsFlaky("return $('#Request-add-section .request-new-fullday:enabled').prop('checked')").ToString() == "false")
				Browser.Interactions.Click("#Request-add-section .request-new-fullday");
		}

		[Then(@"I should not be able to edit the values for the existing text request")]
		public void ThenIShouldNotBeAbleToEditTheValuesForTheExistingTextRequest()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".request .request-edit-subject");

			Browser.Interactions.AssertExists(".request .request-edit-datefrom:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-timefrom:not(:enabled)");

			Browser.Interactions.AssertExists(".request .request-edit-dateto:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-timeto:not(:enabled)");

			Browser.Interactions.AssertExists(".request .request-edit-fullday:not(:enabled)");
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


		[Then(@"I should see the values of the shift trade post")]
		[Then(@"I should see add shift exchange offer form with")]
		[Then(@"I should see the updated values of the shift trade post")]
		public void ThenIShouldSeeAddShiftExchangeOfferFormWith(Table table)
		{
			var exchangeOffer = table.CreateInstance<ShiftExchangeOfferFields>();
			Browser.Interactions.AssertInputValueUsingJQuery(".shift-exchange-offer-end-date",
															 exchangeOffer.OfferEndDate.ToShortDateString(
																 DataMaker.Data().MyCulture));

			var st = exchangeOffer.StartTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var startTimeSpan = new TimeSpan(st[0], st[1], 0);
			var end = exchangeOffer.EndTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var endTimeSpan = new TimeSpan(end[0], end[1], 0);

			Browser.Interactions.AssertInputValueUsingJQuery(".shift-exchange-offer-start-time",
												TimeHelper.TimeOfDayFromTimeSpan(startTimeSpan, DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery(".shift-exchange-offer-end-time",
												TimeHelper.TimeOfDayFromTimeSpan(endTimeSpan, DataMaker.Data().MyCulture));
			Browser.Interactions.AssertExistsUsingJQuery(exchangeOffer.EndTimeNextDay
															 ? ".shift-exchange-offer-next-day:checked"
															 : ".shift-exchange-offer-next-day:not(:checked)");
		}

		[Then(@"I should see the overtime availability form with a start date of '(.*)' and an end date of '(.*)'")]
		public void ThenIShouldSeeTheOvertimeAvailabiltyFormWithAsDefaultDate(string formattedStartDate, string formattedEndDate)
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .overtime-availability-start-date", formattedStartDate);
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .overtime-availability-end-date", formattedEndDate);
		}


		[Then(@"I should see the request form with '(.*)' as default date")]
		public void ThenIShouldSeeTheTextRequestFormWithAsDefaultDate(DateTime date)
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-datefrom", date.ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-dateto", date.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"I should see the detail form for the existing request in the list")]
		public void ThenIShouldSeeTheDetailFormForTheExistingRequestInTheList()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".request-edit");
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

		[Then(@"I should see that the request was denied with reason '(.*)'")]
		public void ThenIShouldSeeThatTheRequestWasDeniedWithReason(string reason)
		{
			Browser.Interactions.AssertFirstContains(".request .request-denyreason", reason);
		}

		[Then(@"I should see request form with subject '(.*)'")]
		public void ThenIShouldSeeRequestFormWithSubject(string subject)
		{
			Browser.Interactions.AssertInputValueUsingJQuery("#Schedule-addRequest-subject-input", subject);
		}

		[When(@"I delete the existing request in the list")]
		public void WhenIDeleteTheExistingRequestInTheList()
		{
			Browser.Interactions.Click(".request-list .request .request-delete");
			Browser.Interactions.Click(".request-list .request #request-list-delete");
		}
	}
}
