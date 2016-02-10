using System;
using System.Diagnostics.CodeAnalysis;
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
	public class ImportForecastsToSkillBase 
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISaveForecastToSkillCommand _saveForecastToSkillCommand;
		private readonly ISkillRepository _skillRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IDisableBusinessUnitFilter _disableBusinessUnitFilter;

		public ImportForecastsToSkillBase(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			  ISaveForecastToSkillCommand saveForecastToSkillCommand,
			  ISkillRepository skillRepository,
			  IJobResultRepository jobResultRepository,
			  IJobResultFeedback feedback,
			  IMessageBrokerComposite messageBroker,
			  IDisableBusinessUnitFilter disableBusinessUnitFilter)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_saveForecastToSkillCommand = saveForecastToSkillCommand;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_disableBusinessUnitFilter = disableBusinessUnitFilter;
		}

		public void Handle(ImportForecastsToSkill message)
		{
			using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
				var skill = _skillRepository.Get(message.TargetSkillId);
				_feedback.SetJobResult(jobResult, _messageBroker);
				if (skill == null)
				{
					_feedback.Error(string.Format("Skill with Id:{0} does not exist.", message.TargetSkillId));
					_feedback.ReportProgress(0, string.Format(CultureInfo.InvariantCulture,
																		"An error occurred while running import to {0} on {1}.",
																		message.TargetSkillId, message.Date));
					unitOfWork.Clear();
					unitOfWork.Merge(jobResult);
					endProcessing(unitOfWork);
					return;
				}
				var stepMessage = string.Format(CultureInfo.InvariantCulture, "Import forecasts to skill: {0} on {1}.",
															skill.Name, message.Date);
				_feedback.Info(stepMessage);
				_feedback.ReportProgress(1, stepMessage);
				using (_disableBusinessUnitFilter.Disable())
				{
					try
					{
						if (message.Forecasts != null)
							_saveForecastToSkillCommand.Execute(new DateOnly(message.Date), skill, message.Forecasts, message.ImportMode);
					}
					catch (Exception exception)
					{
						stepMessage = string.Format(CultureInfo.InvariantCulture,
															 "An error occurred while running import to {0} on {1}.", skill.Name, message.Date);
						_feedback.Error(stepMessage, exception);
						_feedback.ReportProgress(0, stepMessage);
						unitOfWork.Clear();
						unitOfWork.Merge(jobResult);
						endProcessing(unitOfWork);
						return;
					}
					_feedback.ReportProgress(1, string.Format(CultureInfo.InvariantCulture, "Import forecasts to skill: {0} on {1} succeeded.",
																 skill.Name, message.Date));
					jobResult.FinishedOk = true;
					endProcessing(unitOfWork);
				}
			}
		}

		private void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
			_feedback.Clear();
		}
	}

	[UseNotOnToggle(Toggles.Wfm_ForecastFileImportOnStardust_37047)]
	public class ImportForecastsToSkillBusHander: ImportForecastsToSkillBase, IHandleEvent<ImportForecastsToSkill>, IRunOnServiceBus
	{
		public ImportForecastsToSkillBusHander(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			ISaveForecastToSkillCommand saveForecastToSkillCommand, ISkillRepository skillRepository,
			IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker,
			IDisableBusinessUnitFilter disableBusinessUnitFilter)
			: base(
				unitOfWorkFactory, saveForecastToSkillCommand, skillRepository, jobResultRepository, feedback, messageBroker,
				disableBusinessUnitFilter)
		{
		}

		public new void Handle(ImportForecastsToSkill @event)
		{ base.Handle(@event);}
	}

	[UseOnToggle(Toggles.Wfm_ForecastFileImportOnStardust_37047)]
	public class ImportForecastsToSkillStardustHander : ImportForecastsToSkillBase, IHandleEvent<ImportForecastsToSkill>, IRunOnStardust
	{
		public ImportForecastsToSkillStardustHander(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			ISaveForecastToSkillCommand saveForecastToSkillCommand, ISkillRepository skillRepository,
			IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker,
			IDisableBusinessUnitFilter disableBusinessUnitFilter)
			: base(
				unitOfWorkFactory, saveForecastToSkillCommand, skillRepository, jobResultRepository, feedback, messageBroker,
				disableBusinessUnitFilter)
		{
		}

		public new void Handle(ImportForecastsToSkill @event)
		{ base.Handle(@event); }
	}
}
