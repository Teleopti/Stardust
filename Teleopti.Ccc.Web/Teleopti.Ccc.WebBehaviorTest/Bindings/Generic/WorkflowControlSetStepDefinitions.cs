using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WorkflowControlSetStepDefinitions
	{
		[Given(@"there is a workflow control set with")]
		public void GivenThereIsAWorkflowControlSetWith(Table table)
		{
			DataMaker.ApplyFromTable<WorkflowControlSetConfigurable>(table);
		}

		[Given(@"there is a workflow control set named '(.*)' publishing schedules until '(.*)'")]
		public void GivenThereIsAWorkflowControlSetWith(string name, string publishedToDate)
		{
			DataMaker.Data().Apply(new WorkflowControlSetConfigurable
				{
					Name = name,
					SchedulePublishedToDate = publishedToDate
				});
		}

		[Given(@"'?(.*)'? (?:has|have) the workflow control set '(.*)'")]
		public void GivenIHaveTheWorkflowControlSetPublishedSchedule(string person, string name)
		{
			var userWorkflowControlSet = new WorkflowControlSetForUser { Name = name };
			DataMaker.Person(person).Apply(userWorkflowControlSet);
		}

		[Given(@"'(.*)' has a workflow control set publishing schedules until '(.*)'")]
		[Given(@"(I) have a workflow control set publishing schedules until '(.*)'")]
		public void GivenHaveTheWorkflowControlSetPublishingSchedulesUntil(string person, string publishedToDate)
		{
			var workflowControlSet = new WorkflowControlSetConfigurable {SchedulePublishedToDate = publishedToDate};
			DataMaker.Data().Apply(workflowControlSet);
			var userWorkflowControlSet = new WorkflowControlSetForUser { Name = workflowControlSet.Name };
			DataMaker.Person(person).Apply(userWorkflowControlSet);
		}

		[Given(@"'(.*)' have a workflow control set with")]
		[Given(@"(I) have a workflow control set with")]
		public void GivenIHaveAWorkflowControlSetWith(string person, Table table)
		{
			var workflowControlSet = table.CreateInstance<WorkflowControlSetConfigurable>();
			DataMaker.Data().Apply(workflowControlSet);
			var userWorkflowControlSet = new WorkflowControlSetForUser { Name = workflowControlSet.Name };
			DataMaker.Person(person).Apply(userWorkflowControlSet);
		}

	}
}