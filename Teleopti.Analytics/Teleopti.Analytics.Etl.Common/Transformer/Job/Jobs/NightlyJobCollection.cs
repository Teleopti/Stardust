using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class NightlyJobCollection : JobStepCollectionBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public NightlyJobCollection(IJobParameters jobParameters)
		{
			Add(new LicenseCheckJobStep(jobParameters));
			// CLEANUP
			Add(new DimPersonDeleteJobStep(jobParameters));     // BU independent
			AddWhenAllDisabled(new DimPersonTrimJobStep(jobParameters), Toggles.ETL_FixScheduleForPersonPeriod_41393);     // BU independent
			Add(new MaintenanceJobStep(jobParameters));     // BU independent

			// STAGE TABLES
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));         // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));
			AddWhenAllDisabled(new StageScheduleJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new StageScheduleDayOffCountJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			ChooseJobStepUsingToggle(new StageScheduleForecastSkillJobStepWithBpo(jobParameters), new StageScheduleForecastSkillJobStep(jobParameters), Toggles.ETL_UseBpoResources_75855);

			AddWhenAnyDisabled(new StageSchedulePreferenceJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyPreference_38283, Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			Add(new StageKpiJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageScorecardKpiJobStep(jobParameters));
			Add(new StageKpiTargetTeamJobStep(jobParameters));
			Add(new SqlServerUpdateStatistics(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			Add(new DimPersonUpdateMaxDateJobStep(jobParameters));
			Add(new DimPersonWindowsLoginJobStep(jobParameters));
			AddWhenAllDisabled(new DimShiftLengthJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			Add(new DimKpiJobStep(jobParameters));
			Add(new ScorecardKpiJobStep(jobParameters));

			// FACT TABLES
			AddWhenAllDisabled(new FactScheduleJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new FactScheduleDayCountJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			AddWhenAnyDisabled(new FactSchedulePreferenceJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyPreference_38283, Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			Add(new FactScheduleForecastSkillJobStep(jobParameters));
			Add(new FactQueueJobStep(jobParameters));                   // BU independent
			Add(new FactAgentJobStep(jobParameters));                   // BU independent
			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent
			Add(new FactAgentQueueJobStep(jobParameters));              // BU independent
			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			Add(new FactScheduleDeviationJobStep(jobParameters));
			Add(new FactKpiTargetTeamJobStep(jobParameters));
			
			// If PM is installed then show PM job steps
			if (jobParameters.IsPmInstalled)
			{
				Add(new PerformanceManagerJobStep(jobParameters));
				Add(new PmPermissionJobStep(jobParameters));
			}

			// MORE CLEAN UP!
			Add(new PurgeJobStep(jobParameters));     // BU independent
			
			Add(new CalculateBadgesJobStep(jobParameters));
		}
	}
}