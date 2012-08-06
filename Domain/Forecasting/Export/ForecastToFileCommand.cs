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
        private readonly ISkillDayLoadHelper _skillDayLoadHelper; 
        private readonly IScenarioProvider _scenarioProvider;
        private readonly IJobResultFeedback _feedback;
        private ISkill _skill;

        public ForecastToFileCommand(ISkillDayLoadHelper skillDayLoadHelper, IScenarioProvider scenarioProvider, IJobResultFeedback feedback)
        {
            _skillDayLoadHelper = skillDayLoadHelper;
            _scenarioProvider = scenarioProvider;
            _feedback = feedback;
        }

        public void Execute(IForecastExportSelection forecastExportSelection)
        {
            if (forecastExportSelection.ForecastSkillForExport == null) return;

            foreach (var forecastSkill in forecastExportSelection.ForecastSkillForExport)
            {
                _skill = forecastSkill;
            }

            var loadSkillSchedule = _skillDayLoadHelper.LoadSchedulerSkillDays(forecastExportSelection.Period,
                                                                               forecastExportSelection.
                                                                                   ForecastSkillForExport,
                                                                               forecastExportSelection.Scenario);
            var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
            ISkillStaffPeriodDictionary skillStaffPeriods;

            if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(_skill, out skillStaffPeriods))
            {
                var result = new List<string>();

                foreach (var skillStaffPeriod in skillStaffPeriods.Values)
                {
                    var row = new ForecastsRow
                    {
                        LocalDateTimeFrom = skillStaffPeriod.Period.StartDateTimeLocal(_skill.TimeZone),
                        LocalDateTimeTo = skillStaffPeriod.Period.EndDateTimeLocal(_skill.TimeZone),
                        UtcDateTimeFrom = skillStaffPeriod.Period.StartDateTime,
                        UtcDateTimeTo = skillStaffPeriod.Period.EndDateTime,
                        SkillName = _skill.Name,
                    };

                    if (forecastExportSelection.TypeOfExport.Equals(ExportForecastsMode.Calls) || forecastExportSelection.TypeOfExport.Equals(ExportForecastsMode.AgentAndCalls))
                    {
                        row.Tasks = (int)skillStaffPeriod.Payload.TaskData.Tasks;
                        row.TaskTime = skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds;
                        row.AfterTaskTime = skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds;
                    }

                    if (forecastExportSelection.TypeOfExport.Equals(ExportForecastsMode.Agent) || forecastExportSelection.TypeOfExport.Equals(ExportForecastsMode.AgentAndCalls))
                    {
                        row.Agents = skillStaffPeriod.Payload.ForecastedIncomingDemand;
                    }
                    result.Add(row + System.Environment.NewLine);
                }

                File.WriteAllLines(forecastExportSelection.FilePath + "\\" + forecastExportSelection.FileName, result.ToArray());

            }
        }
    }
}
