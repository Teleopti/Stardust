using System;
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
	}
}
