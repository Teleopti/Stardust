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
			AddWhenAllDisabled(new StageDateJobStep(jobParameters), Toggles.ETL_RemoveTimeZoneAndDateNightly_40870);                    // BU independent
			AddWhenAllDisabled(new DimDateJobStep(jobParameters), Toggles.ETL_RemoveTimeZoneAndDateNightly_40870);                     // BU independent
			AddWhenAllDisabled(new StageTimeZoneJobStep(jobParameters), Toggles.ETL_RemoveTimeZoneAndDateNightly_40870);               // BU independent
			AddWhenAllDisabled(new DimTimeZoneJobStep(jobParameters), Toggles.ETL_RemoveTimeZoneAndDateNightly_40870);                 // BU independent
			AddWhenAllDisabled(new StageTimeZoneBridgeJobStep(jobParameters), Toggles.ETL_RemoveTimeZoneAndDateNightly_40870);         // BU independent
			AddWhenAllDisabled(new StageBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyBusinessUnit_38932);            // BU independent
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));         // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));

			AddWhenAllDisabled(new StagePersonJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllDisabled(new StageAgentSkillJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllDisabled(new StageActivityJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyActivity_38303);
			AddWhenAllDisabled(new StageAbsenceJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyAbsence_38301);
			AddWhenAllDisabled(new StageScenarioJobStep(jobParameters), Toggles.ETL_SpeedUpScenarioNightly_38300);
			AddWhenAllDisabled(new StageShiftCategoryJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyShiftCategory_38718);

			AddWhenAllDisabled(new StageScheduleJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new StageScheduleDayOffCountJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			Add(new StageScheduleForecastSkillJobStep(jobParameters));
			AddWhenAnyDisabled(new StageSchedulePreferenceJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyPreference_38283, Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new StageAvailabilityJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyAvailability_38926);
			AddWhenAllDisabled(new StageSkillJobStep(jobParameters), Toggles.ETL_SpeedUpNightlySkill_37543);
			AddWhenAllDisabled(new StageWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyWorkload_38928);

			AddWhenAllDisabled(new StageForecastWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyForecastWorkload_38929);
			Add(new StageKpiJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageScorecardKpiJobStep(jobParameters));
			Add(new StageKpiTargetTeamJobStep(jobParameters));

			AddWhenAllDisabled(new StageGroupPagePersonJobStep(jobParameters), Toggles.ETL_SpeedUpGroupPagePersonNightly_37623);
			AddWhenAllDisabled(new StageOvertimeJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyOvertime_38304);
			AddWhenAllDisabled(new StageRequestJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyRequest_38914);

			Add(new SqlServerUpdateStatistics(jobParameters));

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			AddWhenAllDisabled(new BridgeTimeZoneJobStep(jobParameters), Toggles.ETL_RemoveTimeZoneAndDateNightly_40870);              // BU independent
			AddWhenAllDisabled(new DimBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyBusinessUnit_38932);

			AddWhenAllDisabled(new DimDayOffJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyDayOff_38213);

			Add(new DimScorecardJobStep(jobParameters));

			AddWhenAllDisabled(new DimSiteJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllDisabled(new DimTeamJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);

			AddWhenAllDisabled(new DimSkillJobStep(jobParameters), Toggles.ETL_SpeedUpNightlySkill_37543);

			AddWhenAllDisabled(new DimSkillSetJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllDisabled(new DimPersonJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllEnabled(new DimPersonUpdateMaxDateJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllEnabled(new DimPersonWindowsLoginJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);

			AddWhenAllDisabled(new DimActivityJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyActivity_38303);
			AddWhenAllDisabled(new DimAbsenceJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyAbsence_38301);
			AddWhenAllDisabled(new DimScenarioJobStep(jobParameters), Toggles.ETL_SpeedUpScenarioNightly_38300);
			AddWhenAllDisabled(new DimShiftCategoryJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyShiftCategory_38718);
			AddWhenAllDisabled(new DimShiftLengthJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new DimWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyWorkload_38928);

			Add(new DimKpiJobStep(jobParameters));
			Add(new ScorecardKpiJobStep(jobParameters));

			AddWhenAllDisabled(new BridgeSkillSetSkillJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllDisabled(new BridgeAcdLogOnPersonJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			AddWhenAllDisabled(new BridgeQueueWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyWorkload_38928);
			
			AddWhenAllDisabled(new DimGroupPageJobStep(jobParameters), Toggles.ETL_SpeedUpGroupPagePersonNightly_37623);
			AddWhenAllDisabled(new BridgeGroupPagePersonJobStep(jobParameters), Toggles.ETL_SpeedUpGroupPagePersonNightly_37623);

			AddWhenAllDisabled(new DimOvertimeJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyOvertime_38304);

			// FACT TABLES
			AddWhenAllDisabled(new FactScheduleJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new FactScheduleDayCountJobStep(jobParameters), Toggles.ETL_SpeedUpFactScheduleNightly_38019);

			AddWhenAnyDisabled(new FactSchedulePreferenceJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyPreference_38283, Toggles.ETL_SpeedUpFactScheduleNightly_38019);
			AddWhenAllDisabled(new FactAvailabilityJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyAvailability_38926);

			Add(new FactScheduleForecastSkillJobStep(jobParameters));
			Add(new FactQueueJobStep(jobParameters));                   // BU independent
			Add(new FactAgentJobStep(jobParameters));                   // BU independent
			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent
			Add(new FactAgentQueueJobStep(jobParameters));              // BU independent
			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			AddWhenAllDisabled(new FactForecastWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyForecastWorkload_38929);
			Add(new FactScheduleDeviationJobStep(jobParameters));
			Add(new FactKpiTargetTeamJobStep(jobParameters));

			AddWhenAllDisabled(new FactRequestJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyRequest_38914);
			AddWhenAllDisabled(new FactRequestedDaysJobStep(jobParameters), Toggles.ETL_SpeedUpNightlyRequest_38914);
			AddWhenAllDisabled(new FactAgentSkillJobStep(jobParameters), Toggles.ETL_SpeedUpPersonPeriodNightly_38097);
			// If PM is installed then show PM job steps
			if (jobParameters.IsPmInstalled)
			{
				Add(new PerformanceManagerJobStep(jobParameters));
				Add(new PmPermissionJobStep(jobParameters));
			}

			// MORE CLEAN UP!
			Add(new PurgeJobStep(jobParameters));     // BU independent
			
			AddWhenAllEnabled(new CalculateBadgesJobStep(jobParameters), Toggles.ETL_MoveBadgeCalculationToETL_38421);
		}
	}
}