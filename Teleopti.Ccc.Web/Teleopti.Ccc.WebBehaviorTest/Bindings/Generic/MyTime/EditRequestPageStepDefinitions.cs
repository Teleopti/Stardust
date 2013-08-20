﻿using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
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

		[When(@"I click to add a new absence request")]
		public void WhenIClickToAddANewAbsenceRequest()
		{
			Browser.Interactions.Click(".absence-request-add");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I unchecked the full day checkbox")]
		public void WhenIUncheckedTheFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("return $('#Request-add-section .request-new-fullday:enabled').prop('checked')").ToString() == "true")
				Browser.Interactions.Click("#Request-add-section .request-new-fullday");
		}

		[When(@"I checked the full day checkbox")]
		public void WhenIClickFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("return $('#Request-add-section .request-new-fullday:enabled').prop('checked')").ToString() == "false")
				Browser.Interactions.Click("#Request-add-section .request-new-fullday");
		}

		[Then(@"Subject should not be empty")]
		public void SubjectShouldNotBeEmpty()
		{

			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value.Trim(), Is.Not.Empty);
		}

		[Then(@"I should not be able to input values for text request at position '(.*)' in the list")]
		public void ThenIShouldNotBeAbleToInputValuesForTextRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-subject:not(:enabled)", position));
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-message:not(:enabled)", position));

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
												TimeHelper.TimeOfDayFromTimeSpan(startTimeSpan, UserFactory.User().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-timeto",
												TimeHelper.TimeOfDayFromTimeSpan(endTimeSpan, UserFactory.User().Culture));
		}

		[Then(@"I should see the request form with '(.*)' as default date")]
		public void ThenIShouldSeeTheTextRequestFormWithAsDefaultDate(DateTime date)
		{
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), Is.EqualTo(date));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value), Is.EqualTo(date));
		}

		[Then(@"I should see the detail form for request at position '(.*)' in the list")]
		public void ThenIShouldSeeTheDetailFormForRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format(".request-edit:nth-child({0})", position));
		}

		[Then(@"I should see the add text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .request-new-subject");
		}

		[Then(@"I should not see the add text request form")]
		public void ThenIShouldNotSeeTheTextRequestForm()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .request-new-subject");
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Request-add-section .request-new-absence");
		}

		[Then(@"I should see the add absence request form")]
		public void ThenIShouldSeeTheAbsenceRequestTab()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-add-section .request-new-absence");
		}

		[Then(@"I should not see the add absence request form")]
		public void ThenIShouldNotSeeTheAbsenceRequestTab()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Request-add-section .request-new-absence");
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
	}
}
