using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
	public class PermissionJobCollection : List<IJobStep>
	{
		public PermissionJobCollection(IJobParameters jobParameters)
		{
			Add(new StageBusinessUnitJobStep(jobParameters));
			Add(new StagePersonJobStep(jobParameters));
			Add(new StageAgentSkillJobStep(jobParameters));
			Add(new StagePermissionJobStep(jobParameters));     //
			Add(new StageScorecardJobStep(jobParameters));
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			Add(new DimSiteJobStep(jobParameters));
			Add(new DimTeamJobStep(jobParameters));
			Add(new DimSkillSetJobStep(jobParameters));
			Add(new DimPersonJobStep(jobParameters));
			Add(new PermissionReportJobStep(jobParameters));    //

			// If PM is installed then show ETL job step for synchronizing PM permissions
			if (jobParameters.IsPmInstalled)
				Add(new PmPermissionJobStep(jobParameters));
		}
	}
}
