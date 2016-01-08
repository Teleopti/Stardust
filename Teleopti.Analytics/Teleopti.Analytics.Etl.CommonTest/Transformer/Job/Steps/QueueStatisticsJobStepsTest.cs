using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class QueueStatisticsJobStepsTest
	{
		private IJobParameters _jobParameters;
		IList<IJobStep> _jobStepList;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null);
			_jobStepList = new QueueStatisticsJobCollection(_jobParameters);
			Assert.AreEqual(4, _jobStepList.Count);
		}

		[Test]
		public void VerifyQueueStatisticsJobSteps()
		{
			foreach (IJobStep jobStep in _jobStepList)
			{
				IJobStepResult jobStepResult = jobStep.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
				Assert.IsNotNull(jobStepResult);
				Assert.AreEqual("Done", jobStepResult.Status);
			}
		}
	}
}
