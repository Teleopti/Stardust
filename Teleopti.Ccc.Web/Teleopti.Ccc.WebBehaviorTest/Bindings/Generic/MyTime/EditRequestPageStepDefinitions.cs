using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
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
			Browser.Interactions.AssertNotExists(".bdd-add-text-request-link", ".bdd-add-absence-request-link");
		}

		[Then(@"I should not see the New Shift Trade Request menu item")]
		public void ThenIShouldNotSeeTheNewShiftTradeRequestMenuItem()
		{
			Browser.Interactions.AssertNotExists(".bdd-add-text-request-link", ".bdd-add-shifttrade-request-link");
		}

		[When(@"I click to add a new absence request")]
		public void WhenIClickToAddANewAbsenceRequest()
		{
			Browser.Interactions.Click(".bdd-add-absence-request-link");
			Browser.Interactions.Click(".bdd-add-absence-request-link");
			Browser.Interactions.AssertExists("#Request-add-section");
		}

		[When(@"I unchecked the full day checkbox")]
		public void WhenIUncheckedTheFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("$('#Request-add-section input[type=checkbox]:enabled').prop('checked')").ToString() == "true")
				Browser.Interactions.Click("#Request-add-section input[type='checkbox']");
		}

		[When(@"I checked the full day checkbox")]
		public void WhenIClickFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("$('#Request-add-section input[type=checkbox]:enabled').prop('checked')").ToString() == "false")
				Browser.Interactions.Click("#Request-add-section input[type='checkbox']");
		}



		[Then(@"Subject should not be empty")]
		public void SubjectShouldNotBeEmpty()
		{

			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value.Trim(), Is.Not.Empty);
		}

		[Then(@"I should not be able to input values")]
		public void ThenIShouldNotBeAbleToInputValues()
		{
			const string disabledAttr = "disabled";
			const string readonlyAttr = "readonly";
			var detailForm = Pages.Pages.CurrentEditRequestPage;
			EventualAssert.That(() => detailForm.RequestDetailFromDateTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "TextRequestDetailFromDateInput");
			EventualAssert.That(() => detailForm.RequestDetailFromTimeTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailFromTimeTextField");
			EventualAssert.That(() => detailForm.RequestDetailSubjectInput.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailSubjectInput");
			EventualAssert.That(() => detailForm.RequestDetailToDateTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailToDateTextField");
			EventualAssert.That(() => detailForm.RequestDetailToTimeTextField.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "RequestDetailToTimeTextField");
			EventualAssert.That(() => detailForm.RequestDetailMessageTextField.GetAttributeValue(readonlyAttr), Is.EqualTo("True"), "RequestDetailMessageTextField");
			EventualAssert.That(() => detailForm.FulldayCheck.GetAttributeValue(disabledAttr), Is.EqualTo("True"), "FulldayCheck");
		}

		[Then(@"I should see (.*) - (.*) as the default times")]
		public void ThenIShouldSee800_1700AsTheDefaultTimes(string startTime, string endTime)
		{
			int[] st = startTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var startTimeSpan = new TimeSpan(st[0], st[1], 0);
			int[] end = endTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var endTimeSpan = new TimeSpan(end[0], end[1], 0);

			Browser.Interactions.AssertContains("#Request-add-section input[data-bind*=timepicker: TimeFrom]",
			                                    TimeHelper.TimeOfDayFromTimeSpan(startTimeSpan, UserFactory.User().Culture));
			Browser.Interactions.AssertContains("#Request-add-section input[data-bind*=timepicker: TimeTo]",
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
			Browser.Interactions.AssertElementsAreVisible(string.Format(".bdd-request-edit-detail:nth-of-type({0})", position));
		}

		[Then(@"I should see the add text request form")]
		public void ThenIShouldSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestTab.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestTab.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestTab.JQueryVisible(), Is.True);
		}

		[Then(@"I should not see the add text request form")]
		public void ThenIShouldNotSeeTheTextRequestForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.TextRequestTab.Exists, Is.False);
		}

		[Then(@"I should see the add absence request form")]
		public void ThenIShouldSeeTheAbsenceRequestTab()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.JQueryVisible(), Is.True);
		}

		[Then(@"I should not see the add absence request form")]
		public void ThenIShouldNotSeeTheAbsenceRequestTab()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.Exists, Is.False);
		}

		[Then(@"I should not see the deny reason")]
		public void ThenIShouldNotSeeTheDenyReason()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailDenyReason.Text, Is.Null);
		}

		[Then(@"I should see that my request was denied with reason '(.*)'")]
		public void ThenIShouldSeeThatMyRequestWasDeniedWithGivenReason(string reason)
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailDenyReason.Text,
															Is.EqualTo(reason));
		}

		[Then(@"I should see request form with subject '(.*)'")]
		public void ThenIShouldSeeRequestFormWithSubject(string subject)
		{
			Browser.Interactions.AssertInputValue("#Schedule-addRequest-subject-input", subject);
		}
	}
}
