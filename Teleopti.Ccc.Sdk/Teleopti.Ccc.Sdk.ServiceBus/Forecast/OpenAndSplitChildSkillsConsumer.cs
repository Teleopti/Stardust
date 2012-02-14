using System;
using System.Globalization;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class OpenAndSplitChildSkillsConsumer : ConsumerOf<OpenAndSplitChildSkills>
	{
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOpenAndSplitSkillCommand _command;
		private readonly ISkillRepository _skillRepository;
		private readonly IRepository<IJobResult> _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBroker _messageBroker;
		private readonly IServiceBus _serviceBus;

		public OpenAndSplitChildSkillsConsumer(IUnitOfWorkFactory unitOfWorkFactory, IOpenAndSplitSkillCommand command, ISkillRepository skillRepository, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBroker messageBroker, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_command = command;
			_skillRepository = skillRepository;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_serviceBus = serviceBus;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Error(System.String,System.Exception)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Warning(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Forecasting.Export.IJobResultFeedback.Info(System.String)")]
		public void Consume(OpenAndSplitChildSkills message)
		{
		    var stepIncremental = message.IncreaseProgressBy/2;
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
			    var multisiteSkill = _skillRepository.Get(message.MultisiteSkillSelections.MultisiteSkillId);
				_feedback.SetJobResult(jobResult, _messageBroker);
				_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Open and split target skills for skill {0} period {1}.",
                                                       multisiteSkill.Name, message.Period));
				_feedback.Info(string.Format(CultureInfo.InvariantCulture,
                                             "Incoming number of child skills for multisite skill {0}: {1}.", multisiteSkill.Name,
											 message.MultisiteSkillSelections.ChildSkillSelections.Count()));
                _feedback.ReportProgress(stepIncremental,
										 string.Format(CultureInfo.InvariantCulture, "Open and split target skills for skill {0} period {1}.",
                                                       multisiteSkill.Name, message.Period));

				using (unitOfWork.DisableFilter(QueryFilter.BusinessUnit))
				{
					foreach (var childSkill in message.MultisiteSkillSelections.ChildSkillSelections)
					{
						var skill = _skillRepository.Get(childSkill.TargetSkillId);
						if (!skill.WorkloadCollection.Any())
						{
							_feedback.Warning(string.Format(CultureInfo.InvariantCulture,
															"The skill must have at least one workload. (Ignored skill: {0}).",
															skill.Name));
							continue;
						}

						_command.Execute(skill, message.Period);
					}

                    try
                    {
                        endProcessing(unitOfWork);
                    }
                    catch (Exception exception)
                    {
                        _feedback.Error("An error occurred while running export.", exception);
                        _feedback.ReportProgress(0, string.Format(CultureInfo.InvariantCulture,
                                                               "An error occurred while running export."));
                    }
				}
			}

			_serviceBus.Send(new ExportMultisiteSkillToSkill
			{
				BusinessUnitId = message.BusinessUnitId,
				Datasource = message.Datasource,
				JobId = message.JobId,
				MultisiteSkillSelections = message.MultisiteSkillSelections,
				OwnerPersonId = message.OwnerPersonId,
				Period = message.Period,
				Timestamp = message.Timestamp,
                IncreaseProgressBy = message.IncreaseProgressBy - stepIncremental
			});

			_unitOfWorkFactory = null;
		}

		private void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
			_feedback.Dispose();
		}
	}
}