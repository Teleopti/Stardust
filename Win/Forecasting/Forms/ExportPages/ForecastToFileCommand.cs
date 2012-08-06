using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    public class ForecastToFileCommand
    {
        private readonly ExportSkillToFileCommandModel _model;
        private readonly IEnumerable<ISkillDay> _skillDays;
        private readonly ISkill _skill;
        private const string dateTimeFormat = "yyyyMMdd HH:mm";
        private const string seperator = ",";

        public ForecastToFileCommand(ISkill skill, ExportSkillToFileCommandModel model, IEnumerable<ISkillDay> skillDays)
        {
            _skill = skill;
            _model = model;
            _skillDays = skillDays;
        }

        public void Execute()
        {
            var loadSkillSchedule = new Dictionary<ISkill, IList<ISkillDay>> {{_skill, _skillDays.ToList()}};
            var skillStaffPeriodHolder = new SkillStaffPeriodHolder(loadSkillSchedule);
            ISkillStaffPeriodDictionary skillStaffPeriods;

            if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(_skill, out skillStaffPeriods))
            {
                using (var writter = new StreamWriter(_model.FileName))
                {
                    foreach (var skillStaffPeriod in skillStaffPeriods.Values)
                    {
                        var row = new StringBuilder();
                        row.Append(_skill.Name);
                        row.Append(seperator);
                        row.Append(skillStaffPeriod.Period.StartDateTimeLocal(_skill.TimeZone).ToString(dateTimeFormat,CultureInfo.InvariantCulture));
                        row.Append(seperator);
                        row.Append(skillStaffPeriod.Period.EndDateTimeLocal(_skill.TimeZone).ToString(dateTimeFormat,CultureInfo.InvariantCulture));
                        row.Append(seperator);

                        if (_model.ExportType.Equals(TypeOfExport.Calls) ||
                            _model.ExportType.Equals(TypeOfExport.AgentsAndCalls))
                        {
                            row.Append((int) skillStaffPeriod.Payload.TaskData.Tasks);
                            row.Append(seperator);
                            row.Append(skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds.ToString("F",
                                                                                                               CultureInfo.InvariantCulture));
                            row.Append(seperator);
                            row.Append(skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds.ToString(
                                "F", CultureInfo.InvariantCulture));
                            row.Append(seperator);
                        }

                        if (_model.ExportType.Equals(TypeOfExport.Agents) ||
                            _model.ExportType.Equals(TypeOfExport.AgentsAndCalls))
                        {
                            row.Append(skillStaffPeriod.Payload.ForecastedIncomingDemand.ToString("F",CultureInfo.InvariantCulture));
                        }
                        writter.WriteLine(row.ToString());
                    }
                }
            }
        }
    }
}
