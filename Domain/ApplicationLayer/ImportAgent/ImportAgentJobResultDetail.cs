using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentJobResultDetail : IImportAgentResultCount
	{

		public DateTime Timestamp { get; set; }
		public int SuccessCount { get; set; }
		public int FaildCount { get; set; }
		public int WarningCount { get; set; }

		public bool IsWorking { get; set; }

		public JobResultArtifact InputArtifact { get; set; }
		public JobResultArtifact FaildArtifact { get; set; }
		public JobResultArtifact WarningArtifact { get; set; }
		public IPerson Owner { get; set; }
		public IJobResult JobResult { get; set; }
	}
}
