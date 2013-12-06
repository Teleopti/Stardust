using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
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
			var date = DateOnlyForBehaviorTests.TestToday.Date;
			var time = date.AddHours(12);
			SetValuesForDateAndTime(date, time, date, time.AddHours(1));
		}
		
		[When(@"I input text request values with subject '(.*)' for date '(.*)'")]
		public void WhenIInputSubject(string subject, DateTime date)
		{
			var time = date.AddHours(12);
			SetValuesForDateAndTime(date, time, date, time.AddHours(1));
			TypeSubject(subject);
			TypeMessage("A message. A very very very short message. Or maybe not.");
		}

        [When(@"I input text request values for date '(.*)'")]
        public void WhenIInputTextRequestValuesForDate(DateTime date)
        {
			TypeSubject("The cake is a.. Cake!");
			TypeMessage("A message. A very very very short message. Or maybe not.");
			
			var time = date.AddHours(12);
			SetValuesForDateAndTime(date, time, date, time.AddHours(1));
            
        }

		[When(@"I change the text request values with")]
		public void WhenIChangeTheTextRequestValuesWith(Table table)
		{
			var subject = table.Rows[1][1];
			var position = table.Rows[0][1];

			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(
				string.Format(".request:nth-child({0}) .request-edit-subject", position), subject);
		}

		[Then(@"I should see the updated text request values in the list with")]
		public void ThenIShouldSeeTheUpdatedTextRequestValuesInTheListWith(Table table)
		{
			var subject = table.Rows[1][1];
			var position = table.Rows[0][1];

			Browser.Interactions.AssertNotExists(string.Format(".request-body:nth-child({0})", position), string.Format(".request-body:nth-child({0})", position + 1));
			Browser.Interactions.AssertFirstContains(string.Format(".request-body:nth-child({0}) .request-data-subject", position), subject);
		}

		[When(@"I click send request button")]
		public void WhenIClickSendRequestButton()
		{
			Browser.Interactions.Click(".request-new-send");
		}
		
		[Then(@"I should see the text request's values at position '(.*)' in the list")]
		public void ThenIShouldSeeTheTextRequestSValuesAtPositionInTheList(int position)
		{
			var request = UserFactory.User().UserData<ExistingTextRequest>();

			var firstFiftyCharsOfMessage = request.PersonRequest.GetMessage(new NoFormatting()).Substring(0,50);

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-subject", position),
				request.PersonRequest.GetSubject(new NoFormatting()));
			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-message", position),
				firstFiftyCharsOfMessage);

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.StartDateTime.Date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.StartDateTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.EndDateTime.Date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.EndDateTime.ToShortTimeString(UserFactory.User().Culture));
		}

		[Then(@"I should see the request form with today's date as default")]
		public void ThenIShouldSeeTheRequestFormWithTodaySDateAsDefault()
		{
			var today = DateOnlyForBehaviorTests.TestToday.Date;

			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-datefrom", today.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-dateto", today.ToShortDateString(UserFactory.User().Culture));
		}

		[When(@"I input empty subject")]
		public void WhenIInputEmptySubject()
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-subject", string.Empty);
		}

		[When(@"I try to input too long message request values")]
		[When(@"I input too long text request values")]
        public void WhenIInputTooLongTextRequestValues()
        {
			TypeSubject("The cake is a.. Cake!");
			TypeMessage(new string('t', 2002));
            SetValuesForDateAndTime(DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date.AddHours(2));
        }

		[When(@"I input too long subject request values")]
		public void WhenIInputTooLongSubjectRequestValues()
		{
			TypeSubject("01234567890123456789012345678901234567890123456789012345678901234567890123456789#");
			TypeMessage("A message. A very very very short message. Or maybe not.");
            SetValuesForDateAndTime(DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date.AddHours(2));
		}

		private void TypeSubject(string text)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".request-new-subject", text);
		}

		private void TypeMessage(string text)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".request-new-message", text);
		}

		private void SetValuesForDateAndTime(DateTime fromDate, DateTime toDate, DateTime fromTime, DateTime toTime)
		{
			EnableTimePickersByUncheckingFullDayCheckbox();

			Browser.Interactions.Javascript(string.Format("$('#Request-add-section .request-new-datefrom').datepicker('set', '{0}');",
							  fromDate.ToShortDateString(UserFactory.User().Culture)));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-timefrom", fromTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.Javascript(string.Format("$('#Request-add-section .request-new-dateto').datepicker('set', '{0}');",
							  toDate.ToShortDateString(UserFactory.User().Culture)));
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-section .request-new-timeto", toTime.ToShortTimeString(UserFactory.User().Culture));
		}

		[When(@"I input later start time than end time")]
		public void WhenIInputLaterStartTimeThanEndTime()
		{
			SetValuesForDateAndTime(DateOnlyForBehaviorTests.TestToday.Date.AddDays(1), DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date, DateOnlyForBehaviorTests.TestToday.Date.AddHours(-2));
		}
		
		private void EnableTimePickersByUncheckingFullDayCheckbox()
		{
			if (Browser.Interactions.Javascript("return $('#Request-add-section .request-new-fullday:enabled').prop('checked');").ToString() == "true")
				Browser.Interactions.Click("#Request-add-section .request-new-fullday");
		}

        [When(@"I input later start time than end time for date '(.*)'")]
        public void WhenIInputLaterStartTimeThanEndTimeForDate(DateTime date)
        {
			SetValuesForDateAndTime(date.AddDays(1), date.AddHours(1), date, date.AddHours(-2));
        }

		[When(@"I click the delete button of request at position '(.*)' in the list")]
		public void WhenIClickTheDeleteButtonOfRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.Click(string.Format(".request-list .request:nth-child({0}) .request-delete", position));
		}

		[Then(@"I should see texts describing my errors")]
		public void ThenIShouldSeeTextsDescribingMyErrors()
		{
			Browser.Interactions.AssertFirstContains("#Request-add-section .request-new-error", string.Format(Resources.InvalidTimeValue, Resources.Period));
			Browser.Interactions.AssertFirstContains("#Request-add-section .request-new-error", Resources.MissingSubject);
		}

		[Then(@"I should see message adjusted to maximum length")]
        public void ThenIShouldSeeTextsDescribingTooLongTextError()
        {
			Browser.Interactions.AssertInputValueUsingJQuery("#Request-add-section .request-new-message", new string('t', 2000));
        }

		[Then(@"I should see texts describing too long subject error")]
		public void ThenIShouldSeeTextsDescribingTooLongSubjectError()
		{
			Browser.Interactions.AssertFirstContains("#Request-add-section .request-new-error", Resources.SubjectTooLong);
		}

		[Then(@"I should not see any requests in the list")]
		public void ThenIShouldNotSeeAnyRequestsInTheList()
		{
			Browser.Interactions.AssertNotExists(".request-list", ".request-list .request");
		}


		[Then(@"I should not see a delete button for request at position '(.*)' in the list")]
		public void ThenIShouldNotSeeADeleteButtonForRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertNotExists(".request-list", string.Format(".request-list .request-body:nth-child({0}) .request-delete", position));
		}

		[Then(@"I should not see a save button for request at position '(.*)' in the list")]
		public void ThenIShouldNotSeeASaveButtonForRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertNotExists("#Requests-body-inner",
			                                     string.Format(
				                                     ".request-list .request-edit:nth-child({0}) .request-edit-update", position));
		}

		[When(@"I click the cancel button")]
		public void WhenIClickTheCancelButton()
		{
			Browser.Interactions.Click(".request-new-cancel");
		}

		[Then(@"the add request form should be closed")]
		[Then(@"I should not see the add text request form")]
		public void ThenTheAddRequestFormShouldBeClosed()
		{
			Browser.Interactions.AssertNotExists("ul.nav li a[href='#ScheduleTab']", "#Request-add-section");
		}
	}
}
