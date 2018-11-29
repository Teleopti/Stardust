using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;


namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastLoadControllerTest : IExtendSystem
	{
		public ForecastController Target;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeStatisticRepository StatisticRepository;
		public FakeWorkloadRepository WorkloadRepository;
		public FakeForecastDayOverrideRepository ForecastDayOverrideRepository;
		private const double tolerance = 0.000001d;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ForecastController>();
		}
		
		[Test]
		public void ShouldLoadForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			var workloadDay = skillDay.WorkloadDayCollection.Single();
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());

			var closedDay = new DateOnly(2018, 05, 05);
			var skillDayClosed = SkillDayFactory.CreateSkillDay(skill, workload, closedDay, scenario, false);
			var workloadDayClosed = skillDayClosed.WorkloadDayCollection.Single();
			skillDayClosed.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDayClosed }, new DateOnlyPeriod());

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);
			SkillDayRepository.Add(skillDayClosed);

			var forecastResultInput = new ForecastResultInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = closedDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			result.Content.ForecastDays.Count.Should().Be(2);

			var forecastDay = result.Content.ForecastDays.First();
			Assert.That(forecastDay.Tasks, Is.EqualTo(workloadDay.Tasks).Within(tolerance));
			Assert.That(forecastDay.AverageTaskTime, Is.EqualTo(workloadDay.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay.AverageAfterTaskTime,
				Is.EqualTo(workloadDay.AverageAfterTaskTime.TotalSeconds).Within(tolerance));

			Assert.That(forecastDay.TotalTasks, Is.EqualTo(workloadDay.TotalTasks).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime,
				Is.EqualTo(workloadDay.TotalAverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime,
				Is.EqualTo(workloadDay.TotalAverageAfterTaskTime.TotalSeconds).Within(tolerance));

			forecastDay.HasCampaign.Should().Be.False();
			forecastDay.HasOverride.Should().Be.False();
			forecastDay.IsOpen.Should().Be.True();
			forecastDay.IsInModification.Should().Be.False();

			var forecastDayClosed = result.Content.ForecastDays.Last();
			Assert.That(forecastDayClosed.Tasks, Is.EqualTo(workloadDayClosed.Tasks).Within(tolerance));
			Assert.That(forecastDayClosed.AverageTaskTime, Is.EqualTo(workloadDayClosed.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDayClosed.AverageAfterTaskTime,
				Is.EqualTo(workloadDayClosed.AverageAfterTaskTime.TotalSeconds).Within(tolerance));

			Assert.That(forecastDayClosed.TotalTasks, Is.EqualTo(workloadDayClosed.TotalTasks).Within(tolerance));
			Assert.That(forecastDayClosed.TotalAverageTaskTime,
				Is.EqualTo(workloadDayClosed.TotalAverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDayClosed.TotalAverageAfterTaskTime,
				Is.EqualTo(workloadDayClosed.TotalAverageAfterTaskTime.TotalSeconds).Within(tolerance));

			forecastDayClosed.HasCampaign.Should().Be.False();
			forecastDayClosed.HasOverride.Should().Be.False();
			forecastDayClosed.IsOpen.Should().Be.False();
			forecastDayClosed.IsInModification.Should().Be.False();
		}

		[Test]
		public void ShouldLoadForecastWithCampaign()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var negativeCampaignDay = new DateOnly(2018, 05, 05);
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, workload, negativeCampaignDay, scenario);
			skillDay1.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay1 }, new DateOnlyPeriod());
			skillDay2.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay2 }, new DateOnlyPeriod());

			var workloadDay1 = skillDay1.WorkloadDayCollection.Single();
			workloadDay1.CampaignTasks = new Percent(0.5d);
			workloadDay1.CampaignTaskTime = new Percent(0.5d);
			workloadDay1.CampaignAfterTaskTime = new Percent(0.5d);

			var workloadDay2 = skillDay2.WorkloadDayCollection.Single();
			workloadDay2.CampaignTasks = new Percent(-0.5d);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var forecastResultInput = new ForecastResultInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = negativeCampaignDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			var forecastDay1 = result.Content.ForecastDays.First();
			var forecastDay2 = result.Content.ForecastDays.Last();
			Assert.That(forecastDay1.Tasks, Is.EqualTo(workloadDay1.Tasks).Within(tolerance));
			Assert.That(forecastDay1.AverageTaskTime, Is.EqualTo(workloadDay1.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay1.AverageAfterTaskTime,
				Is.EqualTo(workloadDay1.AverageAfterTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay2.Tasks, Is.EqualTo(workloadDay2.Tasks).Within(tolerance));
			Assert.That(forecastDay2.AverageTaskTime, Is.EqualTo(workloadDay2.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay2.AverageAfterTaskTime,
				Is.EqualTo(workloadDay2.AverageAfterTaskTime.TotalSeconds).Within(tolerance));

			forecastDay1.HasCampaign.Should().Be.True();
			forecastDay1.HasOverride.Should().Be.False();
			forecastDay2.HasCampaign.Should().Be.True();
			forecastDay2.HasOverride.Should().Be.False();

			Assert.That(forecastDay1.TotalTasks,
				Is.EqualTo((1 + workloadDay1.CampaignTasks.Value) * workloadDay1.Tasks).Within(tolerance));
			Assert.That(forecastDay1.TotalAverageTaskTime,
				Is.EqualTo((1 + workloadDay1.CampaignTaskTime.Value) * workloadDay1.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay1.TotalAverageAfterTaskTime,
				Is.EqualTo((1 + workloadDay1.CampaignAfterTaskTime.Value) * workloadDay1.AverageAfterTaskTime.TotalSeconds)
					.Within(tolerance));
			Assert.That(forecastDay2.TotalTasks,
				Is.EqualTo((1 + workloadDay2.CampaignTasks.Value) * workloadDay2.Tasks).Within(tolerance));
			Assert.That(forecastDay2.TotalAverageTaskTime,
				Is.EqualTo((1 + workloadDay2.CampaignTaskTime.Value) * workloadDay2.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay2.TotalAverageAfterTaskTime,
				Is.EqualTo((1 + workloadDay2.CampaignAfterTaskTime.Value) * workloadDay2.AverageAfterTaskTime.TotalSeconds)
					.Within(tolerance));
		}

		[Test]
		public void ShouldLoadForecastWithOverride()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);
			ForecastDayOverrideRepository.Add(new ForecastDayOverride(openDay, workload, scenario)
			{
				OriginalTasks = 10,
				OriginalAverageTaskTime = TimeSpan.FromSeconds(60),
				OriginalAverageAfterTaskTime = TimeSpan.FromSeconds(60),
				OverriddenTasks = 100,
				OverriddenAverageTaskTime = TimeSpan.FromSeconds(150),
				OverriddenAverageAfterTaskTime = TimeSpan.FromSeconds(200)
			});

			var forecastResultInput = new ForecastResultInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			var forecastDay = result.Content.ForecastDays.Single();
			Assert.That(forecastDay.Tasks, Is.EqualTo(10).Within(tolerance));
			Assert.That(forecastDay.AverageTaskTime, Is.EqualTo(60).Within(tolerance));
			Assert.That(forecastDay.AverageAfterTaskTime,
				Is.EqualTo(60).Within(tolerance));

			forecastDay.HasCampaign.Should().Be.False();
			forecastDay.HasOverride.Should().Be.True();

			Assert.That(forecastDay.TotalTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));

			Assert.That(forecastDay.OverrideTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
		}

		[Test]
		public void ShouldLoadForecastWithBothCampaignAndOverride()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());

			var workloadDay = skillDay.WorkloadDayCollection.Single();

			ForecastDayOverrideRepository.Add(new ForecastDayOverride(openDay, workload, scenario)
			{
				OriginalTasks = workloadDay.Tasks,
				OriginalAverageTaskTime = workloadDay.AverageTaskTime,
				OriginalAverageAfterTaskTime = workloadDay.AverageAfterTaskTime,
				OverriddenTasks = 100,
				OverriddenAverageTaskTime = TimeSpan.FromSeconds(150),
				OverriddenAverageAfterTaskTime = TimeSpan.FromSeconds(200)
			});

			workloadDay.CampaignTasks = new Percent(0.5d);
			workloadDay.CampaignTaskTime = new Percent(0.5d);
			workloadDay.CampaignAfterTaskTime = new Percent(0.5d);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			var forecastResultInput = new ForecastResultInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			var forecastDay = result.Content.ForecastDays.Single();
			Assert.That(forecastDay.Tasks, Is.EqualTo(workloadDay.Tasks).Within(tolerance));
			Assert.That(forecastDay.AverageTaskTime, Is.EqualTo(workloadDay.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay.AverageAfterTaskTime,
				Is.EqualTo(workloadDay.AverageAfterTaskTime.TotalSeconds).Within(tolerance));

			forecastDay.HasCampaign.Should().Be.True();
			forecastDay.HasOverride.Should().Be.True();

			Assert.That(forecastDay.TotalTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));

			Assert.That(forecastDay.OverrideTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
		}
		
		[Test]
		public void ShouldFillGapsInsideForecastPeriod()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var firstDay = new DateOnly(2018, 05, 04);
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, workload, firstDay, scenario);
			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, workload, firstDay.AddDays(2), scenario);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var forecastResultInput = new ForecastResultInput
			{
				ForecastStart = firstDay.AddDays(-2).Date,
				ForecastEnd = firstDay.AddDays(4).Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};

			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			result.Content.ForecastDays.Count.Should().Be(3);

			result.Content.ForecastDays[0].Date.Should().Be(new DateOnly(2018, 05, 04));
			result.Content.ForecastDays[1].Date.Should().Be(new DateOnly(2018, 05, 05));
			result.Content.ForecastDays[2].Date.Should().Be(new DateOnly(2018, 05, 06));

			result.Content.ForecastDays[0].IsForecasted.Should().Be.True();
			result.Content.ForecastDays[1].IsForecasted.Should().Be.False();
			result.Content.ForecastDays[2].IsForecasted.Should().Be.True();
		}

		[Test]
		public void ShouldFillSelectedPeriodWhereNoForecast()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var firstForecastedDay = new DateOnly(2018, 05, 04);
			var skillDay1 = SkillDayFactory.CreateSkillDay(skill, workload, firstForecastedDay, scenario);
			var skillDay2 = SkillDayFactory.CreateSkillDay(skill, workload, firstForecastedDay.AddDays(2), scenario);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay1);
			SkillDayRepository.Add(skillDay2);

			var forecastResultInput = new ForecastResultInput
			{
				ForecastStart = firstForecastedDay.AddDays(-1).Date,
				ForecastEnd = firstForecastedDay.AddDays(3).Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value,
				HasUserSelectedPeriod = true
			};

			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			result.Content.ForecastDays.Count.Should().Be(5);

			result.Content.ForecastDays[0].Date.Should().Be(new DateOnly(2018, 05, 03));
			result.Content.ForecastDays[1].Date.Should().Be(new DateOnly(2018, 05, 04));
			result.Content.ForecastDays[2].Date.Should().Be(new DateOnly(2018, 05, 05));
			result.Content.ForecastDays[3].Date.Should().Be(new DateOnly(2018, 05, 06));
			result.Content.ForecastDays[4].Date.Should().Be(new DateOnly(2018, 05, 07));

			result.Content.ForecastDays[0].IsForecasted.Should().Be.False();
			result.Content.ForecastDays[1].IsForecasted.Should().Be.True();
			result.Content.ForecastDays[2].IsForecasted.Should().Be.False();
			result.Content.ForecastDays[3].IsForecasted.Should().Be.True();
			result.Content.ForecastDays[4].IsForecasted.Should().Be.False();
		}
	}
}