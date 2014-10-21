using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public abstract class QuickForecastTest
	{
		private IWorkload _workload;

		protected virtual DateOnlyPeriod HistoricalPeriod
		{
			get { return new DateOnlyPeriod(2000, 1, 1, 2000, 1, 2); }
		}

		protected virtual DateOnlyPeriod FuturePeriod
		{
			get { return new DateOnlyPeriod(HistoricalPeriod.StartDate.AddDays(7), HistoricalPeriod.EndDate.AddDays(7)); }
		}

		protected virtual IWorkload Workload
		{
			get 
			{
				return _workload ?? (_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(SkillFactory.CreateSkill("_")));
			}
		}

		protected abstract IEnumerable<DailyStatistic> DailyStatistics();
		protected abstract IEnumerable<IValidatedVolumeDay> ValidatedVolumeDays();

		protected virtual IEnumerable<ISkillDay> CurrentSkillDays()
		{
			var futureWorkloadDay = WorkloadDayFactory.CreateWorkloadDayFromWorkloadTemplate(Workload, FuturePeriod.StartDate);
			return new[]
			{
				new SkillDay(
					FuturePeriod.StartDate,
					Workload.Skill,
					new Scenario("sdfdsf"),
					new[] {futureWorkloadDay},
					Enumerable.Empty<ISkillDataPeriod>())
			};
		}
		protected abstract void Assert(IEnumerable<ISkillDay> modifiedSkillDays);

		[Test]
		public void DoTheTest()
		{
			var validatedVolumeDayRepository = MockRepository.GenerateStub<IValidatedVolumeDayRepository>();
			validatedVolumeDayRepository.Stub(x => x.FindRange(HistoricalPeriod, Workload)).Return(ValidatedVolumeDays().ToList());

			var dailyStatistics = MockRepository.GenerateStub<IDailyStatisticsAggregator>();
			dailyStatistics.Stub(x => x.LoadDailyStatistics(Workload, HistoricalPeriod)).Return(DailyStatistics());

			var skillDays = CurrentSkillDays();
			var loadSkillDays = MockRepository.GenerateMock<ILoadSkillDaysInDefaultScenario>();
			loadSkillDays.Stub(x => x.FindRange(FuturePeriod, Workload.Skill)).Return(skillDays);

			var target = new QuickForecaster(new HistoricalData(dailyStatistics, validatedVolumeDayRepository), new FutureData(loadSkillDays));
			target.Execute(Workload, HistoricalPeriod, FuturePeriod);

			Assert(skillDays);
		}
	}
}