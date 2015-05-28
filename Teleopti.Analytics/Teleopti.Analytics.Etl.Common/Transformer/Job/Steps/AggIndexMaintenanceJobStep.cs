using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class AggIndexMaintenanceJobStep : JobStepBase
	{
		public AggIndexMaintenanceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "AggIndexMaintenance";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return isLastBusinessUnit ? _jobParameters.Helper.Repository.PerformIndexMaintenance("Agg") : 0;
		}
	}
}