using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.FutureData;
using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
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
			//get historical stuff
			var taskOwnerPeriod = _historicalDataProvider.Fetch(workload, historicalPeriod);

			//get future workloaddays
			var futureSkillDays = _loadSkillDaysInDefaultScenario.FindRange(futurePeriod, workload.Skill);
			new SkillDayCalculator(workload.Skill, futureSkillDays.ToList(), futurePeriod);
			var futureWorkloadDays = getFutureWorkloadDaysFromSkillDays(futureSkillDays);

			//change future skillday
			var totalVolume = new TotalVolume();
			VolumeYear volumeMonthYear = new MonthOfYear(taskOwnerPeriod, new MonthOfYearCreator());
			VolumeYear volumeWeekYear = new WeekOfMonth(taskOwnerPeriod, new WeekOfMonthCreator());
			VolumeYear volumeDayYear = new DayOfWeeks(taskOwnerPeriod, new DaysOfWeekCreator());
			var indexes = new List<IVolumeYear> {volumeMonthYear, volumeWeekYear, volumeDayYear};
			totalVolume.Create(taskOwnerPeriod, futureWorkloadDays, indexes, new IOutlier[] { }, 0, 0, false, workload);
		}

		private IList<ITaskOwner> getFutureWorkloadDaysFromSkillDays(IEnumerable<ISkillDay> skilldays)
		{
			return skilldays.Select(s => s.WorkloadDayCollection.First()).OfType<ITaskOwner>().ToList();
		}
	}
}