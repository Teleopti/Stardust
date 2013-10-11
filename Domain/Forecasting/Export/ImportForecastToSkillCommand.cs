using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public interface IImportForecastToSkillCommand
    {
        void Execute(ISkill sourceSkill, ISkill targetSkill, ISkillStaffPeriodDictionary skillStaffPeriods, DateOnlyPeriod period);
    }

    public class ImportForecastToSkillCommand : IImportForecastToSkillCommand
    {
        private readonly ISendBusMessage _sendBusMessage;

        public ImportForecastToSkillCommand(ISendBusMessage sendBusMessage)
        {
            _sendBusMessage = sendBusMessage;
        }

        public void Execute(ISkill sourceSkill, ISkill targetSkill, ISkillStaffPeriodDictionary skillStaffPeriods, DateOnlyPeriod period)
        {
            var result = new List<IForecastsRow>();
            foreach (var skillStaffPeriod in skillStaffPeriods.Values)
            {
                result.Add(new ForecastsRow
                               {
                                   LocalDateTimeFrom = skillStaffPeriod.Period.StartDateTimeLocal(sourceSkill.TimeZone),
                                   LocalDateTimeTo = skillStaffPeriod.Period.EndDateTimeLocal(sourceSkill.TimeZone),
                                   UtcDateTimeFrom = skillStaffPeriod.Period.StartDateTime,
                                   UtcDateTimeTo = skillStaffPeriod.Period.EndDateTime,
                                   SkillName = sourceSkill.Name,
                                   Tasks = skillStaffPeriod.Payload.TaskData.Tasks,
                                   TaskTime = skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds,
                                   AfterTaskTime = skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
                                   Agents = skillStaffPeriod.Payload.ForecastedIncomingDemand,
                                   Shrinkage = skillStaffPeriod.Payload.Shrinkage.Value
                               });
            }

            _sendBusMessage.Process(result, targetSkill, period);
        }
    }
}