using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class InitialJobStepsTest
	{
		[SetUp]
		public void Setup()
		{
			_jobParameters.Helper = new JobHelper(_raptorRepository, null, null, null);
		}

		private readonly IJobParameters _jobParameters = JobParametersFactory.SimpleParameters(false);
		private readonly IRaptorRepository _raptorRepository = new RaptorRepositoryForTest();

		[Test]
		public void BridgeTimeZoneJobStepTest()
		{
			var ss = new BridgeTimeZoneJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void DimIntervalJobStepTest()
		{
			var ss = new DimIntervalJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void DimTimeZoneJobStepTest()
		{
			var ss = new DimTimeZoneJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test] 
		public void StageTimeZoneBridgeJobStepTest()
		{
			var ss = new StageTimeZoneBridgeJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void StageTimeZoneJobStepTest()
		{
			var ss = new StageTimeZoneJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}
	}
}