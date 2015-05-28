using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class QueueAndAgentLogOnJobStepsTest
	{
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);
		}

		[Test]
		public void VerifyQueueAndAgentLogOnJobJobSteps()
		{
			IList<IJobStep> jobStepList = new QueueAndAgentLogOnJobCollection(_jobParameters);
			foreach (IJobStep jobStep in jobStepList)
			{
				IJobStepResult jobStepResult = jobStep.Run(new List<IJobStep>(), null, null, false);
				Assert.IsNotNull(jobStepResult);
			}
		}
	}
}
