﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class IndexMaintenanceJobsTest
	{
		[SetUp]
		public void Setup()
		{
			_jobParameters.Helper = new JobHelperForTest(_raptorRepository, null);
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
				_raptorRepository.AssertWasCalled(x => x.PerformIndexMaintenance(DatabaseEnum.Analytics), o => o.IgnoreArguments());
			}
		}

		[Test]
		public void ShouldExecuteIndexMaintenanceForApp()
		{
			using (var indexMaintenance = new AppIndexMaintenanceJobStep(_jobParameters))
			{
				var jobStepResult = indexMaintenance.Run(new List<IJobStep>(), null, null, true);
				Assert.IsNotNull(jobStepResult);
				_raptorRepository.AssertWasCalled(x => x.PerformIndexMaintenance(DatabaseEnum.Application), options => options.IgnoreArguments());
			}
		}

		[Test]
		public void ShouldExecuteIndexMaintenanceForAgg()
		{
			using (var indexMaintenance = new AggIndexMaintenanceJobStep(_jobParameters))
			{
				var jobStepResult = indexMaintenance.Run(new List<IJobStep>(), null, null, true);
				Assert.IsNotNull(jobStepResult);
				_raptorRepository.AssertWasCalled(x => x.PerformIndexMaintenance(DatabaseEnum.Agg), options => options.IgnoreArguments());
			}
		}
	}
}
