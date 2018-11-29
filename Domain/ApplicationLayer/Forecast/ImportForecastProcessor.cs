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
	public interface IImportForecastProcessor
	{
		void Process(ImportForecastProcessorMessage message);
	}
	public class ImportForecastProcessor : IImportForecastProcessor
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ISaveForecastToSkill _saveForecastToSkill;
		private readonly ISkillRepository _skillRepository;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IDisableBusinessUnitFilter _disableBusinessUnitFilter;

		public ImportForecastProcessor(ICurrentUnitOfWork unitOfWork,
			  ISaveForecastToSkill saveForecastToSkill,
			  ISkillRepository skillRepository,
			  IJobResultRepository jobResultRepository,
			  IJobResultFeedback feedback,
			  IMessageBrokerComposite messageBroker,
			  IDisableBusinessUnitFilter disableBusinessUnitFilter)
		{
			_unitOfWork = unitOfWork;
			_saveForecastToSkill = saveForecastToSkill;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_disableBusinessUnitFilter = disableBusinessUnitFilter;
		}

		public void Process(ImportForecastProcessorMessage message)
		{
			var unitOfWork = _unitOfWork.Current();
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
							_saveForecastToSkill.Execute(new DateOnly(message.Date), skill, message.Forecasts, message.ImportMode);
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
		}
	}

}
