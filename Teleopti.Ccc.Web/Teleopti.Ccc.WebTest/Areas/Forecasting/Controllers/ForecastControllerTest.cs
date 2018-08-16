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
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
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
		public FakeForecastDayOverrideRepository ForecastDayOverrideRepository;
		public FullPermission FullPermission;

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
			taskPeriods.All(x => x.Tasks > 0).Should().Be.True();
			Assert.That(taskPeriods.Sum(x => x.Tasks), Is.EqualTo(200).Within(tolerance));
		}

		[Test]
		public void ShouldHandleEmptyForecastDaysOnSavingForecastResult()
		{
			var forecastResult = new ForecastModel
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
					CampaignTasksPercentage = 0.5d,
					IsInModification = true
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
					OverrideAverageAfterTaskTime = 20,
					IsInModification = true
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
			savedWorkloadDay.Annotation.Should().Be(overrideNote+"This is existing note.");
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
			workloadDay.Annotation = overrideNote+"This is existing note.";

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
			var forecastResult = new ForecastModel
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
			ForecastDayOverrideRepository.Add(new ForecastDayOverride(forecastedDay,workload,scenario)
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

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var firstForecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			var secondForecastDay = ((List<ForecastDayModel>)result.ForecastDays).Last();

			firstForecastDay.HasCampaign.Should().Be.True();
			firstForecastDay.IsInModification.Should().Be.True();
			firstForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(model.CampaignTasksPercent);
			firstForecastDay.Tasks.Should().Be(100d);
			firstForecastDay.TotalTasks.Should().Be(150d);

			secondForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(0);
			secondForecastDay.HasCampaign.Should().Be.False();
			secondForecastDay.IsInModification.Should().Be.False();
			secondForecastDay.Tasks.Should().Be(100d);
			secondForecastDay.TotalTasks.Should().Be(100d);
		}


		[Test]
		public void ShouldAddNegativeCampaign()
		{
			var model = new CampaignInput
			{
				SelectedDays = new[] { new DateOnly(2018, 5, 4) },
				CampaignTasksPercent = -0.5d,
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

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var firstForecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			var secondForecastDay = ((List<ForecastDayModel>)result.ForecastDays).Last();

			firstForecastDay.HasCampaign.Should().Be.True();
			firstForecastDay.CampaignTasksPercentage.Should().Be.EqualTo(model.CampaignTasksPercent);
			firstForecastDay.Tasks.Should().Be(100d);
			firstForecastDay.TotalTasks.Should().Be(50d);

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

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			Assert.That(forecastDay.TotalTasks, Is.EqualTo(200d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageTaskTime, Is.EqualTo(40d).Within(tolerance));
			Assert.That(forecastDay.TotalAverageAfterTaskTime, Is.EqualTo(20d).Within(tolerance));
			((string) result.WarningMessage).Should().Be(Resources.CampaignNotAppliedWIthExistingOverride);
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

			dynamic data = Target.AddCampaign(model);
			var result = data.Content;
			var firstForecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			firstForecastDay.Tasks
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
			var forecastDays = result.Content.ForecastDays;
			forecastDays.Count.Should().Be(2);
			forecastDays.First().IsOpen.Should().Be.True();
			forecastDays.First().IsInModification.Should().Be.True();
			forecastDays.Last().IsOpen.Should().Be.False();
			forecastDays.Last().IsInModification.Should().Be.False();
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
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());

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
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod());

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
			forecastDay.IsInModification.Should().Be(true);
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

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			var lastForecastDay = ((List<ForecastDayModel>)result.ForecastDays).Last();

			forecastDay.TotalTasks
				.Should().Be(200d);
			forecastDay.TotalAverageTaskTime
				.Should().Be(50d);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(20d);
			forecastDay.OverrideTasks
				.Should().Be(200d);
			forecastDay.HasOverride
				.Should().Be(true);
			forecastDay.IsInModification
				.Should().Be(true);

			lastForecastDay.TotalTasks
				.Should().Be(100d);
			lastForecastDay.TotalAverageTaskTime
				.Should().Be(30d);
			lastForecastDay.TotalAverageAfterTaskTime
				.Should().Be(10d);
			lastForecastDay.OverrideTasks
				.Should().Be(null);
			lastForecastDay.HasOverride
				.Should().Be(false);
			lastForecastDay.IsInModification
				.Should().Be(false);
			((string) result.WarningMessage).Should().Be.Empty();
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

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			forecastDay.TotalTasks
				.Should().Be(forecastDay.Tasks);
			forecastDay.TotalAverageTaskTime
				.Should().Be(forecastDay.AverageTaskTime);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(forecastDay.AverageAfterTaskTime);
			((string)result.WarningMessage).Should().Be.Empty();
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

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			forecastDay.TotalTasks
				.Should().Be(100d);
			forecastDay.TotalAverageTaskTime
				.Should().Be(30d);
			forecastDay.TotalAverageAfterTaskTime
				.Should().Be(10d);
			forecastDay.HasOverride
				.Should().Be(false);
		}

		[Test]
		public void ShouldClearExistingCampaignWhenAddOverride()
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
						HasCampaign = true
					}
				},
				ShouldOverrideTasks = true,
				ShouldOverrideAverageTaskTime = true,
				ShouldOverrideAverageAfterTaskTime = true,
				OverrideTasks = 200d,
				OverrideAverageTaskTime = 50d,
				OverrideAverageAfterTaskTime = 20d,
			};
			
			dynamic data = Target.AddOverride(model);
			var result = data.Content;

			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();
			forecastDay.HasOverride.Should().Be(true);
			forecastDay.TotalTasks.Should().Be(200d);
			forecastDay.TotalAverageTaskTime.Should().Be(50d);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(20d);

			forecastDay.HasCampaign.Should().Be(false);
			forecastDay.CampaignTasksPercentage.Should().Be(0);
			((string)result.WarningMessage).Should().Be(Resources.ClearCampaignWIthOverride);
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

			dynamic data = Target.AddOverride(model);
			var result = data.Content;
			var forecastDay = ((List<ForecastDayModel>)result.ForecastDays).First();

			forecastDay.TotalTasks.Should().Be(150d);
			forecastDay.TotalAverageTaskTime.Should().Be(30d);
			forecastDay.TotalAverageAfterTaskTime.Should().Be(10d);
			forecastDay.HasCampaign.Should().Be(true);
			forecastDay.HasOverride.Should().Be(false);
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
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.LoadForecast(forecastResultInput);
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
		public void ShouldLoadQueueStatistics()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var statsDate = new DateOnly(2018, 05, 04);

			WorkloadRepository.Add(workload);

			StatisticRepository.Has(workload.QueueSourceCollection.First(), new List<IStatisticTask>
			{
				new StatisticTask
				{
					Interval = statsDate.Date.AddHours(10),
					StatOfferedTasks = 10
				},
				new StatisticTask
				{
					Interval = statsDate.AddDays(1).Date.AddHours(10).AddMinutes(15),
					StatOfferedTasks = 20
				}
			});
			
			var result = (OkNegotiatedContentResult<WorkloadQueueStatisticsViewModel>)Target.QueueStatistics(workload.Id.Value);
			result.Content.WorkloadId.Should().Be.EqualTo(workload.Id.Value);
			result.Content.QueueStatisticsDays.Count.Should().Be.EqualTo(2);
			result.Content.QueueStatisticsDays.First().Date.Should().Be.EqualTo(statsDate);
			result.Content.QueueStatisticsDays.First().OriginalTasks.Should().Be.EqualTo(10);
			result.Content.QueueStatisticsDays.First().ValidatedTasks.Should().Be.EqualTo(10);
			result.Content.QueueStatisticsDays.Last().Date.Should().Be.EqualTo(statsDate.AddDays(1));
			result.Content.QueueStatisticsDays.Last().OriginalTasks.Should().Be.EqualTo(20);
			result.Content.QueueStatisticsDays.Last().ValidatedTasks.Should().Be.EqualTo(20);
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
			skillDay1.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay1}, new DateOnlyPeriod());
			skillDay2.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay2}, new DateOnlyPeriod());

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
			var result = (OkNegotiatedContentResult<ForecastModel>) Target.LoadForecast(forecastResultInput);
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
			var result = (OkNegotiatedContentResult<ForecastModel>)Target.LoadForecast(forecastResultInput);
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

		[Test]
		public void ShouldGetSkillsAndWorkloads()
		{
			var skill = SkillFactory.CreateSkillWithWorkloadAndSources().WithId();
			var workload = skill.WorkloadCollection.Single();
			var workloadName = skill.Name + " - " + workload.Name;

			SkillRepository.Has(skill);

			var target = new ForecastController(null, SkillRepository, null, null, null, FullPermission,
				null, null, null, null, null);

			var result = target.Skills();
			result.Skills.Single().Id.Should().Be.EqualTo(skill.Id.Value);
			result.Skills.Single().Workloads.Single().Id.Should().Be.EqualTo(workload.Id.Value);
			result.Skills.Single().Workloads.Single().Name.Should().Be.EqualTo(workloadName);
			result.Skills.Single().SkillType.Should().Be.EqualTo(skill.SkillType.Description.Name);
		}

		[Test]
		public void ShouldHavePermissionForModifySkill()
		{
			var target = new ForecastController(null, SkillRepository, null, null, null, FullPermission,
				null, null, null, null, null);

			var result = target.Skills();
			result.IsPermittedToModifySkill.Should().Be.EqualTo(true);
		}
	}

}
