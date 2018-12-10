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
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;


namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	[DomainTest]
	public class ForecastSaveControllerTest : IExtendSystem
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
					AverageAfterTaskTime = 60,
					IsInModification = true
				},
				new ForecastDayModel
				{
					Date = forecastedDay2,
					Tasks = 15,
					AverageTaskTime = 65,
					AverageAfterTaskTime = 65,
					IsInModification = true
				}
			};
			var forecastResult = new ForecastViewModel
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
					Tasks = 1500,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60,
					IsInModification = true
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
					StatOfferedTasks = 250
				},
				new StatisticTask
				{
					Interval = templateDay.AddHours(10).AddMinutes(30),
					StatOfferedTasks = 1000
				},
				new StatisticTask
				{
					Interval = templateDay.AddHours(10).AddMinutes(45),
					StatOfferedTasks = 100
				},
			});

			var forecastResult = new ForecastViewModel
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
			taskPeriods.All(x => x.Tasks > 0).Should().Be.True();
			Math.Round(taskPeriods[1].Tasks, 4).Should().Be.GreaterThan(Math.Round(taskPeriods[0].Tasks, 4));
			Math.Round(taskPeriods[2].Tasks, 4).Should().Be.EqualTo(Math.Round(taskPeriods[1].Tasks, 4));
			Math.Round(taskPeriods[3].Tasks, 4).Should().Be.LessThan(Math.Round(taskPeriods[2].Tasks, 4));
		}

		[Test]
		public void ShouldHandleEmptyForecastDaysOnSavingForecastResult()
		{
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = Guid.NewGuid(),
				ScenarioId = Guid.NewGuid(),
				ForecastDays = new List<ForecastDayModel>()
			};

			var result = Target.ApplyForecast(forecastResult);
			result.Should().Be.OfType<OkResult>();
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
					AverageAfterTaskTime = 60,
					IsInModification = true
				}
			};

			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};

			var result = Target.ApplyForecast(forecastResult);
			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var taskPeriods = savedForecastDay.WorkloadDayCollection.Single().TaskPeriodList;

			taskPeriods.Count.Should().Be(1);
			taskPeriods.First().Tasks.Should().Be(200);
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

			var forecastResult = new ForecastViewModel
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
					CampaignTasksPercentage = 0.5d,
					IsInModification = true
				}
			};
			var forecastResult = new ForecastViewModel
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
					OverrideAverageAfterTaskTime = 20,
					IsInModification = true
				}
			};
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();
			var savedOverride = ForecastDayOverrideRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), workload, scenario).Single();

			savedOverride.OriginalTasks.Should().Be(forecastDays.First().Tasks);
			savedOverride.OriginalAverageTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageTaskTime);
			savedOverride.OriginalAverageAfterTaskTime.TotalSeconds.Should().Be(forecastDays.First().AverageAfterTaskTime);

			Assert.That(savedOverride.OverriddenTasks, Is.EqualTo(80d).Within(tolerance));
			Assert.That(savedOverride.OverriddenAverageTaskTime, Is.EqualTo(TimeSpan.FromSeconds(50)).Within(tolerance));
			Assert.That(savedOverride.OverriddenAverageAfterTaskTime, Is.EqualTo(TimeSpan.FromSeconds(20)).Within(tolerance));

			Assert.That(savedWorkloadDay.TotalTasks, Is.EqualTo(80d).Within(tolerance));
			Assert.That(savedWorkloadDay.TotalAverageTaskTime, Is.EqualTo(TimeSpan.FromSeconds(50)).Within(tolerance));
			Assert.That(savedWorkloadDay.TotalAverageAfterTaskTime, Is.EqualTo(TimeSpan.FromSeconds(20)).Within(tolerance));
			var overrideNote = $"[*{Resources.ForecastDayIsOverrided}*]";
			savedWorkloadDay.Annotation.Should().Be(overrideNote);
		}

		[Test]
		public void ShouldClearCampaignOnSavingForecastWithOverride()
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

			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, forecastedDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());
			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.CampaignTasks = new Percent(0.2);
			workloadDay.CampaignTaskTime = new Percent(0.1);
			workloadDay.CampaignAfterTaskTime = new Percent(0.1);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 100,
					AverageTaskTime = 300,
					AverageAfterTaskTime = 200,
					HasCampaign = true,
					TotalTasks = 120,
					TotalAverageTaskTime = 330,
					TotalAverageAfterTaskTime = 220,
					OverrideTasks = 80,
					OverrideAverageTaskTime = 50,
					OverrideAverageAfterTaskTime = 20,
					HasOverride = true
				}
			};
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();

			savedWorkloadDay.CampaignTasks.Value.Should().Be(0);
			savedWorkloadDay.CampaignTaskTime.Value.Should().Be(0);
			savedWorkloadDay.CampaignAfterTaskTime.Value.Should().Be(0);
		}

		[Test]
		public void ShouldAppendNoteOnSavingForecastWithOverride()
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

			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, forecastedDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());
			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.Annotation = "This is existing note.";

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);
			var overrideNote = $"[*{Resources.ForecastDayIsOverrided}*]";

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 100,
					AverageTaskTime = 300,
					AverageAfterTaskTime = 200,
					HasCampaign = true,
					TotalTasks = 120,
					TotalAverageTaskTime = 330,
					TotalAverageAfterTaskTime = 220,
					OverrideTasks = 80,
					OverrideAverageTaskTime = 50,
					OverrideAverageAfterTaskTime = 20,
					HasOverride = true
				}
			};
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();
			savedWorkloadDay.Annotation.Should().Be(overrideNote + "This is existing note.");
		}

		[Test]
		public void ShouldClearCampaignTasksOnSavingForecastWithOverride()
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

			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, forecastedDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());
			var workloadDay = skillDay.WorkloadDayCollection.Single();
			workloadDay.CampaignTasks = new Percent(0.2);
			workloadDay.CampaignTaskTime = new Percent(0.1);
			workloadDay.CampaignAfterTaskTime = new Percent(0.1);

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 100,
					AverageTaskTime = 300,
					AverageAfterTaskTime = 200,
					HasCampaign = true,
					TotalTasks = 120,
					TotalAverageTaskTime = 330,
					TotalAverageAfterTaskTime = 220,
					OverrideTasks = 80,
					HasOverride = true
				}
			};
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();

			savedWorkloadDay.CampaignTasks.Value.Should().Be(0);
			Assert.That(savedWorkloadDay.CampaignTaskTime.Value, Is.EqualTo(0.1d).Within(tolerance));
			Assert.That(savedWorkloadDay.CampaignAfterTaskTime.Value, Is.EqualTo(0.1d).Within(tolerance));
		}

		[Test]
		public void ShouldClearOverrideOnSavingForecast()
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

			var skillDay = SkillDayFactory.CreateSkillDay(skill, workload, forecastedDay, scenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod());
			var workloadDay = skillDay.WorkloadDayCollection.Single();
			var overrideNote = $"[*{Resources.ForecastDayIsOverrided}*]";
			workloadDay.Annotation = overrideNote + "This is existing note.";

			WorkloadRepository.Add(skill.WorkloadCollection.Single());
			ScenarioRepository.Has(scenario);
			SkillDayRepository.Add(skillDay);
			ForecastDayOverrideRepository.Add(new ForecastDayOverride(forecastedDay, workload, scenario)
			{
				OverriddenTasks = 15,
				OverriddenAverageTaskTime = TimeSpan.FromSeconds(40),
				OverriddenAverageAfterTaskTime = TimeSpan.FromSeconds(50)
			});

			IList<ForecastDayModel> forecastDays = new List<ForecastDayModel>
			{
				new ForecastDayModel
				{
					Date = forecastedDay,
					Tasks = 10,
					AverageTaskTime = 60,
					AverageAfterTaskTime = 60,
					HasOverride = false
				}
			};
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();

			ForecastDayOverrideRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), workload, scenario)
				.Should().Be.Empty();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();

			Assert.That(savedWorkloadDay.TotalTasks, Is.EqualTo(10d).Within(tolerance));
			Assert.That(savedWorkloadDay.TotalAverageTaskTime, Is.EqualTo(TimeSpan.FromSeconds(60)).Within(tolerance));
			Assert.That(savedWorkloadDay.TotalAverageAfterTaskTime, Is.EqualTo(TimeSpan.FromSeconds(60)).Within(tolerance));
			savedWorkloadDay.Annotation.Should().Be("This is existing note.");
		}

		[Test]
		public void ShouldSaveForecastWithExistingOverride()
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
			ForecastDayOverrideRepository.Add(new ForecastDayOverride(forecastedDay, workload, scenario)
			{
				OverriddenTasks = 15,
				OriginalTasks = 10
			});

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
					OverrideAverageAfterTaskTime = 20,
					IsInModification = true
				}
			};
			var forecastResult = new ForecastViewModel
			{
				WorkloadId = skill.WorkloadCollection.Single().Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = forecastDays
			};
			var result = Target.ApplyForecast(forecastResult);

			result.Should().Be.OfType<OkResult>();
			var savedForecastDay = SkillDayRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), skill, scenario).Single();
			var savedWorkloadDay = savedForecastDay.WorkloadDayCollection.Single();
			var savedOverride = ForecastDayOverrideRepository.FindRange(forecastedDay.ToDateOnlyPeriod(), workload, scenario).Single();

			savedOverride.OriginalTasks.Should().Be(10d);
			savedOverride.OriginalAverageTaskTime.TotalSeconds.Should().Be(60d);
			savedOverride.OriginalAverageAfterTaskTime.TotalSeconds.Should().Be(60d);

			Assert.That(savedOverride.OverriddenTasks, Is.EqualTo(80d).Within(tolerance));
			Assert.That(savedOverride.OverriddenAverageTaskTime, Is.EqualTo(TimeSpan.FromSeconds(50)).Within(tolerance));
			Assert.That(savedOverride.OverriddenAverageAfterTaskTime, Is.EqualTo(TimeSpan.FromSeconds(20)).Within(tolerance));

			Assert.That(savedWorkloadDay.TotalTasks, Is.EqualTo(80d).Within(tolerance));
			Assert.That(savedWorkloadDay.TotalAverageTaskTime, Is.EqualTo(TimeSpan.FromSeconds(50)).Within(tolerance));
			Assert.That(savedWorkloadDay.TotalAverageAfterTaskTime, Is.EqualTo(TimeSpan.FromSeconds(20)).Within(tolerance));
		}
	}
}
