using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class IntradayStageSchedulePreferenceJobStepTest
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
			var target = new StageSchedulePreferenceJobStep(jobParameters);
			jobParameters.Helper = _mock.DynamicMock<IJobHelper>();
			Assert.AreEqual(JobCategoryType.Schedule, target.JobCategory);
			Assert.AreEqual("stg_schedule_preference, stg_day_off, dim_day_off", target.Name);
			Assert.IsFalse(target.IsBusinessUnitIndependent);
		}


		[Test]
		public void ShouldRunDifferentInFactOnIntraday()
		{
			var raptorRepository = _mock.StrictMock<IRaptorRepository>();
			var commonStateHolder = _mock.StrictMock<ICommonStateHolder>();

			var jobParameters = JobParametersFactory.SimpleParameters(false);
			jobParameters.StateHolder = commonStateHolder;
			jobParameters.Helper = new JobHelperForTest(raptorRepository, null);
			var scenario = _mock.DynamicMock<IScenario>();
			Expect.Call(commonStateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });
			Expect.Call(scenario.DefaultScenario).Return(true);

			var step = new FactSchedulePreferenceJobStep(jobParameters, true);

			var bu = _mock.DynamicMock<IBusinessUnit>();
			Expect.Call(raptorRepository.FillIntradayFactSchedulePreferenceMart(bu, scenario)).Return(5).IgnoreArguments();
			Expect.Call(() => commonStateHolder.UpdateThisTime("Schedules", bu)).IgnoreArguments();
			_mock.ReplayAll();
			step.Run(new List<IJobStep>(), bu, null, true);
			_mock.VerifyAll();
		}
	}
}
