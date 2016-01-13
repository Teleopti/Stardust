using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class ForecastJobStepsTest
	{
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);
		}

		[Test]
		public void VerifyForecastJobSteps()
		{
			IList<IJobStep> jobStepList = new ForecastJobCollection(_jobParameters);
			foreach (IJobStep jobStep in jobStepList)
			{
				IJobStepResult jobStepResult = jobStep.Run(new List<IJobStep>(), null, null, false);
				Assert.IsNotNull(jobStepResult);
			}

		}
	}
}
