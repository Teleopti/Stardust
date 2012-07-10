using System.IO;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class ForecastToFileCommand
    {
        private readonly IImportForecastToSkillCommand _importForecastToSkillCommand; // using this will only send it to the servicebus, not needed since we can implement it here
        private readonly ISkillDayLoadHelper _skillDayLoadHelper; // loads data?
        private readonly IScenarioProvider _scenarioProvider; //Scenario repo?

        public ForecastToFileCommand(IImportForecastToSkillCommand importForecastToSkillCommand, ISkillDayLoadHelper skillDayLoadHelper, IScenarioProvider scenarioProvider, IJobResultFeedback feedback)
        {
            _importForecastToSkillCommand = importForecastToSkillCommand;
            _skillDayLoadHelper = skillDayLoadHelper;
            _scenarioProvider = scenarioProvider;
        }

        public void Execute(ISkill skill, IScenario scenario, DateOnlyPeriod period, string filePath)
        {
            // not sure if this will acually get all the data needed
            var loadSkillSchedule = _skillDayLoadHelper.LoadSchedulerSkillDays(period, new[] {skill}, scenario);
            // this seem to only sort/fix gathered data but is needed to create the dictionary
            var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
            
            ISkillStaffPeriodDictionary skillStaffPeriods;
            if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out skillStaffPeriods))
            {
                var result = new List<string>();

                foreach (var skillStaffPeriod in skillStaffPeriods.Values)
                {
                    result.Add(new ForecastsRow
                    {
                        LocalDateTimeFrom = skillStaffPeriod.Period.StartDateTimeLocal(skill.TimeZone),
                        LocalDateTimeTo = skillStaffPeriod.Period.EndDateTimeLocal(skill.TimeZone),
                        UtcDateTimeFrom = skillStaffPeriod.Period.StartDateTime,
                        UtcDateTimeTo = skillStaffPeriod.Period.EndDateTime,
                        SkillName = skill.Name,
                        Tasks = (int)skillStaffPeriod.Payload.TaskData.Tasks,
                        TaskTime = skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds,
                        AfterTaskTime = skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
                        Agents = skillStaffPeriod.Payload.ForecastedIncomingDemand,
                        Shrinkage = skillStaffPeriod.Payload.Shrinkage.Value
                    } + System.Environment.NewLine);
                }

                File.WriteAllLines(filePath, result.ToArray());
            }
        }
    }
}
