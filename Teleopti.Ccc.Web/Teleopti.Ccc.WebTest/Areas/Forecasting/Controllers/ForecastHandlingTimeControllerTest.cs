using System;
using System.Collections.Generic;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastHandlingTimeControllerTest : IExtendSystem
	{
		public ForecastController Target;
		public FakeSkillRepository SkillRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeStatisticRepository StatisticRepository;
		public FakeWorkloadRepository WorkloadRepository;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ForecastController>();
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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			// 997 is the forecasted value when statistics time is 100 for all days in history with no outliers removed
			// 1400 is the forecasted value when statistics time is 200 for all days in history with no outliers removed
			forecastedTaskTime.Should().Be.GreaterThan(997);
			forecastedAfterTaskTime.Should().Be.GreaterThan(997);
			forecastedTaskTime.Should().Be.LessThan(1400);
			forecastedAfterTaskTime.Should().Be.LessThan(1400);
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
					StatAverageTaskTimeSeconds = random.Next(100, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(100, 200)
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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			// 83 is the forecasted value when statistics time is 100 for all days in history with no outliers removed
			// 166 is the forecasted value when statistics time is 200 for all days in history with no outliers removed
			forecastedTaskTime.Should().Be.GreaterThan(83);
			forecastedAfterTaskTime.Should().Be.GreaterThan(83);
			forecastedTaskTime.Should().Be.LessThan(166);
			forecastedAfterTaskTime.Should().Be.LessThan(166);
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
					StatAverageTaskTimeSeconds = random.Next(100, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(100, 200)
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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			// 129 is the forecasted value when statistics time is 100 for all days in history with no outliers removed
			// 211 is the forecasted value when statistics time is 200 for all days in history with no outliers removed
			forecastedTaskTime.Should().Be.GreaterThan(129);
			forecastedAfterTaskTime.Should().Be.GreaterThan(129);
			forecastedTaskTime.Should().Be.LessThan(211);
			forecastedAfterTaskTime.Should().Be.LessThan(211);
		}
	}
}
