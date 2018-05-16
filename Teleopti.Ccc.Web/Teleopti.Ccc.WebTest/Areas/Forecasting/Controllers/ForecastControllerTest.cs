using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastControllerTest : IExtendSystem
	{
		private const double tolerance = 0.000001d;
		public ForecastController Target;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeSkillRepository SkillRepository;
		public FakeWorkloadRepository WorkloadRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakeStatisticRepository StatisticRepository;
		public ForecastProvider ForecastProvider;

		public void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddService<ForecastController>();
		}

		[Test]
		public void ShouldSaveSimpleForecast()
		{
			var forecastedDay1 = new DateOnly(2018, 05, 02);
			var forecastedDay2 = forecastedDay1.AddDays(1);
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);

			var workloadDayTemplate1 = new WorkloadDayTemplate();
			var workloadDayTemplate2 = new WorkloadDayTemplate();
			workloadDayTemplate1.Create(forecastedDay1.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload, new List<TimePeriod>
			{
				new TimePeriod(10, 12)
			});
			workloadDayTemplate2.Create(forecastedDay2.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload, new List<TimePeriod>
			{
				new TimePeriod(11, 14)
			});
			workload.SetTemplate(forecastedDay1.Date.DayOfWeek, workloadDayTemplate1);
			workload.SetTemplate(forecastedDay2.Date.DayOfWeek, workloadDayTemplate2);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay1,
					Tasks = 10,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60
				},
				new ForecastDayModel
				{
					Date = forecastedDay2,
					Tasks = 15,
					AverageTaskTime = 65,
					AverageAfterTaskTime = 65
				}
			};
			var forecastResult = new ForecastModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay1 = SkillDayRepository.FindRange(forecastedDay1.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedForecastDay2 = SkillDayRepository.FindRange(forecastedDay2.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay1 = savedForecastDay1.WorkloadDayCollection.Single();
			var savedWorkloadDay2 = savedForecastDay2.WorkloadDayCollection.Single();

			savedWorkloadDay1.Tasks.Should().Be(forecastDays.First().Tasks);
			savedWorkloadDay1.AverageTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageTaskTime);
			savedWorkloadDay1.AverageAfterTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageAfterTaskTime);

			savedWorkloadDay2.Tasks.Should().Be(forecastDays.Last().Tasks);
			savedWorkloadDay2.AverageTaskTime.TotalSeconds.Should().Be(forecastDays.Last().AverageTaskTime);
			savedWorkloadDay2.AverageAfterTaskTime.TotalSeconds.Should().Be(forecastDays.Last().AverageAfterTaskTime);
		}

		[Test]
		public void ShouldSaveForecastWithIntradayPattern()
		{
			var forecastedDay = new DateOnly(2018, 05, 02);
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);

			var workloadDayTemplate = new WorkloadDayTemplate();
			workloadDayTemplate.Create(forecastedDay.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload, new List<TimePeriod>
			{
				new TimePeriod(10, 11)
			});
			workload.SetTemplate(forecastedDay.Date.DayOfWeek, workloadDayTemplate);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);

			var forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 200,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60
				}
			};

			var templateDay = forecastedDay.AddDays(-7).Date;
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = templateDay.AddHours(10),
					StatOfferedTasks = 15
				},
				new StatisticTask
				{
					Interval = templateDay.AddHours(10).AddMinutes(15),
					StatOfferedTasks = 25
				},
				new StatisticTask
				{
					Interval = templateDay.AddHours(10).AddMinutes(30),
					StatOfferedTasks = 55
				},
				new StatisticTask
				{
					Interval = templateDay.AddHours(10).AddMinutes(45),
					StatOfferedTasks = 5
				}
			});

			var forecastResult = new ForecastModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};

			var result = Target.ApplyForecast(forecastResult);
			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();

			var taskPeriods = savedWorkloadDay.TaskPeriodList;
			taskPeriods.Count.Should().Be(4);
			taskPeriods[0].Tasks.Should().Be.GreaterThan(0);
			taskPeriods[1].Tasks.Should().Be.GreaterThan(0);
			taskPeriods[2].Tasks.Should().Be.GreaterThan(0);
			taskPeriods[3].Tasks.Should().Be.GreaterThan(0);
			Assert.That(taskPeriods.Sum(x => x.Tasks), Is.EqualTo(200).Within(tolerance));
		}

		[Test]
		public void ShouldSaveForecastWithIntradayPatternWithOutAvialableHistory()
		{
			var forecastedDay = new DateOnly(2018, 05, 02);
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);

			var workloadDayTemplate = new WorkloadDayTemplate();
			workloadDayTemplate.Create(forecastedDay.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload, new List<TimePeriod>
			{
				new TimePeriod(10, 11)
			});
			workload.SetTemplate(forecastedDay.Date.DayOfWeek, workloadDayTemplate);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);

			var forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 200,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60
				}
			};

			

			var forecastResult = new ForecastModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};

			var result = Target.ApplyForecast(forecastResult);
			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var taskPeriods = savedForecastDay.WorkloadDayCollection.Single().TaskPeriodList;

			taskPeriods.Count.Should().Be(4);
			taskPeriods[0].Tasks.Should().Be.EqualTo(50);
			taskPeriods[1].Tasks.Should().Be.EqualTo(50);
			taskPeriods[2].Tasks.Should().Be.EqualTo(50);
			taskPeriods[3].Tasks.Should().Be.EqualTo(50);
		}

		[Test]
		public void ShouldNotSaveForecastOnClosedDay()
		{
			var forecastedDay1 = new DateOnly(2018, 05, 02);
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);

			var forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay1,
					Tasks = 10,
					AverageTaskTime = 0,
					AverageAfterTaskTime = 0
				}
			};

			var forecastResult = new ForecastModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};

			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			SkillDayRepository.FindRange(forecastedDay1.ToDateOnlyPeriod(), skill, scenario)
				.Single().WorkloadDayCollection.Single().TotalTasks.Should().Be(0);
		}

		[Test]
		public void ShouldSaveForecastWithCampaign()
		{
			var forecastedDay = new DateOnly(2018, 05, 02);
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);

			var workloadDayTemplate1 = new WorkloadDayTemplate();
			workloadDayTemplate1.Create(forecastedDay.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload, new List<TimePeriod>
			{
				new TimePeriod(10, 12)
			});
			workload.SetTemplate(forecastedDay.Date.DayOfWeek, workloadDayTemplate1);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 10,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60,
					HasCampaign = true,
					CampaignTasksPercentage = 0.5d
				}
			};
			var forecastResult = new ForecastModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();

			savedWorkloadDay.Tasks.Should().Be(forecastDays.First().Tasks);
			savedWorkloadDay.AverageTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageTaskTime);
			savedWorkloadDay.AverageAfterTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageAfterTaskTime);

			savedWorkloadDay.CampaignTasks.Should().Be.EqualTo(new Percent(0.5d));
		}

		[Test]
		public void ShouldSaveForecastWithOverride()
		{
			var forecastedDay = new DateOnly(2018, 05, 02);
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);

			var workloadDayTemplate1 = new WorkloadDayTemplate();
			workloadDayTemplate1.Create(forecastedDay.Date.DayOfWeek.ToString(), DateTime.UtcNow, workload, new List<TimePeriod>
			{
				new TimePeriod(10, 12)
			});
			workload.SetTemplate(forecastedDay.Date.DayOfWeek, workloadDayTemplate1);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 10,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60,
					HasOverride = true,
					OverrideTasks = 80,
					OverrideAverageTaskTime = 50,
					OverrideAverageAfterTaskTime = 20
				}
			};
			var forecastResult = new ForecastModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();

			savedWorkloadDay.Tasks.Should().Be(forecastDays.First().Tasks);
			savedWorkloadDay.AverageTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageTaskTime);
			savedWorkloadDay.AverageAfterTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageAfterTaskTime);

			Assert.That(savedWorkloadDay.OverrideTasks, Is.EqualTo(80d).Within(tolerance));
			Assert.That(savedWorkloadDay.OverrideAverageTaskTime, Is.EqualTo(TimeSpan.FromSeconds(50)).Within(tolerance));
			Assert.That(savedWorkloadDay.OverrideAverageAfterTaskTime, Is.EqualTo(TimeSpan.FromSeconds(20)).Within(tolerance));
		}

		[Test]
		public void ShouldAddCampaign()
		{
			var model = new CampaignInput
			{
				SelectedDays = new[] {new DateOnly(2018, 5, 4)},
				CampaignTasksPercent = 0.5d,
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						TotalTasks = 100d
					},
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 5),
						IsOpen = true,
						Tasks = 100d,
						TotalTasks = 100d
					}
				}
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>) Target.AddCampaign(model);

			result.Should().Be.OfType<OkNegotiatedContentResult<IList<ForecastDayModel>>>();

			var firstForecastDay = result.Content.First();
			firstForecastDay.HasCampaign.Should().Be.True();
			firstForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(model.CampaignTasksPercent);
			firstForecastDay.Tasks.Should().Be(100d);
			firstForecastDay.TotalTasks.Should().Be(150d);

			var secondForecastDay = result.Content.Last();
			secondForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(0);
			secondForecastDay.HasCampaign.Should().Be.False();
			secondForecastDay.Tasks.Should().Be(100d);
			secondForecastDay.TotalTasks.Should().Be(100d);
		}

		[Test]
		public void ShouldNotAddCampaignWhenExistsOverride()
		{
			var forecast = new ForecastDayModel
			{
				Date = new DateOnly(2018, 5, 4),
				IsOpen = true,
				Tasks = 100d,
				AverageTaskTime = 30d,
				AverageAfterTaskTime = 10d,
				TotalTasks = 200d,
				TotalAverageTaskTime = 40d,
				TotalAverageAfterTaskTime = 20d,
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 40d,
				OverrideAverageAfterTaskTime = 20d,
				HasOverride = true
			};
			var model = new CampaignInput
			{
				SelectedDays = new[] {new DateOnly(2018, 5, 4)},
				ForecastDays = new List<ForecastDayModel> { forecast },
				CampaignTasksPercent = 0.5d
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>)Target.AddCampaign(model);
			var forecastDay = result.Content.First();
			result.Should().Be.OfType<OkNegotiatedContentResult<IList<ForecastDayModel>>>();
			Assert.That(forecastDay.TotalTasks, Is.EqualTo(200d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(40d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(20d).Within(tolerance));
		}

		[Test]
		public void ShouldNotAddCampaignForClosedDay()
		{
			var model = new CampaignInput
			{
				SelectedDays = new[] {new DateOnly(2018, 5, 4)},
				CampaignTasksPercent = 0.5d,
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = false,
						Tasks = 100d
					}
				}
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>) Target.AddCampaign(model);

			result.Should().Be.OfType<OkNegotiatedContentResult<IList<ForecastDayModel>>>();
			result.Content.First().Tasks
				.Should().Be(100d);
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
				new List<TimePeriod> {new TimePeriod(10, 12)});
			workload.SetTemplate(openDay.Date.DayOfWeek, workloadDayTemplate);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = closedDay.Date,
				ScenarioId = scenario.Id.Value,
				Workload = new ForecastWorkloadInput
				{
					ForecastMethodId = ForecastMethodType.TeleoptiClassicShortTerm,
					WorkloadId = workload.Id.Value
				}
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.Forecast(forecastInput);

			result.Should().Be.OfType<OkNegotiatedContentResult<ForecastModel>>();
			result.Content.ForecastDays.Count.Should().Be(2);
			result.Content.ForecastDays.First().IsOpen.Should().Be(true);
			result.Content.ForecastDays.Last().IsOpen.Should().Be(false);
		}

		[Test]
		public void ShouldForecastUsingExistingCampaign()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());
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
				Workload = new ForecastWorkloadInput
				{
					ForecastMethodId = ForecastMethodType.TeleoptiClassicShortTerm,
					WorkloadId = workload.Id.Value
				}
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
			forecastDay.HasOverride.Should().Be.False();
			forecastDay.HasCampaign.Should().Be.True();
			forecastDay.CampaignTasksPercentage.Should().Be(new Percent(0.5).Value);
			forecastDay.TotalTasks.Should().Be(forecastDay.Tasks * 1.5);
			forecastDay.TotalAverageTaskTime.Should().Be(forecastDay.AverageTaskTime * 1.6);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(forecastDay.AverageAfterTaskTime * 1.7);
		}

		[Test]
		public void ShouldForecastUsingExistingOverride()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());

			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.SetOverrideTasks(100d, null);
			workloadDay.OverrideAverageTaskTime = TimeSpan.FromSeconds(150d);
			workloadDay.OverrideAverageAfterTaskTime = TimeSpan.FromSeconds(200d);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				Workload = new ForecastWorkloadInput
				{
					ForecastMethodId = ForecastMethodType.TeleoptiClassicShortTerm,
					WorkloadId = workload.Id.Value
				}
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
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
		public void ShouldForecastUsingExistingCampaignAndOverride()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());

			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.CampaignTasks = new Percent(0.5);
			workloadDay.CampaignTaskTime = new Percent(0.6);
			workloadDay.CampaignAfterTaskTime = new Percent(0.7);

			workloadDay.SetOverrideTasks(100d, null);
			workloadDay.OverrideAverageTaskTime = TimeSpan.FromSeconds(150d);
			workloadDay.OverrideAverageAfterTaskTime = TimeSpan.FromSeconds(200d);

			SkillRepository.Add(skill);
			WorkloadRepository.Add(workload);
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			var forecastInput = new ForecastInput
			{
				ForecastStart = openDay.Date,
				ForecastEnd = openDay.Date,
				ScenarioId = scenario.Id.Value,
				Workload = new ForecastWorkloadInput
				{
					ForecastMethodId = ForecastMethodType.TeleoptiClassicShortTerm,
					WorkloadId = workload.Id.Value
				}
			};
			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = openDay.AddDays(-10).Date.AddHours(10),
					StatOfferedTasks = 10
				}
			});
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.Forecast(forecastInput);
			var forecastDay = result.Content.ForecastDays.Single();
			forecastDay.HasCampaign.Should().Be.True();
			forecastDay.HasOverride.Should().Be.True();
			forecastDay.CampaignTasksPercentage.Should().Be(new Percent(0.5).Value);
			Assert.That(forecastDay.TotalTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
			Assert.That(forecastDay.OverrideTasks, Is.EqualTo(100d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageTaskTime, Is.EqualTo(150d).Within(tolerance));
			Assert.That(forecastDay.OverrideAverageAfterTaskTime, Is.EqualTo(200d).Within(tolerance));
		}

		[Test]
		public void ShouldAddOverride()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] {new DateOnly(2018, 5, 4)},
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 100d,
						TotalAverageTaskTime = 30d,
						TotalAverageAfterTaskTime = 10d
					},
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 5),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 100d,
						TotalAverageTaskTime = 30d,
						TotalAverageAfterTaskTime = 10d
					}
				},
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 50d,
				OverrideAverageAfterTaskTime = 20d,
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>) Target.AddOverride(model);

			result.Should().Be.OfType<OkNegotiatedContentResult<IList<ForecastDayModel>>>();
			result.Content.First().TotalTasks
				.Should().Be(200d);
			result.Content.First().TotalAverageTaskTime
				.Should().Be(50d);
			result.Content.First().TotalAverageAfterTaskTime
				.Should().Be(20d);

			result.Content.Last().TotalTasks
				.Should().Be(100d);
			result.Content.Last().TotalAverageTaskTime
				.Should().Be(30d);
			result.Content.Last().TotalAverageAfterTaskTime
				.Should().Be(10d);
		}

		[Test]
		public void ShouldClearOverrideValues()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] {new DateOnly(2018, 5, 4)},
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 150d,
						TotalAverageTaskTime = 50d,
						TotalAverageAfterTaskTime = 20d
					}
				},
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>) Target.AddOverride(model);

			result.Should().Be.OfType<OkNegotiatedContentResult<IList<ForecastDayModel>>>();
			var forecastDay = result.Content.First();
			forecastDay.TotalTasks
				.Should().Be(forecastDay.Tasks);
			forecastDay.TotalAverageTaskTime
				.Should().Be(forecastDay.AverageTaskTime);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(forecastDay.AverageAfterTaskTime);
		}

		[Test]
		public void ShouldNotAddOverrideOnClosedDay()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] {new DateOnly(2018, 5, 4)},
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = false,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						TotalTasks = 100d,
						TotalAverageTaskTime = 30d,
						TotalAverageAfterTaskTime = 10d
					}
				},
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 50d,
				OverrideAverageAfterTaskTime = 20d,
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>) Target.AddOverride(model);

			result.Content.First().TotalTasks
				.Should().Be(100d);
			result.Content.First().TotalAverageTaskTime
				.Should().Be(30d);
			result.Content.First().TotalAverageAfterTaskTime
				.Should().Be(10d);
			result.Content.First().HasOverride
				.Should().Be(false);
		}

		[Test]
		public void ShouldUseExistingCampaignWhenClearingOverride()
		{
			var model = new OverrideInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				ForecastDays = new List<ForecastDayModel>
				{
					new ForecastDayModel
					{
						Date = new DateOnly(2018, 5, 4),
						IsOpen = true,
						Tasks = 100d,
						AverageTaskTime = 30d,
						AverageAfterTaskTime = 10d,
						CampaignTasksPercentage = 0.5d,
						TotalTasks = 200d,
						TotalAverageTaskTime = 40d,
						TotalAverageAfterTaskTime = 20d
					}
				},
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true
			};

			var result = (OkNegotiatedContentResult<IList<ForecastDayModel>>)Target.AddOverride(model);

			result.Content.First().TotalTasks
				.Should().Be(150d);
			result.Content.First().TotalAverageTaskTime
				.Should().Be(30d);
			result.Content.First().TotalAverageAfterTaskTime
				.Should().Be(10d);
			result.Content.First().HasCampaign.Should().Be(true);
			result.Content.First().HasOverride.Should().Be(false);
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
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());

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
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			var forecastDay = result.Content.ForecastDays.Single();
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
		}

		[Test]
		public void ShouldLoadForecastWithCampaign()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var scenario = ScenarioFactory.CreateScenarioWithId("Default", true);
			var openDay = new DateOnly(2018, 05, 04);
			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, openDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());

			var workloadDay = skillDay.WorkloadDayCollection.Single();
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
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			var forecastDay = result.Content.ForecastDays.Single();
			Assert.That(forecastDay.Tasks, Is.EqualTo(workloadDay.Tasks).Within(tolerance));
			Assert.That(forecastDay.AverageTaskTime, Is.EqualTo(workloadDay.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay.AverageAfterTaskTime,
				Is.EqualTo(workloadDay.AverageAfterTaskTime.TotalSeconds).Within(tolerance));

			forecastDay.HasCampaign.Should().Be.True();
			forecastDay.HasOverride.Should().Be.False();

			Assert.That(forecastDay.TotalTasks,
				Is.EqualTo((1 + workloadDay.CampaignTasks.Value) * workloadDay.Tasks).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime,
				Is.EqualTo((1 + workloadDay.CampaignTaskTime.Value) * workloadDay.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime,
				Is.EqualTo((1 + workloadDay.CampaignAfterTaskTime.Value) * workloadDay.AverageAfterTaskTime.TotalSeconds)
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

			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.SetOverrideTasks(100d, null);
			workloadDay.OverrideAverageTaskTime = TimeSpan.FromSeconds(150d);
			workloadDay.OverrideAverageAfterTaskTime = TimeSpan.FromSeconds(200d);

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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.LoadForecast(forecastResultInput);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.ScenarioId.Should().Be.EqualTo(scenario.Id.Value);

			var forecastDay = result.Content.ForecastDays.Single();
			Assert.That(forecastDay.Tasks, Is.EqualTo(workloadDay.Tasks).Within(tolerance));
			Assert.That(forecastDay.AverageTaskTime, Is.EqualTo(workloadDay.AverageTaskTime.TotalSeconds).Within(tolerance));
			Assert.That(forecastDay.AverageAfterTaskTime,
				Is.EqualTo(workloadDay.AverageAfterTaskTime.TotalSeconds).Within(tolerance));

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
			workloadDay.SetOverrideTasks(100d, null);
			workloadDay.OverrideAverageTaskTime = TimeSpan.FromSeconds(150d);
			workloadDay.OverrideAverageAfterTaskTime = TimeSpan.FromSeconds(200d);

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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.LoadForecast(forecastResultInput);
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
	}
}
