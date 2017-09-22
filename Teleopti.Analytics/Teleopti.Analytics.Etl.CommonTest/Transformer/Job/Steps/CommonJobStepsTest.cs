using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class CommonJobStepsTest
	{
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);
		}

		[Test]
		public void DimDateJobStep()
		{
			DimDateJobStep ss = new DimDateJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void StageDateJobStep()
		{
			StageDateJobStep ss = new StageDateJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void StageWorkloadJobStep()
		{
			StageWorkloadJobStep ss = new StageWorkloadJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void StageSkillJobStep()
		{
			StageSkillJobStep ss = new StageSkillJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void DimSkillJobStep()
		{
			DimSkillJobStep ss = new DimSkillJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void DimWorkloadJobStep()
		{
			DimWorkloadJobStep ss = new DimWorkloadJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void StageBusinessUnitJobStep()
		{
			StageBusinessUnitJobStep ss = new StageBusinessUnitJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void DimBusinessUnitJobStep()
		{
			DimBusinessUnitJobStep ss = new DimBusinessUnitJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}

		[Test]
		public void DimQueueJobStep()
		{
			DimQueueJobStep ss = new DimQueueJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);
		}
	}
}