using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class IndexMaintenanceJobsTest
	{
		[SetUp]
		public void Setup()
		{
			_jobParameters.Helper = new JobHelperForTest(_raptorRepository, null, null);
		}

		private readonly IJobParameters _jobParameters = JobParametersFactory.SimpleParameters(false);
		private readonly IRaptorRepository _raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();

		[Test]
		public void ShouldExecuteIndexMaintenanceForAnalytics()
		{
			using (var indexMaintenance = new AnalyticsIndexMaintenanceJobStep(_jobParameters))
			{
				var jobStepResult = indexMaintenance.Run(new List<IJobStep>(), null, null, true);
				Assert.IsNotNull(jobStepResult);
				_raptorRepository.AssertWasCalled(x => x.PerformIndexMaintenance("Analytics"), o => o.IgnoreArguments());
			}
		}

		[Test]
		public void ShouldExecuteIndexMaintenanceForApp()
		{
			using (var indexMaintenance = new AppIndexMaintenanceJobStep(_jobParameters))
			{
				var jobStepResult = indexMaintenance.Run(new List<IJobStep>(), null, null, true);
				Assert.IsNotNull(jobStepResult);
				_raptorRepository.AssertWasCalled(x => x.PerformIndexMaintenance("App"), options => options.IgnoreArguments());
			}
		}

		[Test]
		public void ShouldExecuteIndexMaintenanceForAgg()
		{
			using (var indexMaintenance = new AggIndexMaintenanceJobStep(_jobParameters))
			{
				var jobStepResult = indexMaintenance.Run(new List<IJobStep>(), null, null, true);
				Assert.IsNotNull(jobStepResult);
				_raptorRepository.AssertWasCalled(x => x.PerformIndexMaintenance("Agg"), options => options.IgnoreArguments());
			}
		}
	}
}
