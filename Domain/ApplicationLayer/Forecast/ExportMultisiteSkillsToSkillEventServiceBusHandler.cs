using System.Collections.Generic;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
#pragma warning disable 618
	[UseNotOnToggle(Toggles.Wfm_MoveExportMultiSiteSkillOnStardust_38134)]
	public class ExportMultisiteSkillsToSkillEventServiceBusHandler : ExportMultisiteSkillsToSkillBase, IHandleEvent<ExportMultisiteSkillsToSkillEvent>, IRunOnServiceBus
#pragma warning restore 618
	{
		public ExportMultisiteSkillsToSkillEventServiceBusHandler(ICurrentUnitOfWork unitOfWork,
			IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker,
			IExportMultisiteSkillProcessor exportMultisiteSkillProcessor)
			: base(unitOfWork, jobResultRepository, feedback, messageBroker, exportMultisiteSkillProcessor)
		{
			
		}

		public virtual new void Handle(ExportMultisiteSkillsToSkillEvent message)
		{
			base.Handle(message);
		}
	}

	[UseOnToggle(Toggles.Wfm_MoveExportMultiSiteSkillOnStardust_38134)]
	public class ExportMultisiteSkillsToSkillEventStardustHandler : ExportMultisiteSkillsToSkillBase, IHandleEvent<ExportMultisiteSkillsToSkillEvent>, IRunOnStardust
	{
		public ExportMultisiteSkillsToSkillEventStardustHandler(ICurrentUnitOfWork unitOfWork, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker, IExportMultisiteSkillProcessor exportMultisiteSkillProcessor) : base(unitOfWork, jobResultRepository, feedback, messageBroker, exportMultisiteSkillProcessor)
		{
		}
		
		[AsSystem]
		[UnitOfWork]
		public virtual new void Handle(ExportMultisiteSkillsToSkillEvent message)
		{
			base.Handle(message);
		}
	}

	public class ExportMultisiteSkillsToSkillBase
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IRepository<IJobResult> _jobResultRepository;
		private readonly IJobResultFeedback _feedback;
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IExportMultisiteSkillProcessor _exportMultisiteSkillProcessor;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ExportMultisiteSkillsToSkillBase));

		public ExportMultisiteSkillsToSkillBase(ICurrentUnitOfWork unitOfWork, IJobResultRepository jobResultRepository, IJobResultFeedback feedback, IMessageBrokerComposite messageBroker, IExportMultisiteSkillProcessor exportMultisiteSkillProcessor)
		{
			_unitOfWork = unitOfWork;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_messageBroker = messageBroker;
			_exportMultisiteSkillProcessor = exportMultisiteSkillProcessor;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ExportMultisiteSkillsToSkillEvent message)
		{
			var unitOfWork = _unitOfWork.Current();
			{
				var jobResult = _jobResultRepository.Get(message.JobId);

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
				var period = new DateOnlyPeriod(new DateOnly(message.PeriodStart), new DateOnly(message.PeriodEnd));
				foreach (var dateOnlyPeriod in period.Split(20))
				{
					listOfMessages.Add(new ExportMultisiteSkillToSkill
					{
						LogOnBusinessUnitId = message.LogOnBusinessUnitId,
						LogOnDatasource = message.LogOnDatasource,
						JobId = message.JobId,
						MultisiteSkillSelections = multisiteSkillSelection,
						OwnerPersonId = message.OwnerPersonId,
						PeriodStart = dateOnlyPeriod.StartDate.Date,
						PeriodEnd = dateOnlyPeriod.EndDate.Date,
						Timestamp = message.Timestamp
					});
					progressSteps++;
				}
				progressSteps += multisiteSkillSelection.ChildSkillSelections.Count * period.DayCount() * 4;
			}
			_feedback.ChangeTotalProgress(progressSteps);

			listOfMessages.ForEach(m => _exportMultisiteSkillProcessor.Process(m));
			_feedback.Clear();
		}
	}
}