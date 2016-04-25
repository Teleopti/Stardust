﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{

	public class ImportForecastsFileToSkillBase
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ISkillRepository _skillRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IImportForecastsRepository _importForecastsRepository;
		private readonly IForecastsFileContentProvider _contentProvider;
		private readonly IForecastsAnalyzeQuery _analyzeQuery;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IOpenAndSplitTargetSkillHandler _openAndSplitTargetSkillHandler;

		public ImportForecastsFileToSkillBase(ICurrentUnitOfWork currentUnitOfWork,
			ISkillRepository skillRepository,
			IJobResultRepository jobResultRepository,
			IImportForecastsRepository importForecastsRepository,
			IForecastsFileContentProvider contentProvider,
			IForecastsAnalyzeQuery analyzeQuery,
			IJobResultFeedback feedback,
			IMessageBrokerComposite messageBroker,
			IOpenAndSplitTargetSkillHandler openAndSplitTargetSkillHandler)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_importForecastsRepository = importForecastsRepository;
			_contentProvider = contentProvider;
			_analyzeQuery = analyzeQuery;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_openAndSplitTargetSkillHandler = openAndSplitTargetSkillHandler;
		}

		public void Handle(ImportForecastsFileToSkill @event)
		{
			var unitOfWork = _currentUnitOfWork.Current();
			var jobResult = _jobResultRepository.Get(@event.JobId);
			var skill = _skillRepository.Get(@event.TargetSkillId);
			_feedback.SetJobResult(jobResult, _messageBroker);
			if (skill == null)
			{
				logAndReportValidationError(unitOfWork, jobResult, "Skill does not exist.");
				return;
			}
			var timeZone = skill.TimeZone;
			var forecastFile = _importForecastsRepository.Get(@event.UploadedFileId);
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
			_feedback.ReportProgress(1,
				string.Format(CultureInfo.InvariantCulture, "Importing forecasts for skill {0}.", skill.Name));

			var listOfMessages = generateMessages(@event, queryResult);
			_feedback.ChangeTotalProgress(3 + listOfMessages.Count * 4);
			endProcessing(unitOfWork);
			var currentSendingMsg = new OpenAndSplitTargetSkillEvent { Date = new DateTime() };
			try
			{
				listOfMessages.ForEach(m =>
				{
					currentSendingMsg = m;
					_openAndSplitTargetSkillHandler.Handle(m);
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

		private void notifyServiceBusErrors(OpenAndSplitTargetSkillEvent @event, Exception e)
		{
			var uow = _currentUnitOfWork.Current();
			{
				var job = _jobResultRepository.Get(@event.JobId);
				_feedback.SetJobResult(job, _messageBroker);
				var error = string.Format(CultureInfo.InvariantCulture,
					"Import of {0} is failed due to a service bus error: {1}. ", @event.Date,
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

		private static IList<OpenAndSplitTargetSkillEvent> generateMessages(ImportForecastsFileToSkill message,
			IForecastsAnalyzeQueryResult queryResult)
		{
			var listOfMessages = new List<OpenAndSplitTargetSkillEvent>();
			foreach (var date in queryResult.Period.DayCollection())
			{
				var openHours = queryResult.WorkloadDayOpenHours.GetOpenHour(date);
				listOfMessages.Add(new OpenAndSplitTargetSkillEvent
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					LogOnDatasource = message.LogOnDatasource,
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

	[UseOnToggle(Toggles.Wfm_ForecastFileImportOnStardust_37047)]
	public class ImportForecastsFileToSkillStardust : ImportForecastsFileToSkillBase, IHandleEvent<ImportForecastsFileToSkill>, IRunOnStardust
	{
		private readonly DataSourceState _dataSourceState;
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IPersonRepository _personRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		public ImportForecastsFileToSkillStardust(ICurrentUnitOfWork currentUnitOfWork,
			ISkillRepository skillRepository,
			IJobResultRepository jobResultRepository,
			IImportForecastsRepository importForecastsRepository,
			IForecastsFileContentProvider contentProvider,
			IForecastsAnalyzeQuery analyzeQuery,
			IJobResultFeedback feedback,
			IMessageBrokerComposite messageBroker,
			IOpenAndSplitTargetSkillHandler openAndSplitTargetSkillHandler,
			DataSourceState dataSourceState, 
			IDataSourceScope dataSourceScope, 
			IPersonRepository personRepository, 
			IBusinessUnitRepository businessUnitRepository
			)
			: base(
				currentUnitOfWork, skillRepository, jobResultRepository, importForecastsRepository, contentProvider, analyzeQuery,
				feedback, messageBroker, openAndSplitTargetSkillHandler)
		{
			_dataSourceState = dataSourceState;
			_dataSourceScope = dataSourceScope;
			_personRepository = personRepository;
			_businessUnitRepository = businessUnitRepository;
		}

		public new void Handle(ImportForecastsFileToSkill @event)
		{
			//most of this to aspect or attribute later
			using (_dataSourceScope.OnThisThreadUse(@event.LogOnDatasource))
			{
				// before call we need to set up a person as it is logged on
				using (var uow = _dataSourceState.Get().Application.CreateAndOpenUnitOfWork())
				{
					var person = _personRepository.Get(@event.OwnerPersonId);
					var bu = _businessUnitRepository.Get(@event.LogOnBusinessUnitId);
					Thread.CurrentPrincipal = new TeleoptiPrincipal(new TeleoptiIdentity(person.Name.FirstName, _dataSourceState.Get(), bu, null, @event.LogOnDatasource), person);
					base.Handle(@event);
				}

				
			}
		}
	}

	[UseNotOnToggle(Toggles.Wfm_ForecastFileImportOnStardust_37047)]
#pragma warning disable 618
	public class ImportForecastsFileToSkillBus : ImportForecastsFileToSkillBase, IHandleEvent<ImportForecastsFileToSkill>, IRunOnServiceBus
#pragma warning restore 618
	{
		public ImportForecastsFileToSkillBus(ICurrentUnitOfWork currentUnitOfWork,
			ISkillRepository skillRepository,
			IJobResultRepository jobResultRepository,
			IImportForecastsRepository importForecastsRepository,
			IForecastsFileContentProvider contentProvider,
			IForecastsAnalyzeQuery analyzeQuery,
			IJobResultFeedback feedback,
			IMessageBrokerComposite messageBroker,
			IOpenAndSplitTargetSkillHandler openAndSplitTargetSkillHandler)
			: base(
				currentUnitOfWork, skillRepository, jobResultRepository, importForecastsRepository, contentProvider, analyzeQuery,
				feedback, messageBroker, openAndSplitTargetSkillHandler)
		{

		}

		public new void Handle(ImportForecastsFileToSkill @event)
		{
			base.Handle(@event);
		}

	}
}
