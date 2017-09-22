using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
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
			var scenario = new Scenario("scenario") { DefaultScenario = true };

			Expect.Call(stateHolder.ScenarioCollectionDeletedExcluded).Return(new List<IScenario> { scenario });

			Expect.Call(jobParameters.StateHolder).Return(stateHolder);
			Expect.Call(repository.LastChangedDate(null, "")).IgnoreArguments().Return(new LastChangedReadModel { LastTime = DateTime.Now, ThisTime = DateTime.Now });
			Expect.Call(repository.ChangedAvailabilityOnStep(new DateTime(), null))
				.IgnoreArguments()
				.Return(new List<IStudentAvailabilityDay>());
			Expect.Call(() => transformer.Transform(new List<IStudentAvailabilityDay>(), new DataTable("d"), stateHolder, scenario)).IgnoreArguments();
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
	}
}
