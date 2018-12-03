using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class StageSchedulePreferenceJobStepTest
	{
		[Test]
		public void VerifyDefaultProperties()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			var target = new StageSchedulePreferenceJobStep(jobParameters);
			jobParameters.Helper = new JobHelperForTest(new RaptorRepositoryForTest(), null);
			Assert.AreEqual(JobCategoryType.Schedule, target.JobCategory);
			Assert.AreEqual("stg_schedule_preference, stg_day_off, dim_day_off", target.Name);
			Assert.IsFalse(target.IsBusinessUnitIndependent);
		}

		[Test]
		public void ShouldProcessDayOffAlsoInThisStep()
		{
			var jobParameters = MockRepository.GenerateMock<IJobParameters>();
			var stateHolder = MockRepository.GenerateMock<ICommonStateHolder>();
			var jobHelper = MockRepository.GenerateMock<IJobHelper>();
			var repository = MockRepository.GenerateMock<IRaptorRepository>();
			
			IJobMultipleDate jobMultipleDate = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			jobMultipleDate.Add(DateTime.UtcNow, DateTime.UtcNow, JobCategoryType.Schedule);

			jobParameters.Stub(x => x.JobCategoryDates).Return(jobMultipleDate);
			jobParameters.Stub(x => x.StateHolder).Return(stateHolder);
			stateHolder.Stub(x => x.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> {new Scenario("scenario")});
			stateHolder.Stub(
				x => x.LoadSchedulePartsPerPersonAndDate(Arg<DateTimePeriod>.Is.Anything, Arg<IScenario>.Is.Anything)).Return(
					new List<IScheduleDay>());
			jobParameters.Stub(x => x.Helper).Return(jobHelper);
			jobHelper.Stub(x => x.Repository).Return(repository);

			var target = new StageSchedulePreferenceJobStep(jobParameters)
			{
				Transformer = MockRepository.GenerateMock<ISchedulePreferenceTransformer>(),
				ScheduleDayRestrictor = MockRepository.GenerateMock<IScheduleDayRestrictor>(),
				DayOffSubStep = MockRepository.GenerateMock<IEtlDayOffSubStep>()
			};
			
			target.ScheduleDayRestrictor.Stub(
				x => x.RemoveScheduleDayEndingTooLate(Arg<IList<IScheduleDay>>.Is.Anything, Arg<DateTime>.Is.Anything)).Return(
					new List<IScheduleDay>());
			target.Transformer.Stub(x => x.Transform(Arg<IList<IScheduleDay>>.Is.Anything, Arg<DataTable>.Is.Anything));

			repository.Expect(x => x.PersistSchedulePreferences(Arg<DataTable>.Is.Anything)).Return(1);

			target.DayOffSubStep.Expect(
				x => x.StageAndPersistToMart(DayOffEtlLoadSource.SchedulePreference, RaptorTransformerHelper.CurrentBusinessUnit, repository)).
				Return(1);

			IJobStepResult result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			repository.VerifyAllExpectations();
			target.DayOffSubStep.VerifyAllExpectations();
			result.RowsAffected.Should().Be.EqualTo(2);

		}

	}
}
