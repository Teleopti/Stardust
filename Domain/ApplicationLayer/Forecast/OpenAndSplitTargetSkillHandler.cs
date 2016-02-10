using System;
using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class OpenAndSplitTargetSkillBase 
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOpenAndSplitSkillCommand _command;
		private readonly ISkillRepository _skillRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IEventPublisher _eventPublisher;
		private readonly IDisableBusinessUnitFilter _disableBusinessUnitFilter;

		public OpenAndSplitTargetSkillBase(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IOpenAndSplitSkillCommand command, ISkillRepository skillRepository, IJobResultRepository jobResultRepository,
			IJobResultFeedback feedback, IMessageBrokerComposite messageBroker, IEventPublisher eventPublisher,
			IDisableBusinessUnitFilter disableBusinessUnitFilter)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_command = command;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_eventPublisher = eventPublisher;
			_disableBusinessUnitFilter = disableBusinessUnitFilter;
		}

		public void Handle(OpenAndSplitTargetSkill message)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
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
				var currentSendingMsg = new ImportForecastsToSkill { Date = new DateTime() };
				try
				{
					currentSendingMsg = new ImportForecastsToSkill
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
					_eventPublisher.Publish(currentSendingMsg);
				}
				catch (Exception e)
				{
					notifyServiceBusErrors(currentSendingMsg, e);
				}
			}
		}

		private void notifyServiceBusErrors(ImportForecastsToSkill message, Exception e)
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

		private static void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
		}
	}

	[UseOnToggle(Toggles.Wfm_ForecastFileImportOnStardust_37047)]
	public class OpenAndSplitTargetSkillStardust : OpenAndSplitTargetSkillBase, IHandleEvent<OpenAndSplitTargetSkill>, IRunOnStardust
	{
		public OpenAndSplitTargetSkillStardust(ICurrentUnitOfWorkFactory unitOfWorkFactory, IOpenAndSplitSkillCommand command,
			ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback,
			IMessageBrokerComposite messageBroker, IEventPublisher eventPublisher,
			IDisableBusinessUnitFilter disableBusinessUnitFilter)
			: base(
				unitOfWorkFactory, command, skillRepository, jobResultRepository, feedback, messageBroker, eventPublisher,
				disableBusinessUnitFilter)
		{
		}

		public new void Handle(OpenAndSplitTargetSkill  @event)
		{ base.Handle(@event);}
	}

	[UseNotOnToggle(Toggles.Wfm_ForecastFileImportOnStardust_37047)]
	public class OpenAndSplitTargetSkillBus : OpenAndSplitTargetSkillBase, IHandleEvent<OpenAndSplitTargetSkill>, IRunOnServiceBus
	{
		public OpenAndSplitTargetSkillBus(ICurrentUnitOfWorkFactory unitOfWorkFactory, IOpenAndSplitSkillCommand command,
			ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback,
			IMessageBrokerComposite messageBroker, IEventPublisher eventPublisher,
			IDisableBusinessUnitFilter disableBusinessUnitFilter)
			: base(
				unitOfWorkFactory, command, skillRepository, jobResultRepository, feedback, messageBroker, eventPublisher,
				disableBusinessUnitFilter)
		{
		}

		public new void Handle(OpenAndSplitTargetSkill @event)
		{ base.Handle(@event); }
	}
}
