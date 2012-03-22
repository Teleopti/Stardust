using System;
using System.Collections.Generic;
using System.Globalization;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;
using Teleopti.Messaging.Coders;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public class ImportForecastsFileToSkillConsumer : ConsumerOf<ImportForecastsFileToSkill>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportForecastsFileToSkillConsumer));
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISkillRepository _skillRepository;
        private readonly IJobResultRepository _jobResultRepository;
        private readonly IImportForecastsRepository _importForecastsRepository;
        private readonly IForecastsFileContentProvider _contentProvider;
        private readonly IJobResultFeedback _feedback;
        private readonly IMessageBroker _messageBroker;
        private readonly IServiceBus _serviceBus;
        private readonly JobResultProgressEncoder _jobResultProgressEncoder;

        public ImportForecastsFileToSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory,
            ISkillRepository skillRepository,
            IJobResultRepository jobResultRepository,
            IImportForecastsRepository importForecastsRepository,
            IForecastsFileContentProvider contentProvider,
            IJobResultFeedback feedback,
            IMessageBroker messageBroker,
            IServiceBus serviceBus)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _skillRepository = skillRepository;
            _jobResultRepository = jobResultRepository;
            _importForecastsRepository = importForecastsRepository;
            _contentProvider = contentProvider;
            _feedback = feedback;
            _messageBroker = messageBroker;
            _serviceBus = serviceBus;
            _jobResultProgressEncoder = new JobResultProgressEncoder();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Info(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Consume(ImportForecastsFileToSkill message)
        {
            using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var targetSkill = _skillRepository.Get(message.TargetSkillId);
                if (targetSkill == null)
                {
                    sendValidationError(message.JobId, "Skill does not exsit.");
                    return;
                }
                var timeZone = targetSkill.TimeZone;
                var forecastFile = _importForecastsRepository.Get(message.UploadedFileId);
                if (forecastFile == null)
                {
                    sendValidationError(message.JobId, "The uploaded file has no content.");
                    return;
                }
                var commandResult =_contentProvider.LoadContent(forecastFile.FileContent, timeZone).Analyze();
                if (!commandResult.Succeeded)
                {
                    sendValidationError(message.JobId, string.Concat("Validation error! ", commandResult.ErrorMessage));
                    return;
                }

                var jobResult = _jobResultRepository.Get(message.JobId);
                jobResult.Period = commandResult.Period;
                var skill = _skillRepository.Get(message.TargetSkillId);
                _feedback.SetJobResult(jobResult, _messageBroker);
                _feedback.Info(string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}...", skill.Name));
                _feedback.ReportProgress(0, string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}.", skill.Name));

                var listOfMessages = generateMessages(message, commandResult);
                _feedback.ChangeTotalProgress(4 + listOfMessages.Count * 4);
                endProcessing(unitOfWork);

                listOfMessages.ForEach(m => _serviceBus.Send(m));
            }
            _unitOfWorkFactory = null;
        }

        private static IList<OpenAndSplitTargetSkill> generateMessages(ImportForecastsFileToSkill message,
                                             IForecastsAnalyzeCommandResult commandResult)
        {
            var listOfMessages = new List<OpenAndSplitTargetSkill>();
            foreach (var date in commandResult.Period.DayCollection())
            {
                var openHours = commandResult.WorkloadDayOpenHours.GetOpenHour(date);
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
                                           Forecasts = commandResult.ForecastFileContainer.GetForecastsRows(date),
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

        private void sendValidationError(Guid jobId, string errorMessage)
        {
            var jobResultProgress = new JobResultProgress
            {
                Message = errorMessage,
                Percentage = 0,
                JobResultId = jobId
            };
            sendMessage(_jobResultProgressEncoder.Encode(jobResultProgress));
        }

        private void sendMessage(byte[] binaryData)
        {
            using (new MessageBrokerSendEnabler())
            {
                if (messageBrokerIsRunning())
                {
                    _messageBroker.SendEventMessage(DateTime.UtcNow, DateTime.UtcNow, Guid.Empty, Guid.Empty,
                                                    typeof(IJobResultProgress), DomainUpdateType.NotApplicable, binaryData);
                }
                else
                {
                    Logger.Warn("Job progress could not be sent because the message broker is unavailable.");
                }
            }
        }

        private bool messageBrokerIsRunning()
        {
            return _messageBroker != null && _messageBroker.IsInitialized;
        }
    }
}
