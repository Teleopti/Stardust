using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class NightlyJobCollection : List<IJobStep>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public NightlyJobCollection(IJobParameters jobParameters)
		{
			Add(new LicenseCheckJobStep(jobParameters));
			// CLEANUP
			Add(new DimPersonDeleteJobStep(jobParameters));     // BU independent
			Add(new DimPersonTrimJobStep(jobParameters));     // BU independent
			Add(new MaintenanceJobStep(jobParameters));     // BU independent

			// STAGE TABLES
			Add(new StageDateJobStep(jobParameters));                    // BU independent
			Add(new DimDateJobStep(jobParameters));                     // BU independent
			Add(new StageTimeZoneJobStep(jobParameters));               // BU independent
			Add(new DimTimeZoneJobStep(jobParameters));                 // BU independent
			Add(new StageTimeZoneBridgeJobStep(jobParameters));         // BU independent
			Add(new StageBusinessUnitJobStep(jobParameters));            // BU independent
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));         // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodNightly_38097))
			{
				Add(new StagePersonJobStep(jobParameters));
				Add(new StageAgentSkillJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyActivity_38303))
			{
				Add(new StageActivityJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyAbsence_38301))
			{
				Add(new StageAbsenceJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpScenario_38300))
			{
				Add(new StageScenarioJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyShiftCategory_38718))
			{
				Add(new StageShiftCategoryJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpFactScheduleNightly_38019))
			{
				Add(new StageScheduleJobStep(jobParameters));
				Add(new StageScheduleDayOffCountJobStep(jobParameters));
			}
			if (jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayDayOff_38213))
			{
				Add(new DimDayOffJobStep(jobParameters));
			}
			Add(new StageScheduleForecastSkillJobStep(jobParameters));
			Add(new StageSchedulePreferenceJobStep(jobParameters));
			Add(new StageAvailabilityJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlySkill_37543))
			{
				Add(new StageSkillJobStep(jobParameters));
			}
			Add(new StageWorkloadJobStep(jobParameters));
			Add(new StageForecastWorkloadJobStep(jobParameters));
			Add(new StageKpiJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageScorecardKpiJobStep(jobParameters));
			Add(new StageKpiTargetTeamJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPermissionReport_33584))
			{
				Add(new StagePermissionJobStep(jobParameters));
			}
			Add(new StageGroupPagePersonJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyOvertime_38304))
			{
				Add(new StageOvertimeJobStep(jobParameters));
			}
			Add(new StageRequestJobStep(jobParameters));
			Add(new SqlServerUpdateStatistics(jobParameters));

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			Add(new BridgeTimeZoneJobStep(jobParameters));              // BU independent
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodNightly_38097))
			{
				Add(new DimSiteJobStep(jobParameters));
				Add(new DimTeamJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlySkill_37543))
			{
				Add(new DimSkillJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodNightly_38097))
			{
				Add(new DimSkillSetJobStep(jobParameters));
				Add(new DimPersonJobStep(jobParameters));
			}
			if (jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodNightly_38097))
			{
				Add(new DimPersonWindowsLoginJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyActivity_38303))
			{
				Add(new DimActivityJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyAbsence_38301))
			{
				Add(new DimAbsenceJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpScenario_38300))
			{
				Add(new DimScenarioJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyShiftCategory_38718))
			{
				Add(new DimShiftCategoryJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpFactScheduleNightly_38019))
			{
				Add(new DimShiftLengthJobStep(jobParameters));
			}
			Add(new DimWorkloadJobStep(jobParameters));
			Add(new DimKpiJobStep(jobParameters));
			Add(new ScorecardKpiJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodNightly_38097))
			{
				Add(new BridgeSkillSetSkillJobStep(jobParameters));
				Add(new BridgeAcdLogOnPersonJobStep(jobParameters));
			}
			Add(new BridgeQueueWorkloadJobStep(jobParameters));
			Add(new DimGroupPageJobStep(jobParameters));
			Add(new BridgeGroupPagePersonJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpNightlyOvertime_38304))
			{
				Add(new DimOvertimeJobStep(jobParameters));
			}

			// FACT TABLES
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpFactScheduleNightly_38019))
			{
				Add(new FactScheduleJobStep(jobParameters));
				Add(new FactScheduleDayCountJobStep(jobParameters));
			}
			Add(new FactSchedulePreferenceJobStep(jobParameters));
			Add(new FactAvailabilityJobStep(jobParameters));
			Add(new FactScheduleForecastSkillJobStep(jobParameters));
			Add(new FactQueueJobStep(jobParameters));                   // BU independent
			Add(new FactAgentJobStep(jobParameters));                   // BU independent
			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent
			Add(new FactAgentQueueJobStep(jobParameters));              // BU independent
			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			Add(new FactForecastWorkloadJobStep(jobParameters));
			Add(new FactScheduleDeviationJobStep(jobParameters));
			Add(new FactKpiTargetTeamJobStep(jobParameters));
			Add(new FactRequestJobStep(jobParameters));
			Add(new FactRequestedDaysJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodNightly_38097))
			{
				Add(new FactAgentSkillJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPermissionReport_33584))
			{
				Add(new PermissionReportJobStep(jobParameters));
			}
			// If PM is installed then show PM job steps
			if (jobParameters.IsPmInstalled)
			{
				Add(new PerformanceManagerJobStep(jobParameters));
				Add(new PmPermissionJobStep(jobParameters));
			}

			// MORE CLEAN UP!
			Add(new PurgeJobStep(jobParameters));     // BU independent
			if (jobParameters.RunIndexMaintenance)
			{
				Add(new AnalyticsIndexMaintenanceJobStep(jobParameters)); // BU independent
				Add(new AppIndexMaintenanceJobStep(jobParameters)); // BU independent
				Add(new AggIndexMaintenanceJobStep(jobParameters)); // BU independent
			}
			if (jobParameters.ToggleManager.IsEnabled(Toggles.ETL_MoveBadgeCalculationToETL_38421))
			{
				Add(new CalculateBadgesJobStep(jobParameters));
			}
		}
	}
}