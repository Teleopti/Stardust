using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WorkflowControlSetStepDefinitions
	{
		[Given(@"there is a workflow control set with")]
		public void GivenThereIsAWorkflowControlSetWith(Table table)
		{
			var workflowControlSet = table.CreateInstance<WorkflowControlSetConfigurable>();
			DataMaker.Data().Setup(workflowControlSet);
		}

		[Given(@"'(.*)' has the workflow control set '(.*)'")]
		[Given(@"(.*) have the workflow control set '(.*)'")]
		public void GivenIHaveTheWorkflowControlSetPublishedSchedule(string userName, string name)
		{
			var userWorkflowControlSet = new WorkflowControlSetForUser { Name = name };
			DataMaker.Person(userName).Setup(userWorkflowControlSet);
		}

		[Given(@"I have a workflow control set with")]
		public void GivenIHaveAWorkflowControlSetWith(Table table)
		{
			var workflowControlSet = table.CreateInstance<WorkflowControlSetConfigurable>();
			DataMaker.Data().Setup(workflowControlSet);
			var userWorkflowControlSet = new WorkflowControlSetForUser { Name = workflowControlSet.Name };
			DataMaker.Data().Setup(userWorkflowControlSet);
		}

	}
}