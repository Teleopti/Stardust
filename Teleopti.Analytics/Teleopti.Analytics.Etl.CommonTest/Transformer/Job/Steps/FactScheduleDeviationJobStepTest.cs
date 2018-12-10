using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class FactScheduleDeviationJobStepTest
	{
		private TimeZoneInfo _timeZone;
		private IJobMultipleDate _jobCategoryDates;
		private IRaptorRepository _repository;

		[SetUp]
		public void Setup()
		{
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_jobCategoryDates = new JobMultipleDate(_timeZone);
			_repository = MockRepository.GenerateStrictMock<IRaptorRepository>();
		}

		[Test]
		public void ShouldLoadDataInThreeChunks()
		{
			var startDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-100), DateTimeKind.Local);
			var endDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-20), DateTimeKind.Local);

			_jobCategoryDates.Add(startDate, endDate, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(
					 _jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture, new JobParametersFactory.FakeContainerHolder(), false);

			var jobHelper = new JobHelperForTest(_repository, null);
			jobParameters.Helper = jobHelper;

			_repository.Expect(x => x.FillScheduleDeviationDataMart(new DateTimePeriod(), null, null, 0, null)).Constraints(
																	Rhino.Mocks.Constraints.Is.Anything(),
																	Rhino.Mocks.Constraints.Is.Anything(),
																	Rhino.Mocks.Constraints.Is.Anything(),
																	Rhino.Mocks.Constraints.Is.Anything(),
																	Rhino.Mocks.Constraints.Is.Anything()
																	)
																	.Return(0)
																	.Repeat.Times(3); //Three chunks

			var target = new FactScheduleDeviationJobStep(jobParameters);
			var result = target.Run(new List<IJobStep>(), null, null, false);
			result.JobStepException.Should().Be.Null();

			_repository.VerifyAllExpectations();
		}

		  [Test]
		  public void ShouldRunIfToday()
		  {
				var startDate = DateTime.SpecifyKind(DateTime.Today.AddDays(0), DateTimeKind.Local);
				var endDate = DateTime.SpecifyKind(DateTime.Today.AddDays(0), DateTimeKind.Local);

				_jobCategoryDates.Add(startDate, endDate, JobCategoryType.AgentStatistics);
				var jobParameters = new JobParameters(
						  _jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture, new JobParametersFactory.FakeContainerHolder(), false);

			var jobHelper = new JobHelperForTest(_repository, null);
				jobParameters.Helper = jobHelper;

				_repository.Expect(x => x.FillScheduleDeviationDataMart(new DateTimePeriod(), null, null, 0, null)).Constraints(
																						  Rhino.Mocks.Constraints.Is.Anything(),
																						  Rhino.Mocks.Constraints.Is.Anything(),
																						  Rhino.Mocks.Constraints.Is.Anything(),
																						  Rhino.Mocks.Constraints.Is.Anything(),
																	Rhino.Mocks.Constraints.Is.Anything()
																						  )
																						  .Return(0)
																						  .Repeat.Times(1); 

				var target = new FactScheduleDeviationJobStep(jobParameters);
				var result = target.Run(new List<IJobStep>(), null, null, false);
				result.JobStepException.Should().Be.Null();

				_repository.VerifyAllExpectations();
		  }

	}
}
