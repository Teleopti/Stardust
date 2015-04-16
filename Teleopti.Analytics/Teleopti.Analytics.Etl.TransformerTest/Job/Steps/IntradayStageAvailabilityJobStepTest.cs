using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class IntradayStageAvailabilityJobStepTest
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
			var target = new StageAvailabilityJobStep(jobParameters);
			jobParameters.Helper = _mock.DynamicMock<IJobHelper>();
			Assert.AreEqual(JobCategoryType.Schedule, target.JobCategory);
            Assert.AreEqual("stg_hourly_availability", target.Name);
			Assert.IsFalse(target.IsBusinessUnitIndependent);
		}

		[Test]
		public void ShouldProcessAvailabilities()
		{
			var jobParameters = _mock.DynamicMock<IJobParameters>();
			var stateHolder = _mock.DynamicMock<ICommonStateHolder>();
			var jobHelper = _mock.DynamicMock<IJobHelper>();
			var repository = _mock.DynamicMock<IRaptorRepository>();
			var transformer = _mock.DynamicMock<IIntradayAvailabilityTransformer>();
			var scenario = new Scenario("scenario") {DefaultScenario = true};

			Expect.Call(stateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });
			
			Expect.Call(jobParameters.StateHolder).Return(stateHolder);
			Expect.Call(repository.LastChangedDate(null, "")).IgnoreArguments().Return(new LastChangedReadModel { LastTime = DateTime.Now, ThisTime = DateTime.Now });
			Expect.Call(repository.ChangedAvailabilityOnStep(new DateTime(), null))
				.IgnoreArguments()
				.Return(new List<IStudentAvailabilityDay>());
			Expect.Call(() => transformer.Transform(new List<IStudentAvailabilityDay>(), new DataTable("d"),stateHolder, scenario)).IgnoreArguments();
			Expect.Call(jobParameters.Helper).Return(jobHelper);
			Expect.Call(jobHelper.Repository).Return(repository);
			Expect.Call(repository.PersistAvailability(new DataTable("d"))).IgnoreArguments().Return(5);
			_mock.ReplayAll();
			
			var target = new IntradayStageAvailabilityJobStep(jobParameters)
			{
				Transformer = transformer
			};
			
			
			var result = target.Run(new List<IJobStep>(), null, new List<IJobResult>(), true);

			result.RowsAffected.Should().Be.EqualTo(5);
			_mock.VerifyAll();
		}

		[Test]
		public void ShouldNotProcessOtherThanDefaultScenario()
		{
			var jobParameters = _mock.DynamicMock<IJobParameters>();
			var stateHolder = _mock.DynamicMock<ICommonStateHolder>();
			
			Expect.Call(jobParameters.StateHolder).Return(stateHolder);
			Expect.Call(stateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { new Scenario("scenario") { DefaultScenario = false } });
			_mock.ReplayAll();
			var target = new IntradayStageAvailabilityJobStep(jobParameters);
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

			var step = new FactAvailabilityJobStep(jobParameters, true);

			var bu = _mock.DynamicMock<IBusinessUnit>();
			Expect.Call(raptorRepository.FillIntradayFactAvailabilityMart(bu, scenario)).Return(5).IgnoreArguments();
			Expect.Call(() => commonStateHolder.UpdateThisTime("Availability", bu)).IgnoreArguments();
			_mock.ReplayAll();
			step.Run(new List<IJobStep>(), bu, null, true);
			_mock.VerifyAll();
		}
	}
}
