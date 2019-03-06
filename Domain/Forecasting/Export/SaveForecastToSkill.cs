using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public class SaveForecastToSkill : ISaveForecastToSkill
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

		public SaveForecastToSkill( ISkillDayRepository skillDayRepository, IScenarioRepository scenarioRepository)
		{
			_skillDayRepository = skillDayRepository;
			_scenarioRepository = scenarioRepository;
		}

		public void Execute(DateOnly dateOnly, ISkill targetSkill, ICollection<ForecastsRow> forecasts, ImportForecastsMode importMode)
		{
			var defaultScenario = _scenarioRepository.LoadDefaultScenario(targetSkill.GetOrFillWithBusinessUnit_DONTUSE());
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);

			var skilldays = _skillDayRepository.FindRange(dateOnlyPeriod, targetSkill, defaultScenario);
			var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(skilldays,
																									 targetSkill.WorkloadCollection.First());

			var workloadDay = workloadDays.FirstOrDefault(d => d.CurrentDate.Equals(dateOnly));
			if (workloadDay == null) return;
			var skillDay = (ISkillDay)workloadDay.Parent;
			workloadDay.Lock();
			foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
			{
				var period = skillDataPeriod;
				var forecastRow =
					 forecasts.FirstOrDefault(
						  f => new DateTimePeriod(f.UtcDateTimeFrom, f.UtcDateTimeTo).Equals(period.Period));
				if (forecastRow == null) continue;
				if (forecastRow.Agents.HasValue)
					skillDataPeriod.ManualAgents = forecastRow.Agents;
				if (forecastRow.Shrinkage.HasValue)
					skillDataPeriod.Shrinkage = new Percent(forecastRow.Shrinkage.GetValueOrDefault());

				if (importMode == ImportForecastsMode.ImportWorkload || importMode == ImportForecastsMode.ImportWorkloadAndStaffing)
				{
					var taskPeriod =
						 workloadDay.TaskPeriodList.FirstOrDefault(
							  p => p.Period.StartDateTime == skillDataPeriod.Period.StartDateTime);
					if (taskPeriod == null) continue;
					taskPeriod.Tasks = forecastRow.Tasks;
					taskPeriod.AverageTaskTime = TimeSpan.FromSeconds(forecastRow.TaskTime);
					taskPeriod.AverageAfterTaskTime = TimeSpan.FromSeconds(forecastRow.AfterTaskTime);
				}

			}
		}
	}
}