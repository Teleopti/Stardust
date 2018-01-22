using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class UpgradeMaintenanceJobCollection : List<IJobStep>
	{
		public UpgradeMaintenanceJobCollection(IJobParameters jobParameters)
		{
			Add(new DelayedJobStep(jobParameters));
		}
	}
}
