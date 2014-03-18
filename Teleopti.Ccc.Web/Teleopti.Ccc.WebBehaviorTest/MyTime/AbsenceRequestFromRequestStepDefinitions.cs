using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class AbsenceRequestFromRequestStepDefinitions 
	{
		[Given(@"I have an approved absence request")]
		public void GivenIHaveAnApprovedAbsenceRequest()
		{
			DataMaker.Data().Apply(new ExistingApprovedAbsenceRequest());
		}

		[Given(@"I have a denied absence request")]
		public void GivenIHaveADeniedAbsenceRequest()
		{
			DataMaker.Data().Apply(new ExistingDeniedAbsenceRequest());
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
			var request = DataMaker.Data().UserData<ExistingAbsenceRequest>();

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
				request.PersonRequest.Request.Period.StartDateTime.Date.ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.StartDateTime.ToShortTimeString(DataMaker.Data().MyCulture));

			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.EndDateTime.Date.ToShortDateString(DataMaker.Data().MyCulture));
			Browser.Interactions.AssertFirstContains(
				string.Format(".request-body:nth-child({0}) .request-data-date", position),
				request.PersonRequest.Request.Period.EndDateTime.ToShortTimeString(DataMaker.Data().MyCulture));
		}

		[Given(@"I have a denied absence request beacuse of missing workflow control set")]
		public void GivenIHaveADeniedAbsenceRequestBeacuseOfMissingWorkflowControlSet()
		{
			DataMaker.Data().Apply(new ExistingDeniedAbsenceRequest("RequestDenyReasonNoWorkflow"));
		}
	}
}
