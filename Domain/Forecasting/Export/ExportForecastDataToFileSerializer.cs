using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class ExportForecastDataToFileSerializer : IExportForecastDataToFileSerializer
    {
        private const string DateTimeFormat = "yyyyMMdd HH:mm";
        private const string Seperator = ",";

        public IEnumerable<string> SerializeForecastData(ISkill skill, ExportSkillToFileCommandModel model, IEnumerable<ISkillDay> skillDays)
        {
            var loadSkillSchedule = new Dictionary<ISkill, IList<ISkillDay>> { { skill, skillDays.ToList() } };
            var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
            ISkillStaffPeriodDictionary skillStaffPeriods;
            var fileData = new List<string>();

            if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out skillStaffPeriods))
            {
                foreach (var skillStaffPeriod in skillStaffPeriods.Values)
                {
                    var row = new StringBuilder();
                    row.Append(skill.Name);
                    row.Append(Seperator);
                    row.Append(skillStaffPeriod.Period.StartDateTimeLocal(skill.TimeZone).ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    row.Append(Seperator);
                    row.Append(skillStaffPeriod.Period.EndDateTimeLocal(skill.TimeZone).ToString(DateTimeFormat, CultureInfo.InvariantCulture));
                    row.Append(Seperator);

                    if (model.ExportType.Equals(TypeOfExport.Calls) ||
                        model.ExportType.Equals(TypeOfExport.AgentsAndCalls))
                    {
                        row.Append((int)skillStaffPeriod.Payload.TaskData.Tasks);
                        row.Append(Seperator);
                        row.Append(skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds.ToString("F",
                                                                                                           CultureInfo.InvariantCulture));
                        row.Append(Seperator);
                        row.Append(skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds.ToString(
                            "F", CultureInfo.InvariantCulture));

                        if (model.ExportType.Equals(TypeOfExport.AgentsAndCalls))
                        {
                            row.Append(Seperator);
                            row.Append(skillStaffPeriod.Payload.ForecastedIncomingDemand.ToString("F", CultureInfo.InvariantCulture));
                        }
                    }

                    if (model.ExportType.Equals(TypeOfExport.Agents))
                    {
                        row.Append(0);
                        row.Append(Seperator);
                        row.Append(0D);
                        row.Append(Seperator);
                        row.Append(0D);
                        row.Append(Seperator);
                        row.Append(skillStaffPeriod.Payload.ForecastedIncomingDemand.ToString("F", CultureInfo.InvariantCulture));
                    }

                    fileData.Add(row.ToString());
                }
            }

            return fileData;
        }
    }
}