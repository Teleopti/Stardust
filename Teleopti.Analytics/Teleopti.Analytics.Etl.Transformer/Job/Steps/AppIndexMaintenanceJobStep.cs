using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class AppIndexMaintenanceJobStep : JobStepBase
	{
		public AppIndexMaintenanceJobStep(IJobParameters jobParameters)
            : base(jobParameters)
		{
		    Name = "AppIndexMaintenance";
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return isLastBusinessUnit ? _jobParameters.Helper.Repository.PerformIndexMaintenance("App") : 0;
		}
	}
}