using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WorkloadStepDefinitions
	{
		[Given(@"there is a workload '(.*)' with skill '(.*)' and queue '(.*)'")]
		public void GivenThereIsAWorkloadWithSkillAndQueue(string workload, string skill, string queue)
		{
			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				WorkloadName = workload,
				SkillName = skill,
				QueueSourceName = queue,
				Open24Hours = true
			});
		}

		[Given(@"there is a workload named '(.*)' with skill '(.*)'")]
		public void GivenThereIsAWorkloadNamedWithSkill(string workload, string skill)
		{
			DataMaker.Data().Apply(new WorkloadConfigurable
			{
				WorkloadName = workload,
				SkillName = skill,
				Open24Hours = true
			});
		}
	}
}