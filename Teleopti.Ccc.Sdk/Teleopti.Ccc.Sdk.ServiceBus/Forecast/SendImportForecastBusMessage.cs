using System;
using System.Collections.Generic;
using System.Globalization;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class SendImportForecastBusMessage : ISendBusMessage
    {
        private readonly IForecastsAnalyzeQuery _analyzeQuery;
        private readonly IJobResultFeedback _feedback;
        private readonly IServiceBus _serviceBus;

        public SendImportForecastBusMessage(IForecastsAnalyzeQuery analyzeQuery, IJobResultFeedback feedback, IServiceBus serviceBus)
        {
            _analyzeQuery = analyzeQuery;
            _feedback = feedback;
            _serviceBus = serviceBus;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void Process(IEnumerable<IForecastsRow> importForecast, ISkill targetSkill)
        {
            var result = _analyzeQuery.Run(importForecast, targetSkill.MidnightBreakOffset);
            _feedback.Info(string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}...", targetSkill.Name));
            _feedback.ReportProgress(0, string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}.", targetSkill.Name));

            var identity = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
            var listOfMessages =
                generateMessages(
                    new OpenAndSplitTargetSkill
                        {
                            ImportMode = ImportForecastsMode.ImportWorkloadAndStaffing,
                            BusinessUnitId = targetSkill.BusinessUnit.Id.GetValueOrDefault(),
                            Datasource = identity.DataSource.DataSourceName,
                            Timestamp = DateTime.UtcNow,
                            TargetSkillId = targetSkill.Id.GetValueOrDefault(),
                            JobId = _feedback.JobId()
                        }, result);
            
            listOfMessages.ForEach(m => _serviceBus.Send(m));
        }

        private static IEnumerable<OpenAndSplitTargetSkill> generateMessages(OpenAndSplitTargetSkill messageTemplate,
                                                                       IForecastsAnalyzeQueryResult queryResult)
        {
            var listOfMessages = new List<OpenAndSplitTargetSkill>();
            foreach (var date in queryResult.Period.DayCollection())
            {
                var openHours = queryResult.WorkloadDayOpenHours.GetOpenHour(date);
                listOfMessages.Add(new OpenAndSplitTargetSkill
                                       {
                                           BusinessUnitId = messageTemplate.BusinessUnitId,
                                           Datasource = messageTemplate.Datasource,
                                           JobId = messageTemplate.JobId,
                                           OwnerPersonId = messageTemplate.OwnerPersonId,
                                           Date = date,
                                           Timestamp = messageTemplate.Timestamp,
                                           TargetSkillId = messageTemplate.TargetSkillId,
                                           StartOpenHour = openHours.StartTime,
                                           EndOpenHour = openHours.EndTime,
                                           Forecasts = queryResult.ForecastFileContainer.GetForecastsRows(date),
                                           ImportMode = messageTemplate.ImportMode
                                       });
            }
            return listOfMessages;
        }

    }
}