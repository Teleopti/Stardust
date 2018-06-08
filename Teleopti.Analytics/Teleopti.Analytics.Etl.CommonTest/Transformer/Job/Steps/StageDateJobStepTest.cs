using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
		 "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
	public class StageDateJobStepTest
	{
		private IJobParameters _jobParameters;
		private IRaptorRepository _raptorRepository;
		private StageDateJobStep _target;

		[SetUp]
		public void Setup()
		{
			_raptorRepository = MockRepository.GenerateMock<IRaptorRepository>();
			var jobHelper = new JobHelperForTest(_raptorRepository, null);
			_jobParameters = JobParametersFactory.SimpleParameters(jobHelper, 15);
			_target = new StageDateJobStep(_jobParameters);
		}

		[Test(Description = "Bug #76093 ETL Agent Statistics failure at stg_date - minimum value can not be higher than maximum value")]
		public void ShouldHandleStartDateLaterThenEndDate()
		{
			_raptorRepository.Stub(x => x.GetMaxDateInDimDate(false))
				.Return(new DateTime(2008, 02, 28, 12, 0, 0));
			
			var result = _target.Run(new List<IJobStep>(), BusinessUnitFactory.CreateSimpleBusinessUnit("TestBU"), null,
				false);

			result.HasError.Should().Be.False();
			result.RowsAffected.Value.Should().Be(0);
		}
	}
}