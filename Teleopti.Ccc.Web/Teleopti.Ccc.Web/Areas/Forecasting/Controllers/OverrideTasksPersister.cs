using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class OverrideTasksPersister : IOverrideTasksPersister
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;
		private readonly IIntradayForecaster _intradayForecaster;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public OverrideTasksPersister(ISkillDayRepository skillDayRepository, IFutureData futureData, IIntradayForecaster intradayForecaster, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
			_intradayForecaster = intradayForecaster;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, double overrideTasks)
		{
			var min = days.Min(x => x.Date);
			var max = days.Max(x => x.Date);
			var futurePeriod = new DateOnlyPeriod(new DateOnly(min), new DateOnly(max));
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var workloadDays = _futureData.Fetch(workload, skillDays, futurePeriod).ToList();
			IDictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>> intradayPattern = new Dictionary<DayOfWeek, IEnumerable<ITemplateTaskPeriod>>();

			var allDaysHaveTasks = workloadDays.All(x => Math.Abs(x.Tasks) > 0);
			if (!allDaysHaveTasks)
			{
				var statsPeriod = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(workload);
				if (statsPeriod.HasValue)
					intradayPattern = _intradayForecaster.CalculatePattern(workload, statsPeriod.Value);
			}

			foreach (var workloadDay in days.Select(day => workloadDays.Single(x => x.CurrentDate == new DateOnly(day.Date))).Where(workloadDay => workloadDay.OpenForWork.IsOpen))
			{
				IEnumerable<ITemplateTaskPeriod> weekdayPattern;
				if (workloadDay.Tasks > 0d || !intradayPattern.TryGetValue(workloadDay.CurrentDate.DayOfWeek, out weekdayPattern))
				{
					workloadDay.SetOverrideTasks(overrideTasks, null); 	
				}
				else
				{
					workloadDay.SetOverrideTasks(overrideTasks, weekdayPattern); 
				}
			}
		}
	}
}