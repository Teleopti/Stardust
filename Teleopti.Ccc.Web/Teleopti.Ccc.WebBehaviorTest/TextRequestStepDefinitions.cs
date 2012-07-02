using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TextRequestStepDefinitions
	{
		[When(@"I input text request values")]
		public void WhenIInputTextRequstValues()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			var date = DateTime.Today;
			var time = date.AddHours(12);
			Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value = "The cake is a.. Cake!";
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value = date.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value = time.ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value = date.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value = time.AddHours(1).ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value = "A message. A very very very short message. Or maybe not.";
		}

		[When(@"I input new text request values")]
		public void WhenIInputNewTextRequestValues()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value = "The cake is a.. cinnemon roll!";
		}


		[Then(@"I should see the text request form with the first day of week as default")]
		public void ThenIShouldSeeTheTextRequestFormWithTheFirstDayOfWeekAsDefault()
		{
			if (Browser.Current.Url.Contains("Requests/Index"))
				ScenarioContext.Current.Pending();
			
			var firstDayOfWeek = Pages.Pages.WeekSchedulePage.FirstDate;

			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value, Is.EqualTo(firstDayOfWeek));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value, Is.EqualTo(firstDayOfWeek));
		}

		[Then(@"I should see the text request's details form")]
		public void ThenIShouldSeeTheTextRequestsDetailsForm()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSection.DisplayVisible(), Is.True);
		}

		[Then(@"I should see the request's values")]
		public void ThenIShouldSeeTheRequestsValues()
		{
			var request= UserFactory.User().UserData<ExistingTextRequest>();

			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), 
																		Is.EqualTo(request.PersonRequest.Request.Period.StartDateTime.Date));
			EventualAssert.That(() => TimeSpan.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.StartDateTime.TimeOfDay));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value,
																		Is.EqualTo(request.PersonRequest.GetMessage(new NoFormatting())));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value,
																		Is.EqualTo(request.PersonRequest.GetSubject(new NoFormatting())));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.EndDateTime.Date));
			EventualAssert.That(() => TimeSpan.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value),
																		Is.EqualTo(request.PersonRequest.Request.Period.EndDateTime.TimeOfDay));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailEntityId.Value, Is.EqualTo(request.PersonRequest.Id.ToString()));
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

		[Then(@"I should see the new text request values in the list")]
		public void ThenIShouldSeeTheNewTextRequestValuesInTheList()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.Requests.Count(), Is.EqualTo(1));
			EventualAssert.That(() => Pages.Pages.RequestsPage.FirstRequest.InnerHtml, Contains.Substring("cinnemon roll"));
		}

		[Then(@"I should see the absence request form with today's date as default")]
		[Then(@"I should see the text request form with today's date as default")]
		public void ThenIShouldSeeTheTextRequestFormWithTodaySDateAsDefault()
		{
			var today = DateTime.Today;

			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value), Is.EqualTo(today));
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value), Is.EqualTo(today));
		}

		[Then(@"I should see (.*) - (.*) as the default times")]
		public void ThenIShouldSee800_1700AsTheDefaultTimes(string startTime, string endTime)
		{
			int[] st = startTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var tstart=new TimeSpan(st[0],st[1],0);
			int[] end = endTime.Split(':').Select(n => Convert.ToInt32(n)).ToArray();
			var tend = new TimeSpan(end[0], end[1], 0);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value, Is.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(tstart, UserFactory.User().Culture)));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value, Is.EqualTo(TimeHelper.TimeOfDayFromTimeSpan(tend, UserFactory.User().Culture)));
		}

		[When(@"I input empty subject")]
		public void WhenIInputEmptySubject()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value = string.Empty;
		}

		[When(@"I input too long message request values")]
		[When(@"I input too long text request values")]
        public void WhenIInputTooLongTextRequestValues()
        {
            Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value = "The cake is a.. Cake!";
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value = DateTime.Today.ToShortDateString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value = DateTime.Now.AddHours(1).ToShortTimeString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value = DateTime.Today.ToShortDateString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value = DateTime.Now.AddHours(2).ToShortTimeString(UserFactory.User().Culture);
            Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value = new string('t', 2002);
        }

		[When(@"I input too long subject request values")]
		public void WhenIInputTooLongSubjectRequestValues()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value = "01234567890123456789012345678901234567890123456789012345678901234567890123456789#";
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value = DateTime.Today.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value = DateTime.Now.AddHours(1).ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value = DateTime.Today.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value = DateTime.Now.AddHours(2).ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value = "A message. A very very very short message. Or maybe not.";
		}


		[When(@"I input later start time than end time")]
		public void WhenIInputLaterStartTimeThanEndTime()
		{
			Pages.Pages.CurrentEditRequestPage.RequestDetailSection.WaitUntilDisplayed();
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value = DateTime.Today.AddDays(1).ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value = DateTime.Today.AddHours(1).ToShortTimeString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value = DateTime.Today.ToShortDateString(UserFactory.User().Culture);
			Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value = DateTime.Today.AddHours(-2).ToShortTimeString(UserFactory.User().Culture);
		}

		[When(@"I click the text request's delete button")]
		public void WhenIClickTheRequestSDeleteButton()
		{
			var requestId = UserFactory.User().UserData<ExistingTextRequest>().PersonRequest.Id.Value;
			Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId).EventualClick();
		}

		[Then(@"I should see texts describing my errors")]
		public void ThenIShouldSeeTextsDescribingMyErrors()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.OuterHtml, Is.StringContaining(string.Format(Resources.InvalidTimeValue, Resources.Period)));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.InnerHtml, Is.StringContaining(string.Format(Resources.InvalidTimeValue, Resources.Period)));
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.InnerHtml, Is.StringContaining(Resources.MissingSubject));
		}

        [Then(@"I should see texts describing too long text error")]
        public void ThenIShouldSeeTextsDescribingTooLongTextError()
        {
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.Exists, Is.True);
            EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.InnerHtml, Is.StringContaining(Resources.MessageTooLong));
        }

		[Then(@"I should see texts describing too long subject error")]
		public void ThenIShouldSeeTextsDescribingTooLongSubjectError()
		{
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.ValidationErrorText.InnerHtml, Is.StringContaining(Resources.TheNameIsTooLong));
		}


		[Then(@"I should not see the add text request button")]
		public void ThenIShouldNotSeeTheAddTextRequestButton()
		{
			if (Browser.Current.Url.Contains("Requests/Index"))
				ScenarioContext.Current.Pending();
			
			EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AddRequestButton.Exists, Is.False);
		}

		[Then(@"I should not see the absence request in the list")]
		[Then(@"I should not see the text request in the list")]
		public void ThenIShouldNotSeeTheTextRequestInTheList()
		{
			var existingTextRequest = UserFactory.User().UserData<ExistingTextRequest>();
			if (existingTextRequest != null)
			{
				var requestId = existingTextRequest.PersonRequest.Id.Value;
				EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId).Exists, Is.False);
				Navigation.GotoRequests();
				EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId).Exists, Is.False);
				return;
			}
			EventualAssert.That(() => Pages.Pages.RequestsPage.Requests.Count(), Is.EqualTo(0));
		}

		[Then(@"I should not see a delete button")]
		public void ThenIShouldNotSeeADeleteButton()
		{
			PersonRequest request = null;
			if (UserFactory.User().HasSetup<ExistingApprovedTextRequest>())
				request = UserFactory.User().UserData<ExistingApprovedTextRequest>().PersonRequest;
			else if (UserFactory.User().HasSetup<ExistingDeniedTextRequest>())
				request = UserFactory.User().UserData<ExistingDeniedTextRequest>().PersonRequest;
			else if (UserFactory.User().HasSetup<ExistingApprovedAbsenceRequest>())
				request = UserFactory.User().UserData<ExistingApprovedAbsenceRequest>().PersonRequest;
			else if (UserFactory.User().HasSetup<ExistingDeniedAbsenceRequest>())
				request = UserFactory.User().UserData<ExistingDeniedAbsenceRequest>().PersonRequest;
			if (request == null)
				ScenarioContext.Current.Pending();
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(request.Id.Value).Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestDeleteButtonById(request.Id.Value).Exists, Is.False);
		}

		[Then(@"I should not see a save button")]
		public void ThenIShouldNotSeeASaveButton()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.OkButton.DisplayVisible(), Is.False);
		}
	}
}
