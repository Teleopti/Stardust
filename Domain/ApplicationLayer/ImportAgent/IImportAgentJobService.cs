using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public interface IImportAgentJobService
	{
		IJobResult CreateJob(FileData fileData, ImportAgentDefaults fallbacks);
		IList<ImportAgentJobResultDetail> GetJobsForCurrentBusinessUnit();
		JobResultArtifact GetJobResultArtifact(Guid id, JobResultArtifactCategory category);
	}
}