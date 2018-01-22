using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class DelayedJobStep : JobStepBase
	{
		public DelayedJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "Delayed Job";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			
			return _jobParameters.Helper.Repository.RunDelayedJob();
		}
	}
}