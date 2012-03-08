using System;
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
				    var savedPeriods = new HashSet<DateTimePeriod>();
					foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
					{
						ISkillStaffPeriod skillStaffPeriod;

						if (sourceSkillStaffPeriods.TryGetValue(skillDataPeriod.Period, out skillStaffPeriod))
						{
                            if (savedPeriods.Contains(skillDataPeriod.Period)) continue;
						    savedPeriods.Add(skillDataPeriod.Period);
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

        public void Execute(DateOnly date, ISkill targetSkill, ICollection<IForecastsFileRow> forecasts, ImportForecastsMode importMode)
        {
            var defaultScenario = _scenarioProvider.DefaultScenario(targetSkill.BusinessUnit);
            var dateOnlyPeriod = new DateOnlyPeriod(date, date);
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

                var workloadDay = workloadDays.FirstOrDefault(d => d.CurrentDate.Equals(date));
                if (workloadDay == null) break;
                var skillDay = (ISkillDay) workloadDay.Parent;
                workloadDay.Lock();
                foreach (var skillDataPeriod in skillDay.SkillDataPeriodCollection)
                {
                    var period = skillDataPeriod;
                    var forecastRow =
                        forecasts.FirstOrDefault(
                            f => new DateTimePeriod(f.UtcDateTimeFrom, f.UtcDateTimeTo).Equals(period.Period));
                    if (forecastRow == null) continue;
                    if (importMode == ImportForecastsMode.ImportStaffing || importMode == ImportForecastsMode.ImportWorkloadAndStaffing)
                        skillDataPeriod.ManualAgents = forecastRow.Agents;
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