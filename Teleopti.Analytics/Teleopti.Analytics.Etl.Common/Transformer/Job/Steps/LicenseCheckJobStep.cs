using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class LicenseCheckJobStep : JobStepBase
	{
		public LicenseCheckJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			// another more discreate name??????
			Name = "PrimaryCheck";
			JobCategory = JobCategoryType.Initial;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return JobParameters.Helper.Repository.LicenseStatusUpdater.RunCheck();
		}
	}
}