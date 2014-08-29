using System.Collections.Generic;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class ExportMultisiteSkillsToSkillConsumer : ConsumerOf<ExportMultisiteSkillsToSkill>
	{
		private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepository<IJobResult> _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IServiceBus _serviceBus;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ExportMultisiteSkillsToSkillConsumer));

		public ExportMultisiteSkillsToSkillConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_serviceBus = serviceBus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ExportMultisiteSkillsToSkill message)
		{
			using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var jobResult = _jobResultRepository.Get(message.JobId);
				LazyLoadingManager.Initialize(jobResult.Details);

				if (jobResult.FinishedOk)
				{
					Logger.InfoFormat("The export with job id {0} was already processed.", message.JobId);
					return;
				}

				_feedback.SetJobResult(jobResult, _messageBroker);
				_feedback.ReportProgress(1, "Starting export...");
                unitOfWork.PersistAll();
			}

            var listOfMessages = new List<ExportMultisiteSkillToSkill>();
		    var progressSteps = 1;
            foreach (var multisiteSkillSelection in message.MultisiteSkillSelections)
            {
                foreach (var dateOnlyPeriod in message.Period.Split(20))
                {
                    listOfMessages.Add(new ExportMultisiteSkillToSkill
                                           {
                                               BusinessUnitId = message.BusinessUnitId,
                                               Datasource = message.Datasource,
                                               JobId = message.JobId,
                                               MultisiteSkillSelections = multisiteSkillSelection,
                                               OwnerPersonId = message.OwnerPersonId,
                                               Period = dateOnlyPeriod,
                                               Timestamp = message.Timestamp
                                           });
                    progressSteps++;
                }
                progressSteps += multisiteSkillSelection.ChildSkillSelections.Count*message.Period.DayCount()*4;
            }
            _feedback.ChangeTotalProgress(progressSteps);

			listOfMessages.ForEach(m=> _serviceBus.Send(m));

            _feedback.Dispose();
			_unitOfWorkFactory = null;
		}
	}
}