using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerOldTest
	{
		[Test]
		public void ShouldGetSkillsAndWorkloads()
		{
			var principalAuthorization = new FullPermission();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var skillRepository = new FakeSkillRepository();
			skillRepository.Has(skill1);
			var forecastMisc = MockRepository.GenerateMock<IWorkloadNameBuilder>();
			var workload = skill1.WorkloadCollection.Single();
			var workloadName = skill1.Name + " - " + workload.Name;
			forecastMisc.Stub(x => x.WorkloadName(skill1.Name, workload.Name)).Return(workloadName);
			var target = new ForecastController(null, skillRepository, null, null, null, new BasicActionThrottler(), null, null, null, null, principalAuthorization, forecastMisc, null);
			var result = target.Skills();
			result.Skills.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			result.Skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			result.Skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workloadName);
		}

		[Test]
		public void ShouldHavePermissionForModifySkill()
		{
			var principalAuthorization = new FullPermission();
			var skillRepository = new FakeSkillRepository();
			
			var target = new ForecastController(null, skillRepository, null, null, null, new BasicActionThrottler(), null, null, null, null, principalAuthorization, null, null);
			var result = target.Skills();
			result.IsPermittedToModifySkill.Should().Be.EqualTo(true);
		}
		
		[Test]
		public void ShouldForecast()
		{
			var forecastCreator = MockRepository.GenerateMock<IForecastCreator>();

			var scenarioId = Guid.NewGuid();
			var forecastInput = new ForecastInput
			{
				ForecastStart = new DateTime(2014, 4, 1),
				ForecastEnd = new DateTime(2014, 4, 29),
				Workload = new ForecastWorkloadInput {},
				ScenarioId = scenarioId
			};
			var scenario = new Scenario("test1").WithId(scenarioId);
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var target = new ForecastController(forecastCreator, null, null, null, null, new BasicActionThrottler(), scenarioRepository, null, null, null, null, null, null);
			IList<ForecastResultModel> forecast = new List<ForecastResultModel>()
			{
				new ForecastResultModel()
				{
					date = new DateTime(2016,05,01),
					vc = 10
				}
			};
			forecastCreator.Stub(x => x.CreateForecastForWorkload(new DateOnlyPeriod(new DateOnly(forecastInput.ForecastStart), new DateOnly(forecastInput.ForecastEnd)), forecastInput.Workload, scenario)).Return(forecast);
			var result = (OkNegotiatedContentResult<WorkloadForecastResultViewModel>)target.Forecast(forecastInput);
			forecastCreator.AssertWasCalled(x => x.CreateForecastForWorkload(new DateOnlyPeriod(new DateOnly(forecastInput.ForecastStart), new DateOnly(forecastInput.ForecastEnd)), forecastInput.Workload, scenario));
			result.Should().Be.OfType<OkNegotiatedContentResult<WorkloadForecastResultViewModel>>();
		}

		[Test]
		public void ShouldEvaluate()
		{
			var forecastViewModelFactory = MockRepository.GenerateMock<IForecastViewModelFactory>();
			var evaluateInput = new EvaluateInput();
			var workloadForecastingViewModel = new WorkloadEvaluateViewModel();
			forecastViewModelFactory.Stub(x => x.Evaluate(evaluateInput)).Return(workloadForecastingViewModel);
			var target = new ForecastController(null, null, forecastViewModelFactory, null, null, new BasicActionThrottler(), null, null, null, null, null, null, null);

			var result = target.Evaluate(evaluateInput);

			result.Result.Should().Be.EqualTo(workloadForecastingViewModel);
		}


		[Test]
		public void ShouldGetQueueStatistics()
		{
			var forecastViewModelFactory = MockRepository.GenerateMock<IForecastViewModelFactory>();
			var queueStatisticsInput = new QueueStatisticsInput();
			var workloadQueueStatisticsViewModel = new WorkloadQueueStatisticsViewModel();
			forecastViewModelFactory.Stub(x => x.QueueStatistics(queueStatisticsInput)).Return(workloadQueueStatisticsViewModel);
			var target = new ForecastController(null, null, forecastViewModelFactory, null, null, new BasicActionThrottler(), null, null, null, null, null, null, null);

			var result = target.QueueStatistics(queueStatisticsInput);

			result.Result.Should().Be.EqualTo(workloadQueueStatisticsViewModel);
		}

		[Test]
		public void ShouldGetForecastResult()
		{
			var workloadId = Guid.NewGuid();
			var scenario = new Scenario("test1").WithId();
			var forecastStart = new DateTime(2014,4,1);
			var forecastEnd = new DateTime(2014,4,29);
			var forecastResultViewModelFactory = MockRepository.GenerateMock<IForecastResultViewModelFactory>();
			var workloadForecastResultViewModel = new WorkloadForecastResultViewModel();
			forecastResultViewModelFactory.Stub(
				x => x.Create(workloadId, new DateOnlyPeriod(new DateOnly(forecastStart), new DateOnly(forecastEnd)), scenario))
				.Return(workloadForecastResultViewModel);
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var target = new ForecastController(null, null, null, forecastResultViewModelFactory, null, new BasicActionThrottler(), scenarioRepository, null, null, null, null, null, null);

			var forecastResultInput = new ForecastResultInput
			{
				WorkloadId = workloadId,
				ForecastStart = forecastStart,
				ForecastEnd = forecastEnd,
				ScenarioId = scenario.Id.GetValueOrDefault()
			};
			var result = target.ForecastResult(forecastResultInput);
			result.Result.Should().Be.EqualTo(workloadForecastResultViewModel);
		}

		[Test]
		public void ShouldGetIntradayPatternViewModel()
		{
			var workloadId = Guid.NewGuid();
			var intradayPatternViewModelFactory = MockRepository.GenerateMock<IIntradayPatternViewModelFactory>();
			var intradayPatternViewModel = new IntradayPatternViewModel();
			var input = new IntradayPatternInput
			{
				WorkloadId = workloadId
			};
			intradayPatternViewModelFactory.Stub(x => x.Create(input)).Return(intradayPatternViewModel);
			var target = new ForecastController(null, null, null, null, intradayPatternViewModelFactory, new BasicActionThrottler(), null, null, null, null, null, null, null);
			
			var result = target.IntradayPattern(input);

			result.Result.Should().Be.EqualTo(intradayPatternViewModel);
		}

		[Test]
		public void ShouldSetOverrideValues()
		{
			var input = new OverrideInput
			{
				Days = new[] {new DateOnly()},
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTasks = 50,
				OverrideTalkTime = 20,
				OverrideAfterCallWork = 25,
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = true
			};
			var scenario = new Scenario("default").WithId(input.ScenarioId);
			var overrideTasksPersister = MockRepository.GenerateMock<IOverridePersister>();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var workloadRepository = new FakeWorkloadRepository();
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill")).WithId(input.WorkloadId);
			workloadRepository.Add(workload);
			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null, null, null);

			var result = target.Override(input);
			result.Result.Success.Should().Be.True();

			overrideTasksPersister.AssertWasCalled(x => x.Persist(scenario, workload, input));
		}

		[Test]
		public void ShouldClearOverrideValues()
		{
			var input = new OverrideInput
			{
				Days = new[] { new DateOnly() },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTasks = 50,
				OverrideTalkTime = 20,
				OverrideAfterCallWork = 25,
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = true
			};
			var scenario = new Scenario("default").WithId(input.ScenarioId);
			var overrideTasksPersister = MockRepository.GenerateMock<IOverridePersister>();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var workloadRepository = new FakeWorkloadRepository();
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill")).WithId(input.WorkloadId);
			workloadRepository.Add(workload);
			var overrideTarget = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null, null, null);
			var overrideResult = overrideTarget.Override(input);
			overrideResult.Result.Success.Should().Be.True();
			overrideTasksPersister.AssertWasCalled(x => x.Persist(scenario, workload, input));

			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null, null, null);

			var clearInput = new OverrideInput
			{
				Days = new[] { new DateOnly() },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = true
			};

			var result = target.Override(clearInput);
			result.Result.Success.Should().Be.True();

			overrideTasksPersister.AssertWasCalled(x => x.Persist(scenario, workload, input));
		}
	}
}
