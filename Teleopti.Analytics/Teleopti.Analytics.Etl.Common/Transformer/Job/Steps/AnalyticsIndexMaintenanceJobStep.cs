using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class AnalyticsIndexMaintenanceJobStep : JobStepBase
	{
		public AnalyticsIndexMaintenanceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "AnalyticsIndexMaintenance";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return isLastBusinessUnit ? _jobParameters.Helper.Repository.PerformIndexMaintenance("Analytics") : 0;
		}
	}
}