using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

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
			DataMaker.Data().Apply(new ExistingDeniedAbsenceRequest(null, false));
		}

		[Given(@"I have two requestable absences")]
		public void GivenIHaveDeniedAbsenceRequestWithTwoAbsenceType()
		{
			DataMaker.Data().Apply(new AbsenceConfigurable { Name = "Vacation", Requestable = true });
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open1",
					AvailableAbsence = "Vacation",
					AbsenceRequestWaitlistEnabled = true,
					AutoGrant = "yes"
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Open1" });

			DataMaker.Data().Apply(new AbsenceConfigurable { Name = "Holiday", Requestable = true });
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open2",
					AvailableAbsence = "Holiday",
					AbsenceRequestWaitlistEnabled = true,
					AutoGrant = "yes"
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Open2" });
		}


		[When(@"I change the absence request values with")]
		public void WhenIChangeTheAbsenceRequestValuesWith(Table table)
		{
			var absence = table.Rows[0][1];
			var subject = table.Rows[1][1];

			Browser.Interactions.SelectOptionByTextUsingJQuery(".request .request-edit-absence", absence);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery(".request .request-edit-subject", subject);
		}

		[Then(@"I should see the updated values for the existing absence request in the list with")]
		public void ThenIShouldSeeTheUpdatedValuesForTheExistingAbsenceRequestInTheListWith(Table table)
		{
			var absence = table.Rows[0][1];
			var subject = table.Rows[1][1];

			Browser.Interactions.AssertFirstContains(".request-body .request-data-subject", subject);
			Browser.Interactions.AssertFirstContains(".request-body .request-data-type", absence);
		}

		[Then(@"I should not be able to edit the values for the existing absence request")]
		public void ThenIShouldNotBeAbleToEditTheValuesForTheExistingAbsenceRequest()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".request .request-edit-subject");
			Browser.Interactions.AssertExists(".request .request-edit-absence:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-datefrom:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-timefrom:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-dateto:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-timeto:not(:enabled)");
			Browser.Interactions.AssertExists(".request .request-edit-fullday:not(:enabled)");
		}

		[Then(@"I should see the values of the absence request")]
		public void ThenIShouldSeeTheValuesOfTheAbsenceRequest()
		{
			var request = DataMaker.Data().UserData<ExistingAbsenceRequest>();

			var firstFiftyCharsOfMessage = request.PersonRequest.GetMessage(new NoFormatting()).Substring(0, 50);

			Browser.Interactions.AssertFirstContains(".request-body .request-data-subject", request.PersonRequest.GetSubject(new NoFormatting()));
			Browser.Interactions.AssertFirstContains(".request-body .request-data-message", firstFiftyCharsOfMessage);

			Browser.Interactions.AssertFirstContains(".request-body .request-data-type", request.AbsenceRequest.Absence.Description.Name);

			Browser.Interactions.AssertFirstContains(".request-body .request-data-date",
			                                         request.PersonRequest.Request.Period.StartDateTime.Date.ToShortDateString(
				                                         DataMaker.Data().MyCulture));
			Browser.Interactions.AssertFirstContains(".request-body .request-data-date",
			                                         request.PersonRequest.Request.Period.StartDateTime.ToShortTimeString(
				                                         DataMaker.Data().MyCulture));

			Browser.Interactions.AssertFirstContains(".request-body .request-data-date",
			                                         request.PersonRequest.Request.Period.EndDateTime.Date.ToShortDateString(
				                                         DataMaker.Data().MyCulture));
			Browser.Interactions.AssertFirstContains(".request-body .request-data-date",
			                                         request.PersonRequest.Request.Period.EndDateTime.ToShortTimeString(
				                                         DataMaker.Data().MyCulture));
		}

		[Given(@"I have a denied absence request beacuse of missing workflow control set")]
		public void GivenIHaveADeniedAbsenceRequestBeacuseOfMissingWorkflowControlSet()
		{
			DataMaker.Data().Apply(new ExistingDeniedAbsenceRequest("RequestDenyReasonNoWorkflow"));
		}

		[Given(@"I have an auto denied absence request")]
		public void GivenIHaveAnAutoDeniedAbsenceRequest()
		{
			var absence = DataMaker.Me().Person.WorkflowControlSet.AbsenceRequestOpenPeriods[0].Absence;
			DataMaker.Data().Apply(new ExistingDeniedAbsenceRequest(absence, true));
		}
		
		[Given(@"I have an open workflow control set with absence request waitlisting enabled")]
		public void GivenIHaveAnOpenWorkflowControlSetWithAbsenceRequestWaitlistingEnabled()
		{
			DataMaker.Data().Apply(new AbsenceConfigurable { Name = "Vacation", Requestable = true});
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open",
					AvailableAbsence = "Vacation",
					AbsenceRequestWaitlistEnabled = true,
					AutoGrant="yes"
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Open" });
		}


		[Given(@"(.*) has an open workflow control set with absence request waitlisting enabled")]
		public void HasAnOpenWorkflowControlSetWithAbsenceRequestWaitlistingEnabled(string userName)
		{

			DataMaker.Data().Apply(new AbsenceConfigurable { Name = "Vacation" });
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Open",
					AvailableAbsence = "Vacation",
					AbsenceRequestWaitlistEnabled = true,
					AutoGrant = "yes"
				});

			DataMaker.Person (userName).Apply(new WorkflowControlSetForUser { Name = "Open" });
		}

		[Then(@"I should see the waitlist position is (.*)")]
		public void ThenIShouldSeeTheWaitlistPositionIs(int position)
		{
			Browser.Interactions.AssertFirstContains("#waitlistPosition", position.ToString());
		}
	}
}
