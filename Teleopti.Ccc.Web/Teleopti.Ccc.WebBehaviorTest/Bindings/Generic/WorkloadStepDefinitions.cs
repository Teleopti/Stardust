using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WorkloadStepDefinitions
	{
		[Given(@"there is a Workload with Skill '(.*)' and queuesource '(.*)'")]
		public void GivenThereIsAWorkloadWithSkillAndQueuesource(string skill, string queueSource)
		{
			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				SkillName = skill, 
				QueueSourceName = queueSource
			});
		}
	}

	[Binding]
	public class QueueSourceDefinitions
	{
		[Given(@"there is a QueueSource named '(.*)'")]
		public void GivenThereIsAQueueSourceNamed(string queueSource)
		{
			DataMaker.Data().Apply(new QueueSourceConfigurable
			{
				Name = queueSource
			});
		}		
	}
}