using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class ExportForecastDataToFileSerializer : IExportForecastDataToFileSerializer
    {
        private const string DateTimeFormat = "yyyyMMdd HH:mm";
        private const string Separator = ",";

        public IEnumerable<string> SerializeForecastData(ISkill skill, ExportSkillToFileCommandModel model, IEnumerable<ISkillDay> skillDays)
        {
            var loadSkillSchedule = new Dictionary<ISkill, IEnumerable<ISkillDay>> { { skill, skillDays.ToList() } };
            var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
			var fileData = new List<string>();

            if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out var skillStaffPeriods))
            {
                foreach (var skillStaffPeriod in skillStaffPeriods.Values)
                {
                    var row = new StringBuilder();
                    row.Append(skill.Name);
                    row.Append(Separator);
                    row.Append(skillStaffPeriod.Period.StartDateTimeLocal(skill.TimeZone).ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    row.Append(Separator);
                    row.Append(skillStaffPeriod.Period.EndDateTimeLocal(skill.TimeZone).ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    row.Append(Separator);

                    if (model.ExportType.Equals(TypeOfExport.Calls) ||
                        model.ExportType.Equals(TypeOfExport.AgentsAndCalls))
                    {
	                    row.Append(Math.Round((decimal) skillStaffPeriod.Payload.TaskData.Tasks, 0));
                        row.Append(Separator);
                        row.Append(skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds.ToString("F",
                                                                                                           CultureInfo.InvariantCulture));
                        row.Append(Separator);
                        row.Append(skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds.ToString(
                            "F", CultureInfo.InvariantCulture));

                        if (model.ExportType.Equals(TypeOfExport.AgentsAndCalls))
                        {
                            row.Append(Separator);
                            row.Append(skillStaffPeriod.Payload.ForecastedIncomingDemand.ToString("F", CultureInfo.InvariantCulture));
                        }
                    }

                    if (model.ExportType.Equals(TypeOfExport.Agents))
                    {
                        row.Append(0);
                        row.Append(Separator);
                        row.Append(0D);
                        row.Append(Separator);
                        row.Append(0D);
                        row.Append(Separator);
                        row.Append(skillStaffPeriod.Payload.ForecastedIncomingDemand.ToString("F", CultureInfo.InvariantCulture));
                    }

                    fileData.Add(row.ToString());
                }
            }

            return fileData;
        }
    }
}