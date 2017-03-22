using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.POIFS.NIO;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.ImportAgent
{
	public class ImportAgentService : IImportAgentService
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPublisher _eventPublisher;

		public ImportAgentService(IJobResultRepository jobResultRepository, IEventPublisher eventPublisher)
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

	public interface IImportAgentService
	{
		IJobResult CreateJob(FileData fileData, ImportAgentDefaults fallbacks, IPerson owner);
	}
}
