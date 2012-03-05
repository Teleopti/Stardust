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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public void Execute(DateOnlyPeriod period, ISkill targetSkill, ISkillStaffPeriodDictionary sourceSkillStaffPeriods)
		{
			var defaultScenario = _scenarioProvider.DefaultScenario(targetSkill.BusinessUnit);
			var skillDayDictionary = _skillDayLoadHelper.LoadSchedulerSkillDays(period, new[] { targetSkill }, defaultScenario);
			foreach (var skillDayList in skillDayDictionary)
			{
				var newSkillDayList = _skillDayRepository.GetAllSkillDays(period, skillDayList.Value, skillDayList.Key, defaultScenario, false);
				var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(newSkillDayList,
																				   targetSkill.WorkloadCollection.First());

				foreach (var workloadDay in workloadDays)
				{
					var skillDay = (ISkillDay)workloadDay.Parent;
					workloadDay.Lock();
					foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
					{
						ISkillStaffPeriod skillStaffPeriod;

						if (sourceSkillStaffPeriods.TryGetValue(skillDataPeriod.Period, out skillStaffPeriod))
						{
							skillDataPeriod.ManualAgents = skillStaffPeriod.Payload.ForecastedIncomingDemand;
							skillDataPeriod.Shrinkage = skillStaffPeriod.Payload.Shrinkage;

							var taskPeriod =
								workloadDay.TaskPeriodList.FirstOrDefault(
									p => p.Period.StartDateTime == skillStaffPeriod.Period.StartDateTime);
							if (taskPeriod != null)
							{
								taskPeriod.Tasks = skillStaffPeriod.Payload.TaskData.Tasks;
								taskPeriod.AverageTaskTime = skillStaffPeriod.Payload.TaskData.AverageTaskTime;
								taskPeriod.AverageAfterTaskTime = skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime;
							}
						}
					}
				}
			}
		}

        public void Execute(DateOnly date, ISkill targetSkill, ICollection<IForecastsFileRow> forecasts)
        {
            var defaultScenario = _scenarioProvider.DefaultScenario(targetSkill.BusinessUnit);
            var dateOnlyPeriod = new DateOnlyPeriod(date, date.AddDays(1));
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

                var workloadDay = workloadDays.First();
                var skillDay = (ISkillDay) workloadDay.Parent;
                workloadDay.Lock();
                foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
                {
                    var period = skillDataPeriod;
                    var forecastRow = forecasts.FirstOrDefault(f => f.Period.Equals(period.Period));
                    if (forecastRow == null) continue;
                    skillDataPeriod.ManualAgents = forecastRow.Agents;
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