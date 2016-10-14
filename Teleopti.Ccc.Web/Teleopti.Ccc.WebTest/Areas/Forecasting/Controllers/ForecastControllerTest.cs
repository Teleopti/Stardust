using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[TestFixture]
	public class ForecastControllerTest
	{
		[Test]
		public void ShouldGetSkillsAndWorkloads()
		{
			var principalAuthorization = new FullPermission();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var skillRepository = new FakeSkillRepository();
			skillRepository.Has(skill1);
			var forecastMisc = MockRepository.GenerateMock<IForecastMisc>();
			var workload = skill1.WorkloadCollection.Single();
			var workloadName = skill1.Name + " - " + workload.Name;
			forecastMisc.Stub(x => x.WorkloadName(skill1.Name, workload.Name)).Return(workloadName);
			var target = new ForecastController(null, skillRepository, null, null, null, new BasicActionThrottler(), null, null, null, null, principalAuthorization, forecastMisc);
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
			
			var target = new ForecastController(null, skillRepository, null, null, null, new BasicActionThrottler(), null, null, null, null, principalAuthorization, null);
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
				Workloads = new ForecastWorkloadInput[] {},
				ScenarioId = scenarioId,
				BlockToken = new BlockToken(),
				IsLastWorkload = false
			};
			var scenario = new Scenario("test1").WithId(scenarioId);
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var target = new ForecastController(forecastCreator, null, null, null, null, new BasicActionThrottler(), scenarioRepository, null, null, null, null, null);
			var result = target.Forecast(forecastInput);
			result.Result.Success.Should().Be.True();
			result.Result.BlockToken.Should().Be.EqualTo(forecastInput.BlockToken);
			forecastCreator.AssertWasCalled(x => x.CreateForecastForWorkloads(new DateOnlyPeriod(new DateOnly(forecastInput.ForecastStart), new DateOnly(forecastInput.ForecastEnd)), forecastInput.Workloads, scenario));
		}

		[Test]
		public void ShouldFinishToken()
		{
			var actionThrottler = MockRepository.GenerateMock<IActionThrottler>();
			var target = new ForecastController(MockRepository.GenerateMock<IForecastCreator>(), null, null, null, null, actionThrottler, new FakeScenarioRepository(), null, null, null, null, null);

			var blockToken = new BlockToken();
			target.Forecast(new ForecastInput
			{
				BlockToken = blockToken,
				IsLastWorkload = true
			});
			actionThrottler.AssertWasCalled(x => x.Finish(blockToken));
		}

		[Test]
		public void ShouldReturnErrorIfForecastingIsRunning()
		{
			var actionThrottler = MockRepository.GenerateMock<IActionThrottler>();
			var target = new ForecastController(null, null, null, null, null, actionThrottler, null, null, null, null, null, null);

			actionThrottler.Stub(x => x.IsBlocked(ThrottledAction.Forecasting)).Return(true);
			var result = target.Forecast(new ForecastInput
			{
				IsLastWorkload = false
			});
			result.Result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldUseOldToken()
		{
			var actionThrottler = MockRepository.GenerateMock<IActionThrottler>();
			var target = new ForecastController(MockRepository.GenerateMock<IForecastCreator>(), null, null, null, null, actionThrottler, new FakeScenarioRepository(), null, null, null, null, null);

			var blockToken = new BlockToken();
			target.Forecast(new ForecastInput
			{
				BlockToken = blockToken,
				IsLastWorkload = false
			});
			actionThrottler.AssertWasCalled(x => x.Resume(blockToken));
			actionThrottler.AssertWasCalled(x => x.Pause(blockToken, TimeSpan.FromSeconds(20)));
		}

		[Test]
		public void ShouldCreateNewToken()
		{
			var actionThrottler = MockRepository.GenerateMock<IActionThrottler>();
			var blockToken = new BlockToken();
			actionThrottler.Stub(x => x.Block(ThrottledAction.Forecasting)).Return(blockToken);
			var target = new ForecastController(MockRepository.GenerateMock<IForecastCreator>(), null, null, null, null, actionThrottler, new FakeScenarioRepository(), null, null, null, null, null);

			target.Forecast(new ForecastInput());
			actionThrottler.AssertWasCalled(x => x.Pause(blockToken, TimeSpan.FromSeconds(20)));
		}

		[Test]
		public void ShouldEvaluate()
		{
			var forecastViewModelFactory = MockRepository.GenerateMock<IForecastViewModelFactory>();
			var evaluateInput = new EvaluateInput();
			var workloadForecastingViewModel = new WorkloadEvaluateViewModel();
			forecastViewModelFactory.Stub(x => x.Evaluate(evaluateInput)).Return(workloadForecastingViewModel);
			var target = new ForecastController(null, null, forecastViewModelFactory, null, null, new BasicActionThrottler(), null, null, null, null, null, null);

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
			var target = new ForecastController(null, null, forecastViewModelFactory, null, null, new BasicActionThrottler(), null, null, null, null, null, null);

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
			var target = new ForecastController(null, null, null, forecastResultViewModelFactory, null, new BasicActionThrottler(), scenarioRepository, null, null, null, null, null);

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
			var target = new ForecastController(null, null, null, null, intradayPatternViewModelFactory, new BasicActionThrottler(), null, null, null, null, null, null);
			
			var result = target.IntradayPattern(input);

			result.Result.Should().Be.EqualTo(intradayPatternViewModel);
		}

		[Test]
		public void ShouldAddCampaign()
		{
			var input = new CampaignInput
			{
				Days = new[] {new ModifiedDay {Date = new DateTime()}},
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				CampaignTasksPercent = 50
			};
			var scenario = new Scenario("default").WithId(input.ScenarioId);
			var campaignPersister = MockRepository.GenerateMock<ICampaignPersister>();
			var scenarioRepository = new FakeScenarioRepository(scenario);
			var workloadRepository = new FakeWorkloadRepository();
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill")).WithId(input.WorkloadId);
			workloadRepository.Add(workload);
			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, campaignPersister, null, null, null);

			var result = target.AddCampaign(input);
			result.Result.Success.Should().Be.True();
			campaignPersister.AssertWasCalled(x => x.Persist(scenario, workload, input.Days, input.CampaignTasksPercent));
		}

		[Test]
		public void ShouldSetOverrideValues()
		{
			var input = new OverrideInput
			{
				Days = new[] {new ModifiedDay {Date = new DateTime()}},
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
			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null, null);

			var result = target.Override(input);
			result.Result.Success.Should().Be.True();

			overrideTasksPersister.AssertWasCalled(x => x.Persist(scenario, workload, input));
		}

		[Test]
		public void ShouldClearOverrideValues()
		{
			var input = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = new DateTime() } },
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
			var overrideTarget = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null, null);
			var overrideResult = overrideTarget.Override(input);
			overrideResult.Result.Success.Should().Be.True();
			overrideTasksPersister.AssertWasCalled(x => x.Persist(scenario, workload, input));

			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null, null);

			var clearInput = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = new DateTime() } },
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
