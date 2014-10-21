using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IHistoricalDataProvider _historicalDataProvider;
		private readonly ILoadSkillDaysInDefaultScenario _loadSkillDaysInDefaultScenario;

		public QuickForecaster(IHistoricalDataProvider historicalDataProvider, ILoadSkillDaysInDefaultScenario loadSkillDaysInDefaultScenario)
		{
			_historicalDataProvider = historicalDataProvider;
			_loadSkillDaysInDefaultScenario = loadSkillDaysInDefaultScenario;
		}

		public void Execute(IWorkload workload, DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod)
		{
			//load historical stuff
			var newStuff = _historicalDataProvider.Calculate(workload, historicalPeriod);
			var daysFromThePasthWithStatistics = new TaskOwnerPeriod(DateOnly.MinValue, newStuff.Convert(workload), TaskOwnerPeriodType.Other);

			//load future stuff
			var futureSkillDays = _loadSkillDaysInDefaultScenario.FindRange(futurePeriod, workload.Skill);
			new SkillDayCalculator(workload.Skill, futureSkillDays.ToList(), futurePeriod);
			var futureWorkloadDays = getFutureWorkloadDaysFromSkillDays(futureSkillDays);

			//change future stuff
			var totalVolume = new TotalVolume(); 
			VolumeYear volumeMonthYear = new MonthOfYear(daysFromThePasthWithStatistics, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(daysFromThePasthWithStatistics, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(daysFromThePasthWithStatistics, new DaysOfWeekCreator());
		    var indexes = new List<IVolumeYear> {volumeMonthYear, volumeWeekYear, volumeDayYear};
			totalVolume.Create(daysFromThePasthWithStatistics, futureWorkloadDays, indexes, new IOutlier[] {}, 0, 0, false,
				workload);
		}

		private IList<ITaskOwner> getFutureWorkloadDaysFromSkillDays(IEnumerable<ISkillDay> skilldays)
		{
			return skilldays.Select(s => s.WorkloadDayCollection.First()).OfType<ITaskOwner>().ToList();
		}
	}
}