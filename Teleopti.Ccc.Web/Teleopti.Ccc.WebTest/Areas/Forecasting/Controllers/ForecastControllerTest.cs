using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
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
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var skill1 = SkillFactory.CreateSkillWithWorkloadAndSources();
			skill1.SetId(Guid.NewGuid());
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new[] {skill1});
			var target = new ForecastController(null, skillRepository, null, null, null, new BasicActionThrottler(), null, null, null, null, principalAuthorization);
			var result = target.Skills();
			result.Skills.Single().Id.Should().Be.EqualTo(skill1.Id.Value);
			result.Skills.Single().Name.Should().Be.EqualTo(skill1.Name);
			var workload = skill1.WorkloadCollection.Single();
			result.Skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			result.Skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workload.Name);
		}

		[Test]
		public void ShouldHavePermissionForModifySkill()
		{
			var principalAuthorization = MockRepository.GenerateMock<IPrincipalAuthorization>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();

			principalAuthorization.Stub(x => x.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkill)).Return(true);
			skillRepository.Stub(x => x.FindSkillsWithAtLeastOneQueueSource()).Return(new List<ISkill>());

			var target = new ForecastController(null, skillRepository, null, null, null, new BasicActionThrottler(), null, null, null, null, principalAuthorization);
			var result = target.Skills();
			result.IsPermittedToModifySkill.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldGetScenarios()
		{
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var scenario = new Scenario("scenario1");
			scenario.SetId(Guid.NewGuid());
			scenarioRepository.Stub(x => x.FindAllSorted()).Return(new IScenario[] { scenario });;
			var target = new ForecastController(null, null, null, null, null, null, scenarioRepository, null, null, null, null);
			var result = target.Scenarios();
			result.Single().Id.Should().Be.EqualTo(scenario.Id.Value);
			result.Single().Name.Should().Be.EqualTo("scenario1");
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
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var scenario = new Scenario("test1");
			scenario.SetId(scenarioId);
			scenarioRepository.Stub(x => x.Get(forecastInput.ScenarioId)).Return(scenario);
			var target = new ForecastController(forecastCreator, null, null, null, null, new BasicActionThrottler(), scenarioRepository, null, null, null, null);
			var result = target.Forecast(forecastInput);
			result.Result.Success.Should().Be.True();
			result.Result.BlockToken.Should().Be.EqualTo(forecastInput.BlockToken);
			forecastCreator.AssertWasCalled(x => x.CreateForecastForWorkloads(new DateOnlyPeriod(new DateOnly(forecastInput.ForecastStart), new DateOnly(forecastInput.ForecastEnd)), forecastInput.Workloads, scenario));
		}

		[Test]
		public void ShouldFinishToken()
		{
			var actionThrottler = MockRepository.GenerateMock<IActionThrottler>();
			var target = new ForecastController(MockRepository.GenerateMock<IForecastCreator>(), null, null, null, null, actionThrottler, MockRepository.GenerateMock<IScenarioRepository>(), null, null, null, null);

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
			var target = new ForecastController(null, null, null, null, null, actionThrottler, null, null, null, null, null);

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
			var target = new ForecastController(MockRepository.GenerateMock<IForecastCreator>(), null, null, null, null, actionThrottler, MockRepository.GenerateMock<IScenarioRepository>(), null, null, null, null);

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
			var target = new ForecastController(MockRepository.GenerateMock<IForecastCreator>(), null, null, null, null, actionThrottler, MockRepository.GenerateMock<IScenarioRepository>(), null, null, null, null);

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
			var target = new ForecastController(null, null, forecastViewModelFactory, null, null, new BasicActionThrottler(), null, null, null, null, null);

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
			var target = new ForecastController(null, null, forecastViewModelFactory, null, null, new BasicActionThrottler(), null, null, null, null, null);

			var result = target.QueueStatistics(queueStatisticsInput);

			result.Result.Should().Be.EqualTo(workloadQueueStatisticsViewModel);
		}

		[Test]
		public void ShouldGetForecastResult()
		{
			var workloadId = Guid.NewGuid();
			var scenario = new Scenario("test1");
			scenario.SetId(Guid.NewGuid());
			var forecastStart = new DateTime(2014,4,1);
			var forecastEnd = new DateTime(2014,4,29);
			var forecastResultViewModelFactory = MockRepository.GenerateMock<IForecastResultViewModelFactory>();
			var workloadForecastResultViewModel = new WorkloadForecastResultViewModel();
			forecastResultViewModelFactory.Stub(
				x => x.Create(workloadId, new DateOnlyPeriod(new DateOnly(forecastStart), new DateOnly(forecastEnd)), scenario))
				.Return(workloadForecastResultViewModel);
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(scenario.Id.GetValueOrDefault())).Return(scenario);
			var target = new ForecastController(null, null, null, forecastResultViewModelFactory, null, new BasicActionThrottler(), scenarioRepository, null, null, null, null);

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
			var target = new ForecastController(null, null, null, null, intradayPatternViewModelFactory, new BasicActionThrottler(), null, null, null, null, null);
			
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
			var campaignPersister = MockRepository.GenerateMock<ICampaignPersister>();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var scenario = new Scenario("default");
			scenarioRepository.Stub(x => x.Get(input.ScenarioId)).Return(scenario);
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill"));
			workloadRepository.Stub(x => x.Get(input.WorkloadId))
				.Return(workload);
			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, campaignPersister, null, null);

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
			var overrideTasksPersister = MockRepository.GenerateMock<IOverridePersister>();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var workloadRepository = MockRepository.GenerateMock<IWorkloadRepository>();
			var scenario = new Scenario("default");
			scenarioRepository.Stub(x => x.Get(input.ScenarioId)).Return(scenario);
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill"));
			workloadRepository.Stub(x => x.Get(input.WorkloadId))
				.Return(workload);
			var target = new ForecastController(null, null, null, null, null, new BasicActionThrottler(), scenarioRepository, workloadRepository, null, overrideTasksPersister, null);

			var result = target.Override(input);
			result.Result.Success.Should().Be.True();

			overrideTasksPersister.AssertWasCalled(x => x.Persist(scenario, workload, input));
		}
	}
}
