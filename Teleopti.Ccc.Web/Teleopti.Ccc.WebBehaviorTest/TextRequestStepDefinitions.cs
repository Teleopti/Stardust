using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class TextRequestStepDefinitions
	{
		[When(@"I input text request values")]
		public void WhenIInputTextRequstValues()
		{
			TypeSubject("The cake is a.. Cake!");
			TypeMessage("A message. A very very very short message. Or maybe not.");
			var date = DateTime.Today;
			var time = date.AddHours(12);
			SetValuesForDateAndTime(date, time, date, time.AddHours(1));
		}

		[When(@"I input text request values with subject '(.*)' for date '(.*)'")]
		public void WhenIInputSubject(string subject, DateTime date)
		{
			var time = date.AddHours(12);
			SetValuesForDateAndTimeInSchedule(date, time, date, time.AddHours(1));
			TypeSubject(subject);
			TypeMessage("A message. A very very very short message. Or maybe not.");
		}

        [When(@"I input text request values for date '(.*)'")]
        public void WhenIInputTextRequestValuesForDate(DateTime date)
        {
			TypeSubject("The cake is a.. Cake!");
			TypeMessage("A message. A very very very short message. Or maybe not.");
			
			var time = date.AddHours(12);
			SetValuesForDateAndTimeInSchedule(date, time, date, time.AddHours(1));
            
        }

		[When(@"I input new text request values")]
		public void WhenIInputNewTextRequestValues()
		{
			TypeSubject("The cake is a.. cinnemon roll!");
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

		[Then(@"I should see the new text request values in the list")]
		public void ThenIShouldSeeTheNewTextRequestValuesInTheList()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.Requests.Count(), Is.EqualTo(1));
			EventualAssert.That(() => Pages.Pages.RequestsPage.FirstRequest.InnerHtml, Contains.Substring("cinnemon roll"));
		}

		[Then(@"I should see the request form with today's date as default")]
		public void ThenIShouldSeeTheRequestFormWithTodaySDateAsDefault()
		{
			var today = DateTime.Today;

			Browser.Interactions.AssertContains("#Request-add-section input[data-bind*=datepicker: DateFrom]", today.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertContains("#Request-add-section input[data-bind*=datepicker: DateTo]", today.ToShortDateString(UserFactory.User().Culture));
		}

		[When(@"I input empty subject")]
		public void WhenIInputEmptySubject()
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section input[data-bind='value: Subject']", string.Empty);
		}

		[When(@"I input too long message request values")]
		[When(@"I input too long text request values")]
        public void WhenIInputTooLongTextRequestValues()
        {
			TypeSubject("The cake is a.. Cake!");
			TypeMessage(new string('t', 2002));
			SetValuesForDateAndTime(DateTime.Today, DateTime.Now.AddHours(1), DateTime.Today, DateTime.Today.AddHours(2));
        }

		[When(@"I input too long subject request values")]
		public void WhenIInputTooLongSubjectRequestValues()
		{
			TypeSubject("01234567890123456789012345678901234567890123456789012345678901234567890123456789#");
			TypeMessage("A message. A very very very short message. Or maybe not.");
			SetValuesForDateAndTime(DateTime.Today, DateTime.Now.AddHours(1), DateTime.Today, DateTime.Today.AddHours(2));
		}

		private void TypeSubject(string text)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Schedule-addRequest-subject-input", text);
		}

		private void TypeMessage(string text)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Schedule-addRequest-message-input", text);
		}

		private void SetValuesForDateAndTime(DateTime fromDate, DateTime toDate, DateTime fromTime, DateTime toTime)
		{
			EnableTimePickersByUncheckingFullDayCheckbox();

			Browser.Interactions.Javascript(string.Format("$('#Request-add-section input[data-bind*=\"datepicker: DateFrom\"]').datepicker('set', '{0}');",
							  fromDate.ToShortDateString(UserFactory.User().Culture)));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section input[data-bind*='timepicker: TimeFrom']", fromTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.Javascript(string.Format("$('#Request-add-section input[data-bind*=\"datepicker: DateTo\"]').datepicker('set', '{0}');",
							  toDate.ToShortDateString(UserFactory.User().Culture)));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section input[data-bind*='timepicker: TimeTo']", toTime.ToShortTimeString(UserFactory.User().Culture));
		}

		private void SetValuesForDateAndTimeInSchedule(DateTime fromDate, DateTime toDate, DateTime fromTime, DateTime toTime)
		{
			EnableTimePickersByUncheckingFullDayCheckbox();

			Browser.Interactions.Javascript(string.Format("$('#Schedule-addRequest-fromDate-input').datepicker('set', '{0}');",
							  fromDate.ToShortDateString(UserFactory.User().Culture)));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Schedule-addRequest-fromTime-input-input", fromTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.Javascript(string.Format("$('#Schedule-addRequest-toDate-input').datepicker('set', '{0}');",
							  toDate.ToShortDateString(UserFactory.User().Culture)));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Schedule-addRequest-toTime-input-input", toTime.ToShortTimeString(UserFactory.User().Culture));
		}


		[When(@"I input later start time than end time")]
		public void WhenIInputLaterStartTimeThanEndTime()
		{
			SetValuesForDateAndTime(DateTime.Today.AddDays(1), DateTime.Today, DateTime.Today, DateTime.Today.AddHours(-2));
		}
		
		private void EnableTimePickersByUncheckingFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("$('#Request-add-section input[type=checkbox]:enabled').prop('checked')").ToString() == "true")
				Browser.Interactions.Click("#Request-add-section input[type='checkbox']");
		}

        [When(@"I input later start time than end time for date '(.*)'")]
        public void WhenIInputLaterStartTimeThanEndTimeForDate(DateTime date)
        {
			SetValuesForDateAndTime(date.AddDays(1), date.AddHours(1), date, date.AddHours(-2));
        }

		[When(@"I click the text request's delete button")]
 		public void WhenIClickTheRequestSDeleteButton()
 		{
			PersonRequest requestId = null;
			if (UserFactory.User().HasSetup<ExistingTextRequest>())
				requestId = UserFactory.User().UserData<ExistingTextRequest>().PersonRequest;
			else if (UserFactory.User().HasSetup<ExistingPendingTextRequest>())
				requestId = UserFactory.User().UserData<ExistingPendingTextRequest>().PersonRequest;
			if (requestId == null)
				ScenarioContext.Current.Pending();
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId.Id.Value).Exists, Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId.Id.Value).DisplayVisible(), Is.True);
			Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId.Id.Value).EventualClick();
 		}

		[Then(@"I should see texts describing my errors")]
		public void ThenIShouldSeeTextsDescribingMyErrors()
		{
			Browser.Interactions.AssertContains("#Request-add-section .alert-danger", string.Format(Resources.InvalidTimeValue, Resources.Period));
			Browser.Interactions.AssertContains("#Request-add-section .alert-danger", Resources.MissingSubject);
		}

        [Then(@"I should see texts describing too long text error")]
        public void ThenIShouldSeeTextsDescribingTooLongTextError()
        {
			Browser.Interactions.AssertContains("#Request-add-section .alert-danger", Resources.MessageTooLong);
        }

		[Then(@"I should see texts describing too long subject error")]
		public void ThenIShouldSeeTextsDescribingTooLongSubjectError()
		{
			Browser.Interactions.AssertContains("#Request-add-section .alert-danger", Resources.TheNameIsTooLong);
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
			Browser.Interactions.AssertNotExists(".bdd-add-text-request-link", ".bdd-request-body .close");
		}

		[Then(@"I should not see a save button")]
		public void ThenIShouldNotSeeASaveButton()
		{
			Browser.Interactions.AssertNotExists("#Requests-body-inner", ".bdd-request-edit-detail:nth-of-type(1) button[data-bind*=click: AddRequest]");
		}
	}
}
