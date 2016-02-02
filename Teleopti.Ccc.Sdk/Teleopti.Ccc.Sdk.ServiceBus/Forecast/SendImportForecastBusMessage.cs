using System;
using System.Collections.Generic;
using System.Globalization;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Info(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void Process(IEnumerable<IForecastsRow> importForecast, ISkill targetSkill, DateOnlyPeriod period)
        {
            var result = _analyzeQuery.Run(importForecast, targetSkill);
            var stepMessage = string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}...", targetSkill.Name);
            _feedback.Info(stepMessage);
            _feedback.ReportProgress(0, stepMessage);

            var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
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
                        }, result, period);
            var currentSendingMsg = new OpenAndSplitTargetSkill { Date = new DateTime() };
            try
            {
                listOfMessages.ForEach(m =>
                                           {
                                               currentSendingMsg = m;
                                               _serviceBus.Send(m);
                                           });
            }
            catch (Exception e)
            {
                var error = string.Format(CultureInfo.InvariantCulture,
                                          "Import of {0} is failed due to a service bus error: {1}. ", currentSendingMsg.Date,
                                          e.Message);
                _feedback.Error(error);
                _feedback.ReportProgress(0, error);
            }
        }

        private static IEnumerable<OpenAndSplitTargetSkill> generateMessages(OpenAndSplitTargetSkill messageTemplate,
                                                                       IForecastsAnalyzeQueryResult queryResult, DateOnlyPeriod period)
        {
            var listOfMessages = new List<OpenAndSplitTargetSkill>();
            foreach (var date in period.DayCollection())
            {
                var openHours = queryResult.WorkloadDayOpenHours.GetOpenHour(date);
                listOfMessages.Add(new OpenAndSplitTargetSkill
                                       {
                                           BusinessUnitId = messageTemplate.BusinessUnitId,
                                           Datasource = messageTemplate.Datasource,
                                           JobId = messageTemplate.JobId,
                                           OwnerPersonId = messageTemplate.OwnerPersonId,
                                           Date = date.Date,
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