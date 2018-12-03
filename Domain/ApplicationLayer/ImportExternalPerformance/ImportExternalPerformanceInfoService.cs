using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

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
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly ILoggedOnUser _loggedOnUser;

		public ImportExternalPerformanceInfoService(IJobResultRepository jobResultRepository, IEventPopulatingPublisher eventPublisher, ILoggedOnUser loggedOnUser)
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
			return resultList.Select(jobResult =>
			{
				var hasError = jobResult.HasError();
				var hasInvalidRecords = this.hasInvalidRecords(jobResult);
				var jobResultDetail = jobResult.Details.FirstOrDefault();
				var hasException = !(jobResultDetail?.ExceptionMessage.IsNullOrEmpty() ?? true)
								   || !(jobResultDetail?.InnerExceptionMessage.IsNullOrEmpty() ?? true);
				return new ImportGamificationJobResultDetail()
				{
					Id = jobResult.Id.Value,
					Owner = jobResult.Owner.Name.ToString(),
					CreateDateTime = jobResult.Artifacts.First().CreateTime,
					Name = jobResult.Artifacts.First().FileName,
					Status = getJobStatus(jobResult),
					Category = jobResult.JobCategory,
					HasError = hasError,
					HasInvalidRecords = hasInvalidRecords,
					ErrorMessage =
						hasError ? (hasException ? Resources.InternalErrorMsg : jobResultDetail?.Message) : string.Empty
				};
			}).ToList();
		}

		private bool hasInvalidRecords(IJobResult job)
		{
			var result = false;

			if (job.Artifacts != null)
			{
				result = job.Artifacts.Any(a => a.Category == JobResultArtifactCategory.OutputError);
			}

			return result;
		}

		private string getJobStatus(IJobResult job)
		{
			GamificationJobStatus status = GamificationJobStatus.InProgress;
			if (job.HasError())
			{
				status = GamificationJobStatus.Failed;
			}
			else
			if (job.FinishedOk)
			{
				status = GamificationJobStatus.Finished;
			}

			return status.ToString().ToLower();
		}


		public JobResultArtifact GetJobResultArtifact(Guid id, JobResultArtifactCategory category)
		{
			var jobResult = _jobResultRepository.Get(id);
			return jobResult?.Artifacts.FirstOrDefault(ar => ar.Category == category);
		}
	}
}
