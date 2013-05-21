using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
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
			Browser.Interactions.AssertNotExists("#request-list", "#Requests-addAbsenceRequest-menuItem");
		}

		[Then(@"I should not see the New Shift Trade Request menu item")]
		public void ThenIShouldNotSeeTheNewShiftTradeRequestMenuItem()
		{
			Browser.Interactions.AssertNotExists("#request-list", "#Requests-addShiftTradeRequest-menuItem");
		}

		[When(@"I click new text request menu item in the toolbar")]
		public void WhenIClickNewTextRequestMenuItemInTheToolbar()
		{
			Pages.Pages.CurrentEditRequestPage.AddRequestDropDown.EventualClick();
			Pages.Pages.CurrentEditRequestPage.AddTextRequestMenuItem.EventualClick();
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
		}

		[When(@"I click to add a new absence request")]
		public void WhenIClickToAddANewAbsenceRequest()
		{
			//Pages.Pages.CurrentEditRequestPage.AddRequestDropDown.EventualClick();
			//Pages.Pages.CurrentEditRequestPage.AddAbsenceRequestMenuItem.EventualClick();
			//Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Browser.Interactions.Click(".bdd-add-absence-request-link");
			Browser.Interactions.Click(".bdd-add-absence-request-link");
			Browser.Interactions.AssertExists("#Request-detail-section");
		}

		[When(@"I click absence request tab")]
		public void WhenIClickAbsenceRequestTab()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.AbsenceRequestTab.EventualClick();
		}

		[When(@"I unchecked the full day checkbox")]
		public void WhenIUncheckedTheFullDayCheckbox()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.FulldayCheck.Checked = false;
		}

		[When(@"I checked the full day checkbox")]
		public void WhenIClickFullDayCheckbox()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.FulldayCheck.Checked = true;
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
			var tstart = new TimeSpan(st[0], st[1], 0);
			int[] end = endTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var tend = new TimeSpan(end[0], end[1], 0);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value, Is.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(tstart, UserFactory.User().Culture)));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value, Is.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(tend, UserFactory.User().Culture)));
		}

		[Then(@"I should see the request form with '(.*)' as default date")]
		public void ThenIShouldSeeTheTextRequestFormWithAsDefaultDate(DateTime date)
		{
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), Is.EqualTo(date));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value), Is.EqualTo(date));
		}




		[Then(@"I should see the edit text request form")]
		[Then(@"I should see the edit absence request form")]
		public void ThenIShouldSeeTheRequestsDetailsForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.JQueryVisible(), Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
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
	}
}
