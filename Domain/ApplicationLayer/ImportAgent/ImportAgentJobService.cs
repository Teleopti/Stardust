using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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


		public IJobResult CreateJob(ImportFileData importFileData, ImportAgentDefaults fallbacks)
		{
			var dateOnlyPeriod = DateOnly.Today.ToDateOnlyPeriod();
			var jobResult = new JobResult(JobCategory.WebImportAgent, dateOnlyPeriod, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, importFileData.FileName, importFileData.Data));
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				Defaults = fallbacks
			});
			return jobResult;
		}

		public JobResultArtifact GetJobResultArtifact(Guid id, JobResultArtifactCategory category)
		{
			var jobResult = _jobResultRepository.Get(id);
			return jobResult?.Artifacts.FirstOrDefault(ar => ar.Category == category);
		}

		public IList<ImportAgentJobResultDetail> GetJobsForCurrentBusinessUnit()
		{
			var resultList = _jobResultRepository.LoadAllWithNoLock()
				.Where(r=>r.JobCategory == JobCategory.WebImportAgent)
				.OrderByDescending(r => r.Timestamp)
				.ToList();
			return resultList.Select(jr => new ImportAgentJobResultDetail(jr)).ToList();
		}
	}
}
