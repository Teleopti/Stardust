using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class AbsenceRequestFromRequestStepDefinitions 
	{
		[Given(@"I have an approved absence request")]
		public void GivenIHaveAnApprovedAbsenceRequest()
		{
			UserFactory.User().Setup(new ExistingApprovedAbsenceRequest());
		}

		[Given(@"I have a denied absence request")]
		public void GivenIHaveADeniedAbsenceRequest()
		{
			UserFactory.User().Setup(new ExistingDeniedAbsenceRequest());
		}

		[When(@"I click the absence request's delete button for request at position '(.*)' in the list")]
		public void WhenIClickTheAbsenceRequestSDeleteButtonForRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.Click(string.Format(".bdd-request-body:nth-of-type({0}) button .close", position));
		}

		[When(@"I change the absence request values with")]
		public void WhenIChangeTheAbsenceRequestValuesWith(Table table)
		{
			var absence = table.Rows[1][1];
			var subject = table.Rows[2][1];
			var position = table.Rows[0][1];

			Browser.Interactions.SelectOptionByTextUsingJQuery(
				string.Format(".bdd-request-edit-detail:nth-of-type({0}) select[data-bind='value: AbsenceId']", position), absence);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(
				string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[data-bind='value: Subject']", position), subject);
		}

		[Then(@"I should see the updated request values in the list with")]
		public void ThenIShouldSeeTheUpdatedRequestValuesInTheListWith(Table table)
		{
			var absence = table.Rows[1][1];
			var subject = table.Rows[2][1];
			var position = table.Rows[0][1];

			Browser.Interactions.AssertNotExists(string.Format(".bdd-request-body:nth-of-type({0})", position), string.Format(".bdd-request-body:nth-of-type({0})", position + 1));
			Browser.Interactions.AssertContains(string.Format(".bdd-request-body:nth-of-type({0}) .request-data-subject", position), subject);
			Browser.Interactions.AssertContains(string.Format(".bdd-request-body:nth-of-type({0}) .request-data-type", position), absence);
		}

		[Then(@"I should not be able to input values for absence request for request at position '(.*)' in the list")]
		public void ThenIShouldNotBeAbleToInputValuesForAbsenceRequestForRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[data-bind*=\"value: Subject\"][disabled]", position));
			Browser.Interactions.AssertExists(".bdd-request-edit-detail textarea[data-bind*=\"value: Message\"][disabled]");

			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) select[disabled]", position));

			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[data-bind*=\"datepicker: DateFrom\"][disabled]", position));
			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[data-bind*=\"timepicker: TimeFrom\"][disabled]", position));

			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[data-bind*=\"datepicker: DateTo\"][disabled]", position));
			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[data-bind*=\"timepicker: TimeTo\"][disabled]", position));

			Browser.Interactions.AssertExists(string.Format(".bdd-request-edit-detail:nth-of-type({0}) input[type=\"checkbox\"][disabled]", position));
		}

		[Then(@"I should see the absence request containing '(.*)' at position '(.*)' in the list")]
		public void ThenIShouldSeeTheAbsenceRequestContainingAtPositionInTheList(string absence, int position)
		{
			Browser.Interactions.AssertExists(".bdd-request-body");
			Browser.Interactions.AssertContains(string.Format(".bdd-request-body:nth-of-type({0}) .request-data-type", position), absence);
		}

		[Then(@"I should see the absence request's values at position '(.*)' in the list")]
		public void ThenIShouldSeeTheAbsenceRequestSValuesAtPositionInTheList(int position)
		{
			var request = UserFactory.User().UserData<ExistingAbsenceRequest>();

			//EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromDateTextField.Value),
			//															Is.EqualTo(request.PersonRequest.Request.Period.StartDateTime.Date));
			//EventualAssert.That(() => TimeSpan.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailFromTimeTextField.Value),
			//															Is.EqualTo(request.PersonRequest.Request.Period.StartDateTime.TimeOfDay));
			//EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailMessageTextField.Value,
			//															Is.EqualTo(request.PersonRequest.GetMessage(new NoFormatting())));

			//EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.AbsenceTypesTextField.Value,
			//															Is.EqualTo(request.AbsenceRequest.Absence.Description.Name));

			//EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailSubjectInput.Value,
			//															Is.EqualTo(request.PersonRequest.GetSubject(new NoFormatting())));
			//EventualAssert.That(() => DateTime.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToDateTextField.Value),
			//															Is.EqualTo(request.PersonRequest.Request.Period.EndDateTime.Date));
			//EventualAssert.That(() => TimeSpan.Parse(Pages.Pages.CurrentEditRequestPage.RequestDetailToTimeTextField.Value),
			//															Is.EqualTo(request.PersonRequest.Request.Period.EndDateTime.TimeOfDay));
			//EventualAssert.That(() => Pages.Pages.CurrentEditRequestPage.RequestDetailEntityId.Value, Is.EqualTo(request.PersonRequest.Id.ToString()));

			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-subject", position),
				request.PersonRequest.GetSubject(new NoFormatting()));
			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-subject", position),
				request.PersonRequest.GetMessage(new NoFormatting()));

			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-type", position),
				request.AbsenceRequest.Absence.Description.Name);
			
			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.StartDateTime.Date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.StartDateTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.EndDateTime.Date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertContains(
				string.Format(".bdd-request-body:nth-of-type({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.EndDateTime.ToShortTimeString(UserFactory.User().Culture));


		}

		[Given(@"I have a denied absence request beacuse of missing workflow control set")]
		public void GivenIHaveADeniedAbsenceRequestBeacuseOfMissingWorkflowControlSet()
		{
			UserFactory.User().Setup(new ExistingDeniedAbsenceRequest("RequestDenyReasonNoWorkflow"));
		}
	}
}
