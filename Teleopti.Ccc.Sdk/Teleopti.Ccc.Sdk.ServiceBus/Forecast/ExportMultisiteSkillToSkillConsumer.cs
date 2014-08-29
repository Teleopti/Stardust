using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class ExportMultisiteSkillToSkillConsumer : ConsumerOf<ExportMultisiteSkillToSkill>
	{
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISkillRepository _skillRepository;
		private readonly IRepository<IJobResult> _jobResultRepository;
		private readonly IMultisiteForecastToSkillCommand _command;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;

		public ExportMultisiteSkillToSkillConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory, ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IMultisiteForecastToSkillCommand command, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_command = command;
			_feedback = feedback;
			_messageBroker = messageBroker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Info(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Error(System.String,System.Exception)")]
		public void Consume(ExportMultisiteSkillToSkill message)
		{
			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);

				_feedback.SetJobResult(jobResult, _messageBroker);
				_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Export forecasts to target skills for skill {0} period {1}.",
				                             message.MultisiteSkillSelections.MultisiteSkillId, message.Period));
				_feedback.Info(string.Format(CultureInfo.InvariantCulture,
				                             "Incoming number of child skills for multisite skill {0}: {1}.", message.MultisiteSkillSelections.MultisiteSkillId,
				                             message.MultisiteSkillSelections.ChildSkillSelections.Count()));
			    var multisteSkill = (IMultisiteSkill) _skillRepository.Get(message.MultisiteSkillSelections.MultisiteSkillId);
                _feedback.ReportProgress(1,string.Format(CultureInfo.InvariantCulture, "Export forecasts to target skills for skill {0} period {1}.",
				                                       multisteSkill.Name, message.Period));

				using (unitOfWork.DisableFilter(QueryFilter.BusinessUnit))
				{
					var stopwatch = new Stopwatch();
					stopwatch.Start();
					var settings = prepareSettingsFromMessage(message);
					try
					{
						_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Processing export for multisite skills {0}.",
						                             string.Join(",",
						                                         settings.MultisiteSkillsForExport.Select(m => m.MultisiteSkill.Name).
						                                         	ToArray())));
						_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Processing export for period {0}.", settings.Period));
						_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Number of child skills for each multisite skill: {0}.",
						                             string.Join(",",
						                                         settings.MultisiteSkillsForExport.Select(
						                                         	m => m.SubSkillMapping.Count().ToString(CultureInfo.InvariantCulture)).ToArray())));
						_command.Execute(settings);
					}
					catch (Exception exception)
					{
					    var stepMessage = string.Format(CultureInfo.InvariantCulture, "An error occurred while running export {0} for {1}",
					                                    settings.MultisiteSkillsForExport.Select(m => m.MultisiteSkill.Name).
					                                        ToArray(), settings.Period);
                        _feedback.Error(stepMessage, exception);
                        _feedback.ReportProgress(0, stepMessage);
						unitOfWork.Clear();
						unitOfWork.Merge(jobResult);
						endProcessing(unitOfWork);
						return;
					}
					stopwatch.Stop();
					_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Processing export for multisite skill took {0}.",
					                             stopwatch.Elapsed));
					jobResult.FinishedOk = true;

					endProcessing(unitOfWork);
				}
			}
			_unitOfWorkFactory = null;
		}

		private void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
			_feedback.Dispose();
		}

		private SkillExportSelection prepareSettingsFromMessage(ExportMultisiteSkillToSkill message)
		{
			var selections = new List<IMultisiteSkillForExport>();

			var export = new MultisiteSkillForExport
			             	{
			             		MultisiteSkill =
			             			(IMultisiteSkill)_skillRepository.Get(message.MultisiteSkillSelections.MultisiteSkillId)
			             	};
			foreach (var childSkillSelection in message.MultisiteSkillSelections.ChildSkillSelections)
			{
				export.AddSubSkillMapping(new SkillExportCombination
				                          	{
				                          		SourceSkill =
				                          			(IChildSkill)_skillRepository.Get(childSkillSelection.SourceSkillId),
				                          		TargetSkill = _skillRepository.Get(childSkillSelection.TargetSkillId)
				                          	});
			}
			selections.Add(export);

		    var selection = new SkillExportSelection(selections) {Period = message.Period};
		    return selection;
		}
	}
}