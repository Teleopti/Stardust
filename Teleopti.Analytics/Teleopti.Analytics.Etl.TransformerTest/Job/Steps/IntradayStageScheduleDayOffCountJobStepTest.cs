using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class IntradayStageScheduleDayOffCountJobStepTest
	{
		private MockRepository _mock;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
		}

		[Test]
		public void VerifyDefaultProperties()
		{
			var jobParameters = JobParametersFactory.SimpleParameters(false);
			var target = new IntradayStageScheduleDayOffCountJobStep(jobParameters);
			jobParameters.Helper = _mock.DynamicMock<IJobHelper>(); 
			Assert.AreEqual(JobCategoryType.Schedule, target.JobCategory);
			Assert.AreEqual("stg_schedule_day_off_count, stg_day_off, dim_day_off", target.Name);
			Assert.IsFalse(target.IsBusinessUnitIndependent);
		}

		[Test]
		public void ShouldProcessDayOffAlsoInThisStep()
		{
			var jobParameters = _mock.DynamicMock<IJobParameters>();
			var stateHolder = _mock.DynamicMock<ICommonStateHolder>();
			var jobHelper = _mock.DynamicMock<IJobHelper>();
			var repository = _mock.DynamicMock<IRaptorRepository>();
			var dic = new Dictionary<DateTimePeriod, IScheduleDictionary>();
			var transformer = _mock.DynamicMock<IScheduleDayOffCountTransformer>();
			var subStep = _mock.DynamicMock<IEtlDayOffSubStep>();
			var target = new IntradayStageScheduleDayOffCountJobStep(jobParameters)
							{
								Transformer = transformer,
								DayOffSubStep = subStep
							};

			Expect.Call(jobParameters.StateHolder).Return(stateHolder);
			Expect.Call(stateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { new Scenario("scenario"){DefaultScenario = true} });
			Expect.Call(stateHolder.GetScheduleCashe()).Return(dic);
			Expect.Call(stateHolder.GetSchedulePartPerPersonAndDate(dic)).Return(new List<IScheduleDay>());
			Expect.Call(() => transformer.Transform(new List<IScheduleDay>(), null,1)).IgnoreArguments();
			Expect.Call(jobParameters.Helper).Return(jobHelper);
			Expect.Call(jobHelper.Repository).Return(repository);
			Expect.Call(repository.PersistScheduleDayOffCount(Arg<DataTable>.Is.Anything)).Return(1);
			Expect.Call(subStep.StageAndPersistToMart(DayOffEtlLoadSource.ScheduleDayOff, RaptorTransformerHelper.CurrentBusinessUnit, repository)).
				Return(1);
			_mock.ReplayAll();
			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
			result.RowsAffected.Should().Be.EqualTo(2);
			_mock.VerifyAll();
		}

		[Test]
		public void ShouldNotProcessOtherThanDefaultScenario()
		{
			var jobParameters = _mock.DynamicMock<IJobParameters>();
			var stateHolder = _mock.DynamicMock<ICommonStateHolder>();
			var target = new IntradayStageScheduleDayOffCountJobStep(jobParameters);

			Expect.Call(jobParameters.StateHolder).Return(stateHolder);
			Expect.Call(stateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { new Scenario("scenario") { DefaultScenario = false } });
			_mock.ReplayAll();
			target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);
			_mock.VerifyAll();
		}

		[Test]
		public void ShouldRunDifferentInFactOnIntraday()
		{
			var raptorRepository = _mock.StrictMock<IRaptorRepository>();
			var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();

			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelper(raptorRepository, null, null, null);
			var scenario = _mock.DynamicMock<IScenario>();
			Expect.Call(commonStateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });
			Expect.Call(scenario.DefaultScenario).Return(true);

			var step = new FactScheduleDayCountJobStep(jobParameters, true);

			var bu = _mock.DynamicMock<IBusinessUnit>();
			Expect.Call(raptorRepository.FillIntradayScheduleDayCountDataMart(bu, scenario)).Return(5).IgnoreArguments();
			Expect.Call(() => commonStateHolder.UpdateThisTime("Schedules", bu)).IgnoreArguments();
			_mock.ReplayAll();
			step.Run(new List<IJobStep>(), bu, null, true);
			_mock.VerifyAll();
		}

	}
}
