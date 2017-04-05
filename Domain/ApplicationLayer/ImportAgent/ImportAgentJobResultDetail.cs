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
		public ImportAgentJobResultDetail(IJobResult result)
		{
			this.ResultDetail = result.Details.FirstOrDefault();
			this.JobResult = result;
			SetValues();
		}

		public IJobResultDetail ResultDetail { get; }
		public int SuccessCount { get; private set; }
		public int FaildCount { get; private set; }
		public int WarningCount { get; private set; }

		public JobResultArtifact InputArtifact { get; private set; }
		public JobResultArtifact FaildArtifact { get; private set; }
		public JobResultArtifact WarningArtifact { get; private set; }
		public IJobResult JobResult { get; }

		private void SetValues()
		{
			if (ResultDetail != null)
			{

				this.InputArtifact = this.JobResult.Artifacts.FirstOrDefault(ar => ar.Category == JobResultArtifactCategory.Input);
				if (JobResult.FinishedOk)
				{
					var summaryCount = ResultDetail.GetSummaryCount();
					if (summaryCount != null)
					{
						this.SuccessCount = summaryCount.SuccessCount;
						this.FaildCount = summaryCount.FaildCount;
						this.WarningCount = summaryCount.WarningCount;
						if (this.WarningCount > 0)
						{
							this.WarningArtifact = JobResult.Artifacts.FirstOrDefault(ar => ar.Category == JobResultArtifactCategory.OutputWarning);
						}
						if (this.FaildCount > 0)
						{
							this.FaildArtifact = JobResult.Artifacts.FirstOrDefault(ar => ar.Category == JobResultArtifactCategory.OutputError);
						}

					}
				}
			}
		}
	}
}
