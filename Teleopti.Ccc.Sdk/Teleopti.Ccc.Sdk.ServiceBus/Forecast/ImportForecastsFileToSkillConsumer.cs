using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
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
        private readonly IJobResultFeedback _feedback;
        private readonly IMessageBroker _messageBroker;
        private readonly IServiceBus _serviceBus;
        private readonly JobResultProgressEncoder _jobResultProgressEncoder;

        public ImportForecastsFileToSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory,
            ISkillRepository skillRepository,
            IJobResultRepository jobResultRepository,
            IImportForecastsRepository importForecastsRepository,
            IJobResultFeedback feedback,
            IMessageBroker messageBroker,
            IServiceBus serviceBus)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _skillRepository = skillRepository;
            _jobResultRepository = jobResultRepository;
            _importForecastsRepository = importForecastsRepository;
            _feedback = feedback;
            _messageBroker = messageBroker;
            _serviceBus = serviceBus;
            _jobResultProgressEncoder = new JobResultProgressEncoder();
        }

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
                var formatter = new BinaryFormatter();
                IList<CsvFileRow> fileContent;
                using (var stream = new MemoryStream(forecastFile.FileContent))
                {
                    var tempObj = formatter.Deserialize(stream);
                    fileContent = tempObj as List<CsvFileRow>;
                    if (fileContent == null)
                    {
                        sendValidationError(message.JobId, "The uploaded file has no content.");
                        return;
                    }
                }
                var provider = new ForecastsFileContentProvider(fileContent, timeZone);
                provider.LoadContent();
                var analyzeCommand = new ForecastsAnalyzeCommand(provider.Forecasts);
                var commandResult = analyzeCommand.Execute();
                if (!commandResult.Succeeded)
                {
                    sendValidationError(message.JobId, string.Concat("Validation error! ", commandResult.ErrorMessage));
                    return;
                }

                var listOfMessages = new List<OpenAndSplitTargetSkill>();

                var jobResult = _jobResultRepository.Get(message.JobId);
                jobResult.Period = commandResult.Period;
                var skill = _skillRepository.Get(message.TargetSkillId);
                _feedback.SetJobResult(jobResult, _messageBroker);
                _feedback.Info(string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}...",
                                             skill.Name));
                _feedback.ReportProgress(0,
                                         string.Format(CultureInfo.InvariantCulture,
                                                       "Importing forecasts for skill {0}.", skill.Name));

                foreach (var date in commandResult.Period.DayCollection())
                {
                    var openHours = commandResult.WorkloadDayOpenHours.Get(date);
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
                                               Forecasts = commandResult.ForecastFileDictionary.Get(date),
                                               ImportMode = message.ImportMode
                                           });
                }
                _feedback.ChangeTotalProgress(4 + listOfMessages.Count*4);
                endProcessing(unitOfWork);

                listOfMessages.ForEach(m => _serviceBus.Send(m));
            }
            _unitOfWorkFactory = null;
        }

        private void endProcessing(IUnitOfWork unitOfWork)
        {
            unitOfWork.PersistAll();
            _feedback.Dispose();
        }

        private void sendValidationError( Guid jobId, string errorMessage)
        {
            var jobResultProgress = new JobResultProgress
            {
                Message = errorMessage,
                Percentage = 0,
                JobResultId = jobId
            };
            var binaryData = _jobResultProgressEncoder.Encode(jobResultProgress);
            sendMessage(binaryData);
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
