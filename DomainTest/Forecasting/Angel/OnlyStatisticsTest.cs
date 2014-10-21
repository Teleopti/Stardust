using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class OnlyStatisticsTest
	{
		[Test]
		public void SingleSimpleWorkloadDay()
		{
			const int expectedNumberOfTasks = 123;

			var skill = SkillFactory.CreateSkill("_");
			var workload = WorkloadFactory.CreateWorkload(skill);
			var historicalPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			var futurePeriod = new DateOnlyPeriod(historicalPeriod.StartDate.AddDays(7), historicalPeriod.EndDate.AddDays(7));
			var currentScenario = new FakeCurrentScenario();

			var historicalDailyStatistic = new DailyStatistic(historicalPeriod.StartDate, expectedNumberOfTasks);

			var dailyStatistics = MockRepository.GenerateStub<IDailyStatisticsAggregator>();
			dailyStatistics.Stub(x => x.LoadDailyStatistics(workload, historicalPeriod)).Return(new[] { historicalDailyStatistic });
	
			IWorkloadDay futureWorkloadDay = new WorkloadDay();
			var template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Saturday);
			template.MakeOpen24Hours();
			futureWorkloadDay.CreateFromTemplate(futurePeriod.StartDate, workload, template);
			
			var futureSkillDay = new SkillDay(
				futurePeriod.StartDate, 
				skill, 
				currentScenario.Current(), 
				new []{futureWorkloadDay},
				Enumerable.Empty<ISkillDataPeriod>());

			//should not be like this, include "GetAllSkillDays" move to some service later
			var skillDayRepository = MockRepository.GenerateStub<ISkillDayRepository>();
			skillDayRepository.Stub(x => x.FindRange(futurePeriod, skill, currentScenario.Current())).Return(new[] { futureSkillDay });
			//skillDayRepository.Stub(x => x.GetAllSkillDays(period, new ISkillDay[] { }, skill, scenario, skillDayRepository.AddRange)).Return(new []{skillDay});

			var target = new QuickForecaster(new HistoricalDataProvider(dailyStatistics, null), skillDayRepository, currentScenario);
			target.Execute(workload, historicalPeriod, futurePeriod);

			futureSkillDay.Tasks.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}