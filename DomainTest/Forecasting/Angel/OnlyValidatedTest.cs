using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
	public class OnlyValidatedTest
	{
		[Test]
		public void SingleSimpleWorkloadDay()
		{
			const int expectedNumberOfTasks = 123;

			var skill = SkillFactory.CreateSkill("_");
			var workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			var historicalPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2);
			var futurePeriod = new DateOnlyPeriod(historicalPeriod.StartDate.AddDays(7), historicalPeriod.EndDate.AddDays(7));
			
			var loadStatistics = MockRepository.GenerateStub<IDailyStatisticsAggregator>();
			loadStatistics.Stub(x => x.LoadDailyStatistics(workload, historicalPeriod)).Return(Enumerable.Empty<DailyStatistic>());

			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			var workloadDay = new WorkloadDay();
			workloadDay.Create(historicalPeriod.StartDate,workload,new TimePeriod[]{});
			workloadDay.MakeOpen24Hours();
			validatedVolumeDayRepository.Stub(x => x.FindRange(historicalPeriod, workload)).Return(new[]
			{
				new ValidatedVolumeDay(workload, historicalPeriod.StartDate)
				{
					TaskOwner = workloadDay,
					ValidatedTasks = expectedNumberOfTasks
				}
			});
			
			IWorkloadDay futureWorkloadDay = new WorkloadDay();
			var template = (IWorkloadDayTemplate)workload.GetTemplate(TemplateTarget.Workload, DayOfWeek.Saturday);
			template.MakeOpen24Hours();
			futureWorkloadDay.CreateFromTemplate(futurePeriod.StartDate, workload, template);

			var futureSkillDay = new SkillDay(
				futurePeriod.StartDate,
				skill,
				new Scenario("sdf"), 
				new[] { futureWorkloadDay },
				Enumerable.Empty<ISkillDataPeriod>());

			var loadSkillDays = MockRepository.GenerateMock<ILoadSkillDaysInDefaultScenario>();
			loadSkillDays.Stub(x => x.FindRange(futurePeriod, skill)).Return(new[] { futureSkillDay });

			var target = new QuickForecaster(new HistoricalDataProvider(loadStatistics, validatedVolumeDayRepository), loadSkillDays);
			target.Execute(workload, historicalPeriod, futurePeriod);

			futureSkillDay.Tasks.Should().Be.EqualTo(expectedNumberOfTasks);
		}
	}
}