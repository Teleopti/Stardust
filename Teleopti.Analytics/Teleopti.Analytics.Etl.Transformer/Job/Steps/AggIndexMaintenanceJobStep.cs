using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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