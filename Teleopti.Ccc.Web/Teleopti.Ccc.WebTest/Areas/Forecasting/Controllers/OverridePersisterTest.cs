using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Forecasting.Controllers
{
	public class OverridePersisterTest
	{
		[Test]
		public void ShouldPersistADayThatHasOverrideValues()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var dateOnly = new DateOnly();
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			workloadDay.MakeOpen24Hours();
			workloadDay.Tasks = 100d;
			var skillDays = new[] { new SkillDay() };
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, workload.Skill, scenario)).Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] { workloadDay });

			var intradayForecaster = MockRepository.GenerateMock<IIntradayForecaster>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			var target = new OverridePersister(skillDayRepository, futureData, intradayForecaster, historicalPeriodProvider);
			workloadDay.OverrideTasks.Should().Be.EqualTo(null);

			var input = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = dateOnly.Date } },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTasks = 300,
				OverrideTalkTime = 20,
				OverrideAfterCallWork = 30,
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = true
			};

			target.Persist(scenario, workload, input);

			Math.Round(workloadDay.OverrideTasks.Value, 2).Should().Be.EqualTo(input.OverrideTasks);
			workloadDay.OverrideAverageTaskTime.Value.TotalSeconds.Should().Be.EqualTo(input.OverrideTalkTime);
			workloadDay.OverrideAverageAfterTaskTime.Value.TotalSeconds.Should().Be.EqualTo(input.OverrideAfterCallWork);
			historicalPeriodProvider.AssertWasNotCalled(x=>x.AvailableIntradayTemplatePeriod(workload));
			intradayForecaster.AssertWasNotCalled(x=>x.CalculatePattern(Arg<IWorkload>.Is.Anything, Arg<DateOnlyPeriod>.Is.Anything));
		}

		[Test]
		public void ShouldPersistOnlyOverrideTasks()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var dateOnly = new DateOnly();
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			workloadDay.MakeOpen24Hours();

			workloadDay.OverrideAverageTaskTime = TimeSpan.FromSeconds(120);
			workloadDay.OverrideAverageAfterTaskTime = TimeSpan.FromSeconds(90);
			
			var skillDays = new[] { new SkillDay() };
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, workload.Skill, scenario)).Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] { workloadDay });

			var target = new OverridePersister(skillDayRepository, futureData, MockRepository.GenerateMock<IIntradayForecaster>(), MockRepository.GenerateMock<IHistoricalPeriodProvider>());

			var input = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = dateOnly.Date } },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTasks = 300,
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = false,
				ShouldSetOverrideAfterCallWork = false
			};

			target.Persist(scenario, workload, input);

			Math.Round(workloadDay.OverrideTasks.Value, 2).Should().Be.EqualTo(input.OverrideTasks);
			workloadDay.OverrideAverageTaskTime.HasValue.Should().Be.True();
			workloadDay.OverrideAverageAfterTaskTime.HasValue.Should().Be.True();
		}

		[Test]
		public void ShouldPersistOnlyOverrideTaskTime()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var dateOnly = new DateOnly();
			var workloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(workload, dateOnly);
			workloadDay.MakeOpen24Hours();

			workloadDay.SetOverrideTasks(400d, null);
			
			var skillDays = new[] { new SkillDay() };
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, workload.Skill, scenario)).Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] { workloadDay });

			var target = new OverridePersister(skillDayRepository, futureData, MockRepository.GenerateMock<IIntradayForecaster>(), MockRepository.GenerateMock<IHistoricalPeriodProvider>());

			var input = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = dateOnly.Date } },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTalkTime = 30,
				ShouldSetOverrideTasks = false,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = false
			};

			target.Persist(scenario, workload, input);

			Math.Round(workloadDay.OverrideTasks.Value, 2).Should().Be.EqualTo(400d);
			workloadDay.OverrideTasks.HasValue.Should().Be.True();
		}

		[Test]
		public void ShouldNotProvidePatternWhenNoHistoricalData()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var dateOnly = new DateOnly();
			var workloadDay = MockRepository.GenerateMock<IWorkloadDay>();
			workloadDay.Stub(x => x.CurrentDate).Return(dateOnly);
			workloadDay.Stub(x => x.OpenForWork).Return(new OpenForWork(true, true));

			var skillDays = new SkillDay[] { };
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, workload.Skill, scenario)).Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] { workloadDay });

			var intradayForecaster = MockRepository.GenerateMock<IIntradayForecaster>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();

			historicalPeriodProvider.Stub(x => x.AvailableIntradayTemplatePeriod(workload)).Return(null);
			
			var target = new OverridePersister(skillDayRepository, futureData, intradayForecaster, historicalPeriodProvider);

			var input = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = dateOnly.Date } },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTasks = 300,
				OverrideTalkTime = 120,
				OverrideAfterCallWork = 30,
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = true
			};

			target.Persist(scenario, workload, input);
			workloadDay.AssertWasCalled(x => x.SetOverrideTasks(300d, null));
			intradayForecaster.AssertWasNotCalled(x => x.CalculatePattern(Arg<IWorkload>.Is.Anything, Arg<DateOnlyPeriod>.Is.Anything));
		}

		[Test]
		public void ShouldProvidePatternWhenADayThatHasZeroCalls()
		{
			var workload = WorkloadFactory.CreateWorkload(SkillFactory.CreateSkill("skill1"));
			var scenario = new Scenario("scenario1");
			var dateOnly = new DateOnly();
			var workloadDay = MockRepository.GenerateMock<IWorkloadDay>();
			workloadDay.Stub(x => x.CurrentDate).Return(dateOnly);
			workloadDay.Stub(x => x.OpenForWork).Return(new OpenForWork(true, true));

			var skillDays = new SkillDay[] {};
			var futurePeriod = new DateOnlyPeriod(dateOnly, dateOnly);
			var skillDayRepository = MockRepository.GenerateMock<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, workload.Skill, scenario)).Return(skillDays);
			var futureData = MockRepository.GenerateMock<IFutureData>();
			futureData.Stub(x => x.Fetch(workload, skillDays, futurePeriod)).Return(new[] { workloadDay });

			var intradayForecaster = MockRepository.GenerateMock<IIntradayForecaster>();
			var historicalPeriodProvider = MockRepository.GenerateMock<IHistoricalPeriodProvider>();
			var patternPeriod = new DateOnlyPeriod();

			historicalPeriodProvider.Stub(x => x.AvailableIntradayTemplatePeriod(workload)).Return(patternPeriod);
			var mondayPattern = new List<ITemplateTaskPeriod>();
			intradayForecaster.Stub(x => x.CalculatePattern(workload, patternPeriod))
				.Return(new Dictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>>
				{
					{DayOfWeek.Monday, mondayPattern}
				});

			var target = new OverridePersister(skillDayRepository, futureData, intradayForecaster, historicalPeriodProvider);

			var input = new OverrideInput
			{
				Days = new[] { new ModifiedDay { Date = dateOnly.Date } },
				ScenarioId = Guid.NewGuid(),
				WorkloadId = Guid.NewGuid(),
				OverrideTasks = 300,
				OverrideTalkTime = 120,
				OverrideAfterCallWork = 30,
				ShouldSetOverrideTasks = true,
				ShouldSetOverrideTalkTime = true,
				ShouldSetOverrideAfterCallWork = true
			};

			target.Persist(scenario, workload, input);

			workloadDay.AssertWasCalled(x => x.SetOverrideTasks(300d, mondayPattern));

		}
	}
}