using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentJobService : IImportAgentJobService
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPublisher _eventPublisher;

		public ImportAgentJobService(IJobResultRepository jobResultRepository, IEventPublisher eventPublisher)
		{
			_jobResultRepository = jobResultRepository;
			_eventPublisher = eventPublisher;
		}


		public IJobResult CreateJob(FileData fileData, ImportAgentDefaults fallbacks, IPerson owner)
		{
			
			var dateOnlyPeriod = DateOnly.Today.ToDateOnlyPeriod();
			var jobResult = new JobResult(JobCategory.WebImportAgent, dateOnlyPeriod, owner, DateTime.UtcNow);
			jobResult.AddArtifact(new JobResultArtifact(JobResultArtifactCategory.Input, fileData.FileName, fileData.Data));
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new ImportAgentEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				Defaults = fallbacks
			});
			return jobResult;
		}

	
	}

	public interface IImportAgentJobService
	{
		IJobResult CreateJob(FileData fileData, ImportAgentDefaults fallbacks, IPerson owner);
	}
}
