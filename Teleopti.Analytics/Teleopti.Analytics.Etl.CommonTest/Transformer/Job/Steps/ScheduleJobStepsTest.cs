using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class ScheduleJobStepsTest
	{
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null, null);
		}

		[Test]
		public void VerifyScheduleJobSteps()
		{
			IList<IJobStep> jobStepList = new ScheduleJobCollection(_jobParameters);

			foreach (var jobStep in jobStepList)
			{
				if (jobStep is StageScheduleJobStep) //Threaded stuff not tested here...
					continue;
				jobStep.Run(new List<IJobStep>(), null, null, false);
			}
		}
	}
}