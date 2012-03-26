using System.Collections.Generic;
using System.Globalization;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ImportForecastsFileToSkillConsumer : ConsumerOf<ImportForecastsFileToSkill>
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISkillRepository _skillRepository;
        private readonly IJobResultRepository _jobResultRepository;
        private readonly IImportForecastsRepository _importForecastsRepository;
        private readonly IForecastsFileContentProvider _contentProvider;
        private readonly IForecastsAnalyzeQuery _analyzeQuery;
        private readonly IJobResultFeedback _feedback;
        private readonly IMessageBroker _messageBroker;
        private readonly IServiceBus _serviceBus;

        public ImportForecastsFileToSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory,
            ISkillRepository skillRepository,
            IJobResultRepository jobResultRepository,
            IImportForecastsRepository importForecastsRepository,
            IForecastsFileContentProvider contentProvider,
            IForecastsAnalyzeQuery analyzeQuery,
            IJobResultFeedback feedback,
            IMessageBroker messageBroker,
            IServiceBus serviceBus)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _skillRepository = skillRepository;
            _jobResultRepository = jobResultRepository;
            _importForecastsRepository = importForecastsRepository;
            _contentProvider = contentProvider;
            _analyzeQuery = analyzeQuery;
            _feedback = feedback;
            _messageBroker = messageBroker;
            _serviceBus = serviceBus;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Info(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(ImportForecastsFileToSkill message)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var jobResult = _jobResultRepository.Get(message.JobId);
                var skill = _skillRepository.Get(message.TargetSkillId);
                _feedback.SetJobResult(jobResult, _messageBroker);
                if (skill == null)
                {
                    const string errorMessage = "Skill does not exist.";
                    _feedback.Error(errorMessage);
                    _feedback.ReportProgress(0, errorMessage);
                    endProcessing(unitOfWork);
                    return;
                }
                var timeZone = skill.TimeZone;
                var forecastFile = _importForecastsRepository.Get(message.UploadedFileId);
                if (forecastFile == null)
                {
                    const string errorMessage = "The uploaded file has no content.";
                    _feedback.Error(errorMessage);
                    _feedback.ReportProgress(0, errorMessage);
                    endProcessing(unitOfWork);
                    return;
                }
                var queryResult = _analyzeQuery.Run(_contentProvider.LoadContent(forecastFile.FileContent, timeZone, skill.MidnightBreakOffset));
                if (!queryResult.Succeeded)
                {
                    var errorMessage = string.Concat("Validation error! ", queryResult.ErrorMessage);
                    _feedback.Error(errorMessage);
                    _feedback.ReportProgress(0, errorMessage);
                    endProcessing(unitOfWork);
                    return;
                }
                jobResult.Period = queryResult.Period;
                
                _feedback.Info(string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}...", skill.Name));
                _feedback.ReportProgress(0, string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}.", skill.Name));

                var listOfMessages = generateMessages(message, queryResult);
                _feedback.ChangeTotalProgress(4 + listOfMessages.Count * 4);
                endProcessing(unitOfWork);

                listOfMessages.ForEach(m => _serviceBus.Send(m));
            }
            _unitOfWorkFactory = null;
        }

        private static IList<OpenAndSplitTargetSkill> generateMessages(ImportForecastsFileToSkill message,
                                             IForecastsAnalyzeQueryResult queryResult)
        {
            var listOfMessages = new List<OpenAndSplitTargetSkill>();
            foreach (var date in queryResult.Period.DayCollection())
            {
                var openHours = queryResult.WorkloadDayOpenHours.GetOpenHour(date);
                listOfMessages.Add(new OpenAndSplitTargetSkill
                                       {
                                           BusinessUnitId = message.BusinessUnitId,
                                           Datasource = message.Datasource,
                                           JobId = message.JobId,
                                           OwnerPersonId = message.OwnerPersonId,
                                           Date = date,
                                           Timestamp = message.Timestamp,
                                           TargetSkillId = message.TargetSkillId,
                                           StartOpenHour = openHours.StartTime,
                                           EndOpenHour = openHours.EndTime,
                                           Forecasts = queryResult.ForecastFileContainer.GetForecastsRows(date),
                                           ImportMode = message.ImportMode
                                       });
            }
            return listOfMessages;
        }

        private void endProcessing(IUnitOfWork unitOfWork)
        {
            unitOfWork.PersistAll();
            _feedback.Dispose();
        }
    }
}
