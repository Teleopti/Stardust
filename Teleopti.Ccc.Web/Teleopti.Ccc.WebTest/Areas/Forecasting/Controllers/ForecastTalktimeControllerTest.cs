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
	public class ForecastTalkTimeControllerTest : IExtendSystem
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
		public void ShouldForecastDaysWithDifferentTaskTime()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			const int dayInAWeek = 7;

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-dayInAWeek+1).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 100
				},
				new StatisticTask
				{
					Interval = openDay.AddDays(-dayInAWeek).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 400
				}
			});

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date.AddDays(1),
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedDay1 = result.Content.ForecastDays.First().TotalAverageTaskTime;
			var forecastedDay2 = result.Content.ForecastDays.Last().TotalAverageTaskTime;
			forecastedDay1.Should().Not.Be.EqualTo(forecastedDay2);
			Assert.AreNotEqual(Math.Round(forecastedDay1, 3), Math.Round(forecastedDay2, 3));
		}

		[Test]
		public void ShouldForecastByRemovingHighOutlier()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var friday = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			var statisticTasks = new List<IStatisticTask>();
			var random = new Random();
			for (var i = 1; i <= 30; i++)
			{
				statisticTasks.Add(new StatisticTask
				{
					Interval = friday.AddDays(-i).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = random.Next(100, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(100, 200)
				});
			}

			var lastFriday = statisticTasks.Single(x => new DateOnly(x.Interval.Date) == friday.AddDays(-7));
			lastFriday.StatAverageTaskTimeSeconds = 5000;
			lastFriday.StatAverageAfterTaskTimeSeconds = 5000;

			StatisticRepository.Has(workload.QueueSourceCollection.First(), statisticTasks);

			var forecastInput = new ForecastInput
			{
				ForecastStart = friday.Date,
				ForecastEnd = friday.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			// Forecasted value is 997.252 when StatAverageTaskTimeSeconds,StatAverageAfterTaskTimeSeconds  in all historical data is 100 and outliers were not removed
			forecastedTaskTime.Should().Be.GreaterThan(997);
			forecastedAfterTaskTime.Should().Be.GreaterThan(997);
			// Forecasted value is 1400 when StatAverageTaskTimeSeconds, StatAverageAfterTaskTimeSeconds in all historical data is 200 and outliers were not removed
			forecastedTaskTime.Should().Be.LessThan(1400);
			forecastedAfterTaskTime.Should().Be.LessThan(1400);
		}

		[Test]
		public void ShouldForecastWithNoOutliers()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var friday = new DateOnly(2018, 05, 04);

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			var statisticTasks = new List<IStatisticTask>();
			var random = new Random();
			for (var i = 1; i <= 30; i++)
			{
				statisticTasks.Add(new StatisticTask
				{
					Interval = friday.AddDays(-i).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = random.Next(100, 200),
					StatAverageAfterTaskTimeSeconds = random.Next(100, 200)
				});
			}

			var lastFriday = statisticTasks.Single(x => new DateOnly(x.Interval.Date) == friday.AddDays(-7));
			lastFriday.StatAverageTaskTimeSeconds = 260;
			lastFriday.StatAverageAfterTaskTimeSeconds = 260;

			StatisticRepository.Has(workload.QueueSourceCollection.First(), statisticTasks);

			var forecastInput = new ForecastInput
			{
				ForecastStart = friday.Date,
				ForecastEnd = friday.Date,
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedTaskTime = result.Content.ForecastDays.Single().AverageTaskTime;
			var forecastedAfterTaskTime = result.Content.ForecastDays.Single().AverageAfterTaskTime;

			// Forecasted value is 129.298 when StatAverageTaskTimeSeconds in all historical data is 100 and outliers were not removed
			forecastedTaskTime.Should().Be.GreaterThan(129);
			forecastedAfterTaskTime.Should().Be.GreaterThan(129);
			// Forecasted value is 210.98 when StatAverageTaskTimeSeconds in all historical data is 200 and outliers were not removed
			forecastedTaskTime.Should().Be.LessThan(211);
			forecastedAfterTaskTime.Should().Be.LessThan(211);
		}

		[Test]
		public void ShouldForecastDaysWithDifferentAfterTaskTime()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			const int dayInAWeek = 7;

			SkillRepository.Add(skill);
			ScenarioRepository.Has(scenario);
			WorkloadRepository.Add(workload);

			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-dayInAWeek+1).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 100,
					StatAverageAfterTaskTimeSeconds = 100,
				},
				new StatisticTask
				{
					Interval = openDay.AddDays(-dayInAWeek).Date.AddHours(10),
					StatOfferedTasks = 10,
					StatAverageTaskTimeSeconds = 400,
					StatAverageAfterTaskTimeSeconds = 400
				}
			});

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date.AddDays(1),
				ScenarioId = scenario.Id.Value,
				WorkloadId = workload.Id.Value
			};
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.Forecast(forecastInput);
			var forecastedDay1 = result.Content.ForecastDays.First().TotalAverageAfterTaskTime;
			var forecastedDay2 = result.Content.ForecastDays.Last().TotalAverageAfterTaskTime;
			forecastedDay1.Should().Not.Be.EqualTo(forecastedDay2);
			Assert.AreNotEqual(Math.Round(forecastedDay1, 3), Math.Round(forecastedDay2, 3));
		}
	}
}
