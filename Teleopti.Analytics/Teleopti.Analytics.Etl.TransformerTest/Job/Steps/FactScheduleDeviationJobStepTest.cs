using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
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
			var endDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-30), DateTimeKind.Local);

			_jobCategoryDates.Add(startDate, endDate, JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(_jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture);
			
			var jobHelper = new JobHelper(_repository, null, null);
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
		public void ShouldOnlyLoadDataUpUntilToday()
		{
			var startDate = DateTime.SpecifyKind(DateTime.Now.Date, DateTimeKind.Local);
			var endDate = startDate.AddDays(10);
			_jobCategoryDates.Add(startDate, endDate, JobCategoryType.AgentStatistics);
			

			var expectedPeriod = extractExpectedPeriod(_jobCategoryDates.AllDatePeriodCollection[0]);
			_repository.Expect(x => x.FillScheduleDeviationDataMart(expectedPeriod, RaptorTransformerHelper.CurrentBusinessUnit, _timeZone, false)).Constraints(
																	Rhino.Mocks.Constraints.Is.Equal(expectedPeriod),
																	Rhino.Mocks.Constraints.Is.Matching<IBusinessUnit>(b => b.Id == RaptorTransformerHelper.CurrentBusinessUnit.Id),
																	Rhino.Mocks.Constraints.Is.Matching<TimeZoneInfo>(t => t.Id == _timeZone.Id),
																	Rhino.Mocks.Constraints.Is.Equal(false)
																	).Return(0);

			var target = new FactScheduleDeviationJobStep(CreateJobParameter());
			var result = target.Run(new List<IJobStep>(), null, null, false);
			result.JobStepException.Should().Be.Null();

			_repository.VerifyAllExpectations();
		}

		private IJobParameters CreateJobParameter()
		{
			var jobParameters = new JobParameters(_jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture);

			var jobHelper = new JobHelper(_repository, null, null);
			jobParameters.Helper = jobHelper;

			return jobParameters;
		}

		[Test]
		public void ShouldNotLoadDataWhenPeriodIsInTheFuture()
		{
			var startDateInFuture = DateTime.SpecifyKind(DateTime.Now.Date.AddDays(2), DateTimeKind.Local);
			_jobCategoryDates.Add(startDateInFuture, startDateInFuture.AddDays(10), JobCategoryType.AgentStatistics);
			var jobParameters = new JobParameters(_jobCategoryDates, 1, _timeZone.Id, 15, "", "", CultureInfo.CurrentCulture);

			var jobHelper = new JobHelper(_repository, null, null);
			jobParameters.Helper = jobHelper;

			_repository.AssertWasNotCalled(x => x.FillScheduleDeviationDataMart(new DateTimePeriod(), null, null, false), x=> x.IgnoreArguments());

			var target = new FactScheduleDeviationJobStep(jobParameters);
			var result = target.Run(new List<IJobStep>(), null, null, false);
			result.JobStepException.Should().Be.Null();

			_repository.VerifyAllExpectations();

		}

		private static DateTimePeriod extractExpectedPeriod(IJobMultipleDateItem jobMultipleDateItem)
		{
			var endDate = jobMultipleDateItem.StartDateUtc.AddDays(1).AddMilliseconds(-1);
			return new DateTimePeriod(jobMultipleDateItem.StartDateUtc, endDate);
		}
	}
}
