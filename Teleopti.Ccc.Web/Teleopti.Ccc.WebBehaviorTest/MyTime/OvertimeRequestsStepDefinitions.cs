using System;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class OvertimeRequestsStepDefinitions
	{
		[Given(@"there is multiplicator definition set")]
		public void GivenThereIsMultiplicatorDefinitionSet(Table table)
		{
			DataMaker.Data().Apply(new MultiplicatorDefinitionSetConfigurable {Name = table.Rows[0].Values.ElementAt(1)});
		}

		[When(@"I click on the day summary for the first day of next week")]
		public void WhenIClickOnTheDaySummaryForTheFirstDayOfNextWeek()
		{
			var date = DateTime.Now.AddDays(7).Date;
			Browser.Interactions.Click($".weekview-day[data-mytime-date='{date:yyyy-MM-dd}'] .weekview-day-summary");
		}

		[Given(@"I have created an overtime request with subject '(.*)'")]
		public void GivenIHaveCreatedAnOvertimeRequestWithSubject(string subject)
		{
			DataMaker.Data().Apply(new ExistingOvertimeRequest { Subject = subject });
		}

		[Then(@"I should see my existing overtime request with subject '(.*)'")]
		public void ThenIShouldSeeMyExistingOvertimeRequestWithSubject(string subject)
		{
			Browser.Interactions.AssertExists(".request");
			Browser.Interactions.AssertFirstContains(".request-data-subject", subject);
		}

		[When(@"I open add new overtime request form")]
		public void WhenIOpenAddNewOvertimeRequestForm()
		{
			Browser.Interactions.Click("#addOvertimeRequest");
		}

		[When(@"I fill overtime request form with subject '(.*)'")]
		public void WhenIFillOvertimeRequestFormWithSubject(string subject)
		{
			Browser.Interactions.FillWith(".overtime-add-container input.request-new-subject", subject);
		}

		[When(@"I save overtime request")]
		public void WhenISaveOvertimeRequest()
		{
			Browser.Interactions.Click(".overtime-add-container button.request-new-send");
			Thread.Sleep(1000);
		}

		[Given(@"I have a workflow control set with overtime request open periods")]
		public void GivenIHaveAWorkflowControlSetWithOvertimeRequestOpenPeriods()
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Published 100 days, SA open",
					SchedulePublishedToDate = "2030-12-01",
					StudentAvailabilityPeriodIsClosed = false,
					OvertimeRequestOpenPeriodRollingStart = 0,
					OvertimeRequestOpenPeriodRollingEnd = 13
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published 100 days, SA open" });
		}

		[Given(@"I have a workflow control set with overtime request open periods and auto approval")]
		public void GivenIHaveAWorkflowControlSetWithOvertimeRequestOpenPeriodsAndAutoApproval()
		{
			DataMaker.Data()
				.Apply(new WorkflowControlSetConfigurable
				{
					Name = "Published 100 days, SA open",
					SchedulePublishedToDate = "2030-12-01",
					StudentAvailabilityPeriodIsClosed = false,
					OvertimeRequestOpenPeriodRollingStart = 0,
					OvertimeRequestOpenPeriodRollingEnd = 13,
					OvertimeRequestAutoApprove = true
				});
			DataMaker.Data().Apply(new WorkflowControlSetForUser { Name = "Published 100 days, SA open" });
		}

		[Then(@"I should see my existing overtime request with status '(.*)'")]
		public void ThenIShouldSeeMyExistingOvertimeRequestWithStatus(string status)
		{
			Browser.Interactions.AssertExists(".request");
			Browser.Interactions.AssertFirstContains(".request-details .request-label", status);
		}
	}
}
