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
	public class OverrideTasksPersisterTest
	{
		[Test]
		public void ShouldPersistADayThatHasCalls()
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
			var target = new OverrideTasksPersister(skillDayRepository, futureData, intradayForecaster, historicalPeriodProvider);
			workloadDay.OverrideTasks.Should().Be.EqualTo(null);
			target.Persist(scenario, workload, new[]
			{
				new ModifiedDay
				{
					Date = dateOnly.Date
				}
			}, 300);
			Math.Round(workloadDay.OverrideTasks.Value, 2).Should().Be.EqualTo(300d);
			historicalPeriodProvider.AssertWasNotCalled(x=>x.AvailableIntradayTemplatePeriod(workload));
			intradayForecaster.AssertWasNotCalled(x=>x.CalculatePattern(Arg<IWorkload>.Is.Anything, Arg<DateOnlyPeriod>.Is.Anything));
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
			
			var target = new OverrideTasksPersister(skillDayRepository, futureData, intradayForecaster, historicalPeriodProvider);
			target.Persist(scenario, workload, new[]
			{
				new ModifiedDay
				{
					Date = dateOnly.Date
				}
			}, 300);
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

			var target = new OverrideTasksPersister(skillDayRepository, futureData, intradayForecaster, historicalPeriodProvider);
			target.Persist(scenario, workload, new[]
			{
				new ModifiedDay
				{
					Date = dateOnly.Date
				}
			}, 300);

			workloadDay.AssertWasCalled(x => x.SetOverrideTasks(300d, mondayPattern));

		}
	}
}