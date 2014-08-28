using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
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
			var jobParameters = new JobParameters(_jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture);

			var jobHelper = new JobHelper(_repository, null, null, null);
			jobParameters.Helper = jobHelper;

			_repository.Expect(x => x.FillScheduleDeviationDataMart(new DateTimePeriod(), null, null, false)).Constraints(
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
            var jobParameters = new JobParameters(_jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture);

			var jobHelper = new JobHelper(_repository, null, null, null);
            jobParameters.Helper = jobHelper;

            _repository.Expect(x => x.FillScheduleDeviationDataMart(new DateTimePeriod(), null, null, false)).Constraints(
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
