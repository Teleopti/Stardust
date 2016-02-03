using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class ImportForecastsFileToSkillHandler : IHandleEvent<ImportForecastsFileToSkill>, IRunOnServiceBus
	{
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISkillRepository _skillRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IImportForecastsRepository _importForecastsRepository;
		private readonly IForecastsFileContentProvider _contentProvider;
		private readonly IForecastsAnalyzeQuery _analyzeQuery;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IEventPublisher _eventPublisher;

		public ImportForecastsFileToSkillHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			 ISkillRepository skillRepository,
			 IJobResultRepository jobResultRepository,
			 IImportForecastsRepository importForecastsRepository,
			 IForecastsFileContentProvider contentProvider,
			 IForecastsAnalyzeQuery analyzeQuery,
			 IJobResultFeedback feedback,
			 IMessageBrokerComposite messageBroker,
			 IEventPublisher eventPublisher)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_importForecastsRepository = importForecastsRepository;
			_contentProvider = contentProvider;
			_analyzeQuery = analyzeQuery;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_eventPublisher = eventPublisher;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Info(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ImportForecastsFileToSkill message)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
				var skill = _skillRepository.Get(message.TargetSkillId);
				_feedback.SetJobResult(jobResult, _messageBroker);
				if (skill == null)
				{
					logAndReportValidationError(unitOfWork, jobResult, "Skill does not exist.");
					return;
				}
				var timeZone = skill.TimeZone;
				var forecastFile = _importForecastsRepository.Get(message.UploadedFileId);
				if (forecastFile == null)
				{
					logAndReportValidationError(unitOfWork, jobResult, "The uploaded file has no content.");
					return;
				}
				_feedback.ReportProgress(2, string.Format(CultureInfo.InvariantCulture, "Validating..."));
				IForecastsAnalyzeQueryResult queryResult;
				try
				{
					var content = _contentProvider.LoadContent(forecastFile.FileContent, timeZone);
					queryResult = _analyzeQuery.Run(content, skill);
					if (!queryResult.Succeeded)
					{
						logAndReportValidationError(unitOfWork, jobResult, queryResult.ErrorMessage);
						return;
					}
				}
				catch (ValidationException exception)
				{
					logAndReportValidationError(unitOfWork, jobResult, exception.Message);
					return;
				}
				jobResult.Period = queryResult.Period;

				_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}...", skill.Name));
				_feedback.ReportProgress(1, string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}.", skill.Name));

				var listOfMessages = generateMessages(message, queryResult);
				_feedback.ChangeTotalProgress(3 + listOfMessages.Count * 4);
				endProcessing(unitOfWork);
				var currentSendingMsg = new OpenAndSplitTargetSkill { Date = new DateTime() };
				try
				{
					listOfMessages.ForEach(m =>
														{
															currentSendingMsg = m;
															_eventPublisher.Publish(m);
														});
				}
				catch (SerializationException e)
				{
					notifyServiceBusErrors(currentSendingMsg, e);
				}
				catch (Exception e)
				{
					notifyServiceBusErrors(currentSendingMsg, e);
				}
			}
		}

		private void notifyServiceBusErrors(OpenAndSplitTargetSkill message, Exception e)
		{
			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var job = _jobResultRepository.Get(message.JobId);
				_feedback.SetJobResult(job, _messageBroker);
				var error = string.Format(CultureInfo.InvariantCulture,
												  "Import of {0} is failed due to a service bus error: {1}. ", message.Date,
												  e.Message);
				_feedback.Error(error);
				_feedback.ReportProgress(0, error);
				uow.Clear();
				uow.Merge(job);
				uow.PersistAll();
			}
		}

		private void logAndReportValidationError(IUnitOfWork unitOfWork, IJobResult jobResult, string errorMessage)
		{
			var error = string.Format(CultureInfo.InvariantCulture, "Validation error! {0}", errorMessage);
			_feedback.Error(error);
			_feedback.ReportProgress(0, error);
			unitOfWork.Clear();
			unitOfWork.Merge(jobResult);
			endProcessing(unitOfWork);
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
					Date = date.Date,
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

		private static void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
		}
	}
}
