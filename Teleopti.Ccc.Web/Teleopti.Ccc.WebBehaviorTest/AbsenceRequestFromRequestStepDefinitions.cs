using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
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
			Browser.Interactions.Click(string.Format(".request-body:nth-child({0}) .request-delete", position));
		}

		[When(@"I change the absence request values with")]
		public void WhenIChangeTheAbsenceRequestValuesWith(Table table)
		{
			var absence = table.Rows[1][1];
			var subject = table.Rows[2][1];
			var position = table.Rows[0][1];

			Browser.Interactions.SelectOptionByTextUsingJQuery(
				string.Format(".request:nth-child({0}) .request-edit-absence", position), absence);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(
				string.Format(".request:nth-child({0}) .request-edit-subject", position), subject);
		}

		[Then(@"I should see the updated absence request values in the list with")]
		public void ThenIShouldSeeTheUpdatedAbsenceRequestValuesInTheListWith(Table table)
		{
			var absence = table.Rows[1][1];
			var subject = table.Rows[2][1];
			var position = table.Rows[0][1];

			Browser.Interactions.AssertNotExists(string.Format(".request-body:nth-child({0})", position), string.Format(".request-body:nth-child({0})", position + 1));
			Browser.Interactions.AssertFirstContains(string.Format(".request-body:nth-child({0}) .request-data-subject", position), subject);
			Browser.Interactions.AssertFirstContains(string.Format(".request-body:nth-child({0}) .request-data-type", position), absence);
		}

		[Then(@"I should not be able to input values for absence request at position '(.*)' in the list")]
		public void ThenIShouldNotBeAbleToInputValuesForAbsenceRequestAtPositionInTheList(int position)
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format(".request:nth-child({0}) .request-edit-subject", position));

			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-absence:not(:enabled)", position));

			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-datefrom:not(:enabled)", position));
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-timefrom:not(:enabled)", position));

			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-dateto:not(:enabled)", position));
			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-timeto:not(:enabled)", position));

			Browser.Interactions.AssertExists(string.Format(".request:nth-child({0}) .request-edit-fullday:not(:enabled)", position));
		}

		[Then(@"I should see the absence request containing '(.*)' at position '(.*)' in the list")]
		public void ThenIShouldSeeTheAbsenceRequestContainingAtPositionInTheList(string absence, int position)
		{
			Browser.Interactions.AssertExists(".request-body");
			Browser.Interactions.AssertFirstContains(string.Format(".request-body:nth-child({0}) .request-data-type", position), absence);
		}

		[Then(@"I should see the absence request's values at position '(.*)' in the list")]
		public void ThenIShouldSeeTheAbsenceRequestSValuesAtPositionInTheList(int position)
		{
			var request = UserFactory.User().UserData<ExistingAbsenceRequest>();

			var firstFiftyCharsOfMessage = request.PersonRequest.GetMessage(new NoFormatting()).Substring(0, 50);

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-subject", position),
				request.PersonRequest.GetSubject(new NoFormatting()));
			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-message", position),
				firstFiftyCharsOfMessage);

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-type", position),
				request.AbsenceRequest.Absence.Description.Name);
			
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

		[Then(@"I should see the absence request's edit values at position '(.*)' in the list")]
		public void ThenIShouldSeeTheAbsenceRequestSEditValuesAtPositionInTheList(int position)
		{
			var request = UserFactory.User().UserData<ExistingAbsenceRequest>();

			Browser.Interactions.AssertInputValueUsingJQuery(
				string.Format(".request-list .request:nth-child({0}) .request-edit-subject", position),
				request.PersonRequest.GetSubject(new NoFormatting()));
			Browser.Interactions.AssertInputValueUsingJQuery(
				string.Format(".request-list .request:nth-child({0}) .request-edit-message", position),
				request.PersonRequest.GetMessage(new NoFormatting()));

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-list .request:nth-child({0}) .request-edit-absence option:checked", position),
				request.AbsenceRequest.Absence.Description.Name);

			Browser.Interactions.AssertInputValueUsingJQuery(
				string.Format(".request-list .request:nth-child({0}) .request-edit-datefrom", position),
				request.PersonRequest.Request.Period.StartDateTime.Date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery(
				string.Format(".request-list .request:nth-child({0}) .request-edit-timefrom", position),
				request.PersonRequest.Request.Period.StartDateTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.AssertInputValueUsingJQuery(
				string.Format(".request-list .request:nth-child({0}) .request-edit-dateto", position),
				request.PersonRequest.Request.Period.EndDateTime.Date.ToShortDateString(UserFactory.User().Culture));
			Browser.Interactions.AssertInputValueUsingJQuery(
				string.Format(".request-list .request:nth-child({0}) .request-edit-timeto", position),
				request.PersonRequest.Request.Period.EndDateTime.ToShortTimeString(UserFactory.User().Culture));

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-list .request:nth-child({0}) .request-edit-fullday checkbox:checked", position),
				request.AbsenceRequest.Absence.Description.Name);
		}

		[Given(@"I have a denied absence request beacuse of missing workflow control set")]
		public void GivenIHaveADeniedAbsenceRequestBeacuseOfMissingWorkflowControlSet()
		{
			UserFactory.User().Setup(new ExistingDeniedAbsenceRequest("RequestDenyReasonNoWorkflow"));
		}
	}
}
