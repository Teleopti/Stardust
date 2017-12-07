using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public interface IImportExternalPerformanceInfoService
	{
		IJobResult CreateJob(ImportFileData importFileData);
		JobResultArtifact GetJobResultArtifact(Guid id, JobResultArtifactCategory category);

		IList<ImportGamificationJobResultDetail> GetJobsForCurrentBusinessUnit();
	}
	public class ImportExternalPerformanceInfoService : IImportExternalPerformanceInfoService
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ILoggedOnUser _loggedOnUser;

		public ImportExternalPerformanceInfoService(IJobResultRepository jobResultRepository, IEventPublisher eventPublisher, ILoggedOnUser loggedOnUser)
		{
			_jobResultRepository = jobResultRepository;
			_eventPublisher = eventPublisher;
			_loggedOnUser = loggedOnUser;
		}

		public IJobResult CreateJob(ImportFileData importFileData)
		{
			var dateOnlyPeriod = DateOnly.Today.ToDateOnlyPeriod();
			var jobResult = new JobResult(JobCategory.WebImportExternalGamification, dateOnlyPeriod, _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, importFileData.FileName, importFileData.Data));
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new ImportExternalPerformanceInfoEvent()
			{
				JobResultId = jobResult.Id.GetValueOrDefault()
			});
			return jobResult;
		}


		public IList<ImportGamificationJobResultDetail> GetJobsForCurrentBusinessUnit()
		{
			var resultList = _jobResultRepository.LoadAllWithNoLock()
				.Where(r => r.JobCategory == JobCategory.WebImportExternalGamification)
				.OrderByDescending(r => r.Timestamp)
				.ToList();
			return resultList.Select(jr => new ImportGamificationJobResultDetail() {
				Id = jr.Id.Value,
				Owner = jr.Owner.Name.ToString(),
				CreateDateTime = jr.Artifacts.First().CreateTime,
				Name = jr.Artifacts.First().FileName,
				Status = GetJobStatus(jr),
				Category = jr.JobCategory

			}).ToList();
		}

		private string GetJobStatus(IJobResult job)
		{
			if (!job.Details.Any())
			{
				return "errored";
			}

			if (job.FinishedOk)
			{
				return "finished";
			}

			if (job.HasError() || !string.IsNullOrEmpty( job.Details.First().ExceptionMessage)|| !string.IsNullOrEmpty(job.Details.First().InnerExceptionMessage))
			{
				return "errord";
			}

			if (job.IsWorking())
			{
				return "inprogress";
			}

			return "errord";
		}


		public JobResultArtifact GetJobResultArtifact(Guid id, JobResultArtifactCategory category)
		{
			var jobResult = _jobResultRepository.Get(id);
			return jobResult?.Artifacts.FirstOrDefault(ar => ar.Category == category);
		}
	}
}
