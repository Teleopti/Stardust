﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public class SaveForecastToSkillCommand : ISaveForecastToSkillCommand
	{
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IScenarioProvider _scenarioProvider;
		private readonly WorkloadDayHelper _workloadDayHelper = new WorkloadDayHelper();

		public SaveForecastToSkillCommand(ISkillDayLoadHelper skillDayLoadHelper, ISkillDayRepository skillDayRepository, IScenarioProvider scenarioProvider)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
			_skillDayRepository = skillDayRepository;
			_scenarioProvider = scenarioProvider;
		}

        public void Execute(DateOnly dateOnly, ISkill targetSkill, ICollection<IForecastsRow> forecasts, ImportForecastsMode importMode)
        {
            var defaultScenario = _scenarioProvider.DefaultScenario(targetSkill.BusinessUnit);
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnly, dateOnly);
            var skillDayDictionary =
                _skillDayLoadHelper.LoadSchedulerSkillDays(dateOnlyPeriod,
                                                           new[] {targetSkill}, defaultScenario);
            foreach (var skillDayList in skillDayDictionary)
            {
                var newSkillDayList = _skillDayRepository.GetAllSkillDays(dateOnlyPeriod,
                                                                          skillDayList.Value, skillDayList.Key,
                                                                          defaultScenario, false);
                var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(newSkillDayList,
                                                                                   targetSkill.WorkloadCollection.First());

                var workloadDay = workloadDays.FirstOrDefault(d => d.CurrentDate.Equals(dateOnly));
                if (workloadDay == null) continue;
                var skillDay = (ISkillDay) workloadDay.Parent;
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
}