using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{


	[TestFixture]
	public class KpiJobSteps
	{
		[SetUp]
		public void Setup()
		{
			_jobParameters.Helper = new JobHelperForTest(raptorRepository, null);
		}

		private readonly IJobParameters _jobParameters = JobParametersFactory.SimpleParameters(false);
		private readonly IRaptorRepository raptorRepository = new RaptorRepositoryForTest();

		[Test]
		public void StageKpiJobStepTest()
		{
			StageKpiJobStep ss = new StageKpiJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}

		[Test]
		public void StageScorecardJobStepTest()
		{
			StageScorecardJobStep ss = new StageScorecardJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}

		[Test]
		public void StageScorecardKpiJobStepTest()
		{
			StageScorecardKpiJobStep ss = new StageScorecardKpiJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}
		[Test]
		public void StageKpiTargetTeamJobStepTest()
		{
			StageKpiTargetTeamJobStep ss = new StageKpiTargetTeamJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}


		[Test]
		public void DimKpiJobStepTest()
		{
			DimKpiJobStep ss = new DimKpiJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}
		[Test]
		public void DimScorecardJobStepTest()
		{
			DimScorecardJobStep ss = new DimScorecardJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}
		[Test]
		public void ScorecardKpiJobStepTest()
		{
			ScorecardKpiJobStep ss = new ScorecardKpiJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}
		[Test]
		public void FactKpiTargetTeamJobStepTest()
		{
			FactKpiTargetTeamJobStep ss = new FactKpiTargetTeamJobStep(_jobParameters);
			IJobStepResult jobStepResult = ss.Run(new List<IJobStep>(), null, null, false);
			Assert.IsNotNull(jobStepResult);

		}



	}
}

