using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public interface IOpenAndSplitTargetSkill
	{
		void Process(OpenAndSplitTargetSkillMessage message);
	}

	public class OpenAndSplitTargetSkill : IOpenAndSplitTargetSkill
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IOpenAndSplitSkillCommand _command;
		private readonly ISkillRepository _skillRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IImportForecastProcessor _importForecastProcessor;
		private readonly IDisableBusinessUnitFilter _disableBusinessUnitFilter;

		public OpenAndSplitTargetSkill(ICurrentUnitOfWork unitOfWork,
			IOpenAndSplitSkillCommand command, ISkillRepository skillRepository, IJobResultRepository jobResultRepository,
			IJobResultFeedback feedback, IMessageBrokerComposite messageBroker, IImportForecastProcessor importForecastProcessor,
			IDisableBusinessUnitFilter disableBusinessUnitFilter)
		{
			_unitOfWork = unitOfWork;
			_command = command;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_importForecastProcessor = importForecastProcessor;
			_disableBusinessUnitFilter = disableBusinessUnitFilter;
		}

		public void Process(OpenAndSplitTargetSkillMessage message)
		{
			var unitOfWork = _unitOfWork.Current();
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
				var targetSkill = _skillRepository.Get(message.TargetSkillId);
				_feedback.SetJobResult(jobResult, _messageBroker);
				var stepMessage = string.Format(CultureInfo.InvariantCulture, "Open and split target skill {0} on {1}.",
														  targetSkill.Name, message.Date);
				_feedback.Info(stepMessage);
				_feedback.ReportProgress(1, stepMessage);
				using (_disableBusinessUnitFilter.Disable())
				{
					var openHours = new TimePeriod(message.StartOpenHour, message.EndOpenHour);
					var dateOnly = new DateOnly(message.Date);
					try
					{
						_command.Execute(targetSkill, new DateOnlyPeriod(dateOnly, dateOnly), new[] { openHours });
					}
					catch (Exception exception)
					{
						stepMessage = string.Format(CultureInfo.InvariantCulture,
																  "An error occurred while running import to {0} on {1}", targetSkill.Name, dateOnly);
						_feedback.Error(stepMessage, exception);
						_feedback.ReportProgress(0, stepMessage);
						unitOfWork.Clear();
						unitOfWork.Merge(jobResult);
						endProcessing(unitOfWork);
						return;
					}
					_feedback.ReportProgress(1, string.Format(CultureInfo.InvariantCulture, "Open and split target skill {0} on {1} done.",
																		targetSkill.Name, message.Date));
					endProcessing(unitOfWork);
				}
				var currentSendingMsg = new ImportForecastProcessorMessage { Date = new DateTime() };
				try
				{
					currentSendingMsg = new ImportForecastProcessorMessage
					{
						LogOnBusinessUnitId = message.LogOnBusinessUnitId,
						LogOnDatasource = message.LogOnDatasource,
						JobId = message.JobId,
						OwnerPersonId = message.OwnerPersonId,
						Date = message.Date,
						TargetSkillId = message.TargetSkillId,
						Forecasts = message.Forecasts,
						Timestamp = message.Timestamp,
						ImportMode = message.ImportMode
					};
					_importForecastProcessor.Process(currentSendingMsg);
				}
				catch (Exception e)
				{
					notifyServiceBusErrors(currentSendingMsg, e);
				}
			}
		}

		private void notifyServiceBusErrors(ImportForecastProcessorMessage message, Exception e)
		{
			using (var uow = _unitOfWork.Current())
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

		private static void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
		}
	}
}
