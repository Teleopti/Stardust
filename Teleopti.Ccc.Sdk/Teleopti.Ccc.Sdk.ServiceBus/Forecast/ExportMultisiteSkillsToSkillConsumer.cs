using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class ExportMultisiteSkillsToSkillConsumer : ConsumerOf<ExportMultisiteSkillsToSkill>
	{
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRepository<IJobResult> _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBroker _messageBroker;
		private readonly IServiceBus _serviceBus;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ExportMultisiteSkillsToSkillConsumer));

		public ExportMultisiteSkillsToSkillConsumer(IUnitOfWorkFactory unitOfWorkFactory, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBroker messageBroker, IServiceBus serviceBus)
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
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
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
				endProcessing(unitOfWork);
			}

			var listOfMessages = new List<OpenAndSplitChildSkills>();
			foreach (var multisiteSkillSelection in message.MultisiteSkillSelections)
			{
				foreach (var dateOnlyPeriod in message.Period.Split(20))
				{
					listOfMessages.Add(new OpenAndSplitChildSkills
					                   	{
					                   		BusinessUnitId = message.BusinessUnitId,
					                   		Datasource = message.Datasource,
					                   		JobId = message.JobId,
					                   		MultisiteSkillSelections = multisiteSkillSelection,
					                   		OwnerPersonId = message.OwnerPersonId,
					                   		Period = dateOnlyPeriod,
					                   		Timestamp = message.Timestamp
					                   	});
				}
			}

			var messageCount = listOfMessages.Count;
			var incremental = (int)Math.Floor(99d / messageCount);
            listOfMessages.ForEach(m=>m.IncreaseProgressBy = incremental);
		    listOfMessages.Last().IncreaseProgressBy = 99 - incremental*(messageCount - 1);
			listOfMessages.ForEach(m=> _serviceBus.Send(m));
			_unitOfWorkFactory = null;
		}

		private void endProcessing(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
			_feedback.Dispose();
		}
	}
}