using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	public class ForecastCreateControllerTest : IExtendSystem
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
		public void ShouldHaveClosedAndOpenDayWhenForecasting()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var closedDay = new DateOnly(2018, 05, 05);
			var workloadDayTemplate = new WorkloadDayTemplate();
			workloadDayTemplate.Create(openDay.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload,
				new List<TimePeriod> { new TimePeriod(10, 12) });
			workload.SetTemplate(openDay.Date.DayOfWeek, workloadDayTemplate);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = closedDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);

			result.Should().Be.OfType<OkNegotiatedContentResult<ForecastViewModel>>();
			var forecastDays = result.Content.ForecastDays;
			forecastDays.Count.Should().Be(2);
			forecastDays.First().IsOpen.Should().Be.True();
			forecastDays.First().IsInModification.Should().Be.True();
			forecastDays.First().IsForecasted.Should().Be.True();
			forecastDays.Last().IsOpen.Should().Be.False();
			forecastDays.Last().IsInModification.Should().Be.False();
			forecastDays.Last().IsForecasted.Should().Be.True();
		}


		[Test]
		public void ShouldForecastUsingExistingCampaign()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());
			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.CampaignTasks = new Percent(0.5);
			workloadDay.CampaignTaskTime = new Percent(0.6);
			workloadDay.CampaignAfterTaskTime = new Percent(0.7);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
			forecastDay.HasOverride.Should().Be.False();
			forecastDay.HasCampaign.Should().Be.True();
			forecastDay.CampaignTasksPercentage.Should().Be(new Percent(0.5).Value);
			forecastDay.TotalTasks.Should().Be(forecastDay.Tasks * 1.5);
			forecastDay.TotalAverageTaskTime.Should().Be(forecastDay.AverageTaskTime * 1.6);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(forecastDay.AverageAfterTaskTime * 1.7);
			forecastDay.IsInModification.Should().Be(true);
		}

		[Test]
		public void ShouldForecastUsingExistingOverride()
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

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
			forecastDay.HasCampaign.Should().Be.False();
			forecastDay.HasOverride.Should().Be.True();
			forecastDay.IsInModification.Should().Be.True();
			Assert.That(forecastDay.TotalTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
			Assert.That(forecastDay.OverrideTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
		}

		[Test]
		public void ShouldForecastUsingExistingCampaignAndOverride()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());

			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.CampaignTasks = new Percent(0.5);
			workloadDay.CampaignTaskTime = new Percent(0.6);
			workloadDay.CampaignAfterTaskTime = new Percent(0.7);

			ForecastDayOverrideRepository.Add(new ForecastDayOverride(openDay, workload, scenario)
			{
				OriginalTasks = workloadDay.Tasks,
				OriginalAverageTaskTime = workloadDay.AverageTaskTime,
				OriginalAverageAfterTaskTime = workloadDay.AverageAfterTaskTime,
				OverriddenTasks = 100,
				OverriddenAverageTaskTime = TimeSpan.FromSeconds(150),
				OverriddenAverageAfterTaskTime = TimeSpan.FromSeconds(200)
			});

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
			forecastDay.HasCampaign.Should().Be.True();
			forecastDay.HasOverride.Should().Be.True();
			forecastDay.CampaignTasksPercentage.Should().Be(new Percent(0.5).Value);
			forecastDay.IsInModification.Should().Be(true);
			Assert.That(forecastDay.TotalTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
			Assert.That(forecastDay.OverrideTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
		}

		[Test]
		public void ShouldForecastDaysWithDifferentHandleTime()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var date = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = date.AddDays(-6).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 100,
					StatAverageAfterTaskTimeSeconds = 100,
				},
				new StatisticTask
				{
					Interval = date.AddDays(-7).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 400,
					StatAverageAfterTaskTimeSeconds = 400
				}
			});

			var forecastInput = new ForecastInput
			{
				ForecastStart = date.Date,
				ForecastEnd = date.Date.AddDays(1),
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var taskTimeDay1 = result.Content.ForecastDays.First().TotalAverageTaskTime;
			var taskTimeDay2 = result.Content.ForecastDays.Last().TotalAverageTaskTime;
			var afterTaskTimeDay1 = result.Content.ForecastDays.First().TotalAverageAfterTaskTime;
			var afterTaskTimeDay2 = result.Content.ForecastDays.Last().TotalAverageAfterTaskTime;
			taskTimeDay1.Should().Not.Be.EqualTo(taskTimeDay2);
			afterTaskTimeDay1.Should().Not.Be.EqualTo(afterTaskTimeDay2);
			Assert.AreNotEqual(Math.Round(taskTimeDay1, 3), Math.Round(taskTimeDay2, 3));
			Assert.AreNotEqual(Math.Round(afterTaskTimeDay1, 3), Math.Round(afterTaskTimeDay2, 3));
		}

		[Test]
		public void ShouldForecastByRemovingHighOutlier()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var date = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			var statisticTasks = new List<IStatisticTask>();
			var random = new Random();
			for (var i = 1; i <= 30; i++)
			{
				statisticTasks.Add(new StatisticTask
				{
					Interval = date.AddDays(-i).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = random.Next(100, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(100, 200)
				});
			}

			var outlierDay = statisticTasks.Single(x => new DateOnly(x.Interval.Date) == date.AddDays(-7));
			outlierDay.StatAverageTaskTimeSeconds = 5000;
			outlierDay.StatAverageAfterTaskTimeSeconds = 5000;

			StatisticRepository.Has(workload.QueueSourceCollection.First(), statisticTasks);

			var forecastInput = new ForecastInput
			{
				ForecastStart = date.Date,
				ForecastEnd = date.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			forecastedTaskTime.Should().Be.GreaterThanOrEqualTo(997);
			forecastedAfterTaskTime.Should().Be.GreaterThanOrEqualTo(997);
			forecastedTaskTime.Should().Be.LessThanOrEqualTo(1400);
			forecastedAfterTaskTime.Should().Be.LessThanOrEqualTo(1400);
		}

		[Test]
		public void ShouldForecastByRemovingLowOutlier()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var date = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			var statisticTasks = new List<IStatisticTask>();
			var random = new Random();
			for (var i = 1; i <= 30; i++)
			{
				statisticTasks.Add(new StatisticTask
				{
					Interval = date.AddDays(-i).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = random.Next(190, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(190, 200)
				});
			}
			
			var outlierDay = statisticTasks.Single(x => new DateOnly(x.Interval.Date) == date.AddDays(-7));
			outlierDay.StatAverageTaskTimeSeconds = 10;
			outlierDay.StatAverageAfterTaskTimeSeconds = 10;

			StatisticRepository.Has(workload.QueueSourceCollection.First(), statisticTasks);

			var forecastInput = new ForecastInput
			{
				ForecastStart = date.Date,
				ForecastEnd = date.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			forecastedTaskTime.Should().Be.GreaterThanOrEqualTo(145);
			forecastedAfterTaskTime.Should().Be.GreaterThanOrEqualTo(145);
			forecastedTaskTime.Should().Be.LessThanOrEqualTo(166);
			forecastedAfterTaskTime.Should().Be.LessThanOrEqualTo(166);
		}

		[Test]
		public void ShouldForecastWithNoOutliers()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var date = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			var statisticTasks = new List<IStatisticTask>();
			var random = new Random();
			for (var i = 1; i <= 30; i++)
			{
				statisticTasks.Add(new StatisticTask
				{
					Interval = date.AddDays(-i).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = random.Next(190, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(190, 200)
				});
			}

			var outlierDay = statisticTasks.Single(x => new DateOnly(x.Interval.Date) == date.AddDays(-7));
			outlierDay.StatAverageTaskTimeSeconds = 260;
			outlierDay.StatAverageAfterTaskTimeSeconds = 260;

			StatisticRepository.Has(workload.QueueSourceCollection.First(), statisticTasks);

			var forecastInput = new ForecastInput
			{
				ForecastStart = date.Date,
				ForecastEnd = date.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			forecastedTaskTime.Should().Be.GreaterThanOrEqualTo(200);
			forecastedAfterTaskTime.Should().Be.GreaterThanOrEqualTo(200);
			forecastedTaskTime.Should().Be.LessThanOrEqualTo(211);
			forecastedAfterTaskTime.Should().Be.LessThanOrEqualTo(211);
		}

		[Test]
		public void ShouldForecastAndGetCorrectTotals()
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

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastViewModel>)Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
			forecastDay.HasOverride.Should().Be.False();
			forecastDay.HasCampaign.Should().Be.False();
 			forecastDay.TotalTasks.Should().Be(forecastDay.Tasks);
			forecastDay.TotalAverageTaskTime.Should().Be(forecastDay.AverageTaskTime);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(forecastDay.AverageAfterTaskTime);
			forecastDay.IsInModification.Should().Be(true);
		}

	}
}
