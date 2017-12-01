using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class ScheduleJobStepsTest
	{
		[Test]
		public void VerifyScheduleJobSteps()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);
			IList<IJobStep> jobStepList = new ScheduleJobCollection(jobParameters);

			foreach (var jobStep in jobStepList)
			{
				if (jobStep is StageScheduleJobStep) //Threaded stuff not tested here...
					continue;
				jobStep.Run(new List<IJobStep>(), null, null, false);
			}
		}
	}
}