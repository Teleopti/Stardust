using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentJobService : IImportAgentJobService
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ILoggedOnUser _loggedOnUser;

		public ImportAgentJobService(
			IJobResultRepository jobResultRepository,
			IEventPublisher eventPublisher,
			ILoggedOnUser loggedOnUser)
		{
			_jobResultRepository = jobResultRepository;
			_eventPublisher = eventPublisher;
			_loggedOnUser = loggedOnUser;
		}


		public IJobResult CreateJob(FileData fileData, ImportAgentDefaults fallbacks)
		{
			var dateOnlyPeriod = DateOnly.Today.ToDateOnlyPeriod();
			var jobResult = new JobResult(JobCategory.WebImportAgent, dateOnlyPeriod, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, fileData.FileName, fileData.Data));
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				Defaults = fallbacks
			});
			return jobResult;
		}

		public IList<ImportAgentJobResultDetail> GetJobsForLoggedOnBusinessUnit()
		{

			var loggedOnBU = _loggedOnUser.CurrentUser().WorkflowControlSet.BusinessUnit.Id;
			var resultList = _jobResultRepository.LoadAll().Where(r => r.Owner.WorkflowControlSet.BusinessUnit.Id == loggedOnBU).ToList();

			return	resultList.Select(jr =>
				{
					var detail = new ImportAgentJobResultDetail();
					detail.Timestamp = jr.Timestamp;
					detail.IsWorking = jr.IsWorking();
					detail.InputArtifact = jr.Artifacts.FirstOrDefault(ar => ar.Category == JobResultArtifactCategory.Input);
					detail.Owner = jr.Owner;
					if (jr.FinishedOk)
					{
						var resultDetail = jr.Details.FirstOrDefault();
						var summaryCount = resultDetail.GetSummaryCount();
						detail.SuccessCount = summaryCount.SuccessCount;
						detail.FaildCount = summaryCount.FaildCount;
						detail.WarningCount = summaryCount.WarningCount;
						detail.JobResult = jr;
						if (detail.WarningCount > 0)
						{
							detail.WarningArtifact = jr.Artifacts.FirstOrDefault(ar => ar.Category == JobResultArtifactCategory.OutputWarning);
						}
						if (detail.FaildCount > 0)
						{
							detail.FaildArtifact = jr.Artifacts.FirstOrDefault(ar => ar.Category == JobResultArtifactCategory.OutputError);
						}
					}
					return detail;
				}).ToList();
		}
	}

	public interface IImportAgentJobService
	{
		IJobResult CreateJob(FileData fileData, ImportAgentDefaults fallbacks);
		IList<ImportAgentJobResultDetail> GetJobsForLoggedOnBusinessUnit();
	}
}
