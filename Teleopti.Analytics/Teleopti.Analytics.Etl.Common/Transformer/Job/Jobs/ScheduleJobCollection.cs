using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class ScheduleJobCollection : JobStepCollectionBase
	{
		public ScheduleJobCollection(IJobParameters jobParameters)
		{
			Add(new StageBusinessUnitJobStep(jobParameters));
			Add(new StageDateJobStep(jobParameters));
			Add(new StagePersonJobStep(jobParameters));
			Add(new StageAgentSkillJobStep(jobParameters));
			Add(new StageActivityJobStep(jobParameters));
			Add(new StageAbsenceJobStep(jobParameters));
			Add(new StageScenarioJobStep(jobParameters));
			Add(new StageShiftCategoryJobStep(jobParameters));
			Add(new StageScheduleJobStep(jobParameters));
			//Add(new StageScheduleForecastSkillJobStep(jobParameters));
			
			Add(new StageScheduleForecastSkillJobStepWithBpo(jobParameters));
			Add(new StageScheduleDayOffCountJobStep(jobParameters));
			Add(new StageSchedulePreferenceJobStep(jobParameters));
			Add(new StageAvailabilityJobStep(jobParameters));
			Add(new StageSkillJobStep(jobParameters));
			Add(new StageWorkloadJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageGroupPagePersonJobStep(jobParameters));
			Add(new StageOvertimeJobStep(jobParameters));
			Add(new StageRequestJobStep(jobParameters));
			Add(new SqlServerUpdateStatistics(jobParameters));
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimDayOffJobStep(jobParameters));
			Add(new DimDateJobStep(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			Add(new DimSiteJobStep(jobParameters));
			Add(new DimTeamJobStep(jobParameters));
			Add(new DimSkillJobStep(jobParameters));
			Add(new DimSkillSetJobStep(jobParameters));
			Add(new DimPersonJobStep(jobParameters));
			Add(new DimActivityJobStep(jobParameters));
			Add(new DimAbsenceJobStep(jobParameters));
			Add(new DimScenarioJobStep(jobParameters));
			Add(new DimShiftCategoryJobStep(jobParameters));
			Add(new DimShiftLengthJobStep(jobParameters));
			Add(new DimGroupPageJobStep(jobParameters));
			Add(new DimOvertimeJobStep(jobParameters));
			Add(new BridgeGroupPagePersonJobStep(jobParameters));
			Add(new FactScheduleJobStep(jobParameters));
			Add(new FactScheduleDayCountJobStep(jobParameters));
			Add(new FactSchedulePreferenceJobStep(jobParameters));
			Add(new FactAvailabilityJobStep(jobParameters));
			Add(new DimWorkloadJobStep(jobParameters));
			Add(new FactScheduleForecastSkillJobStep(jobParameters));
			Add(new FactRequestJobStep(jobParameters));
			Add(new FactRequestedDaysJobStep(jobParameters));
		}
	}
}