using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.Steps;

namespace Teleopti.Analytics.Etl.Transformer.Job.Jobs
{
	public class IntradayJobCollection : List<IJobStep>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IntradayJobCollection(IJobParameters jobParameters)
		{
			// STAGE TABLES
			Add(new StageDateJobStep(jobParameters));                    // BU independent
			Add(new DimDateJobStep(jobParameters));                     // BU independent
			Add(new StageTimeZoneJobStep(jobParameters));               // BU independent
			Add(new DimTimeZoneJobStep(jobParameters));                 // BU independent
			Add(new StageTimeZoneBridgeJobStep(jobParameters));         // BU independent
			Add(new StageBusinessUnitJobStep(jobParameters));            // BU independent
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));          // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));
			Add(new StagePersonJobStep(jobParameters));
			Add(new StageAgentSkillJobStep(jobParameters));
			Add(new StageStateGroupJobStep(jobParameters));
			Add(new StageActivityJobStep(jobParameters));
			Add(new StageAbsenceJobStep(jobParameters));
			Add(new StageScenarioJobStep(jobParameters));
			Add(new StageShiftCategoryJobStep(jobParameters));
			Add(new IntradayStageScheduleJobStep(jobParameters));
			//Add(new StageScheduleForecastSkillJobStep(jobParameters)); //removed 2010-02-24 to reduce duration/Load (scheduling resource calculation)
			Add(new IntradayStageScheduleDayOffCountJobStep(jobParameters));
			Add(new IntradayStageSchedulePreferenceJobStep(jobParameters));
			Add(new IntradayStageAvailabilityJobStep(jobParameters));
			Add(new StageSkillJobStep(jobParameters));
			Add(new StageWorkloadJobStep(jobParameters));
			Add(new IntradayStageForecastWorkloadJobStep(jobParameters));
			Add(new StageKpiJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageScorecardKpiJobStep(jobParameters));
			Add(new StageKpiTargetTeamJobStep(jobParameters));
			Add(new StagePermissionJobStep(jobParameters,true));
			Add(new StageUserJobStep(jobParameters));                   // BU independent
			Add(new StageGroupPagePersonJobStep(jobParameters));
			Add(new StageOvertimeJobStep(jobParameters));
			Add(new IntradayStageRequestJobStep(jobParameters));

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			Add(new BridgeTimeZoneJobStep(jobParameters));              // BU independent
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			Add(new DimSiteJobStep(jobParameters));
			Add(new DimTeamJobStep(jobParameters));
			Add(new DimSkillJobStep(jobParameters));
			Add(new DimSkillSetJobStep(jobParameters));
			Add(new DimStateGroupJobStep(jobParameters));
			Add(new DimPersonJobStep(jobParameters));
			Add(new DimActivityJobStep(jobParameters));
			Add(new DimAbsenceJobStep(jobParameters));
			Add(new DimScenarioJobStep(jobParameters));
			Add(new DimShiftCategoryJobStep(jobParameters));
			Add(new DimShiftLengthJobStep(jobParameters));
			Add(new DimWorkloadJobStep(jobParameters));
			Add(new DimKpiJobStep(jobParameters));
			Add(new DimOvertimeJobStep(jobParameters));
			Add(new ScorecardKpiJobStep(jobParameters));
			Add(new BridgeSkillSetSkillJobStep(jobParameters));
			Add(new BridgeAcdLogOnPersonJobStep(jobParameters));
			Add(new BridgeQueueWorkloadJobStep(jobParameters));
			Add(new AspNetUsersJobStep(jobParameters));                 // BU independent
			Add(new DimGroupPageJobStep(jobParameters));
			Add(new BridgeGroupPagePersonJobStep(jobParameters));

			// FACT TABLES
			Add(new FactScheduleJobStep(jobParameters,true));
			Add(new FactScheduleDayCountJobStep(jobParameters,true));
			Add(new FactSchedulePreferenceJobStep(jobParameters, true));
			Add(new FactAvailabilityJobStep(jobParameters, true));
			Add(new FactQueueJobStep(jobParameters));                   // BU independent
			Add(new FactAgentJobStep(jobParameters));                   // BU independent
			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent
			Add(new FactAgentQueueJobStep(jobParameters));              // BU independent
			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			Add(new FactAgentStateJobStep(jobParameters));
			Add(new FactForecastWorkloadJobStep(jobParameters, true));
			Add(new FactScheduleDeviationJobStep(jobParameters,true));
			Add(new FactKpiTargetTeamJobStep(jobParameters));
			Add(new FactRequestJobStep(jobParameters,true));
			Add(new FactRequestedDaysJobStep(jobParameters,true));
            Add(new FactAgentSkillJobStep(jobParameters));
			Add(new PermissionReportJobStep(jobParameters,true));

			// If PM is installed then show ETL job step for synchronizing PM permissions
			if (jobParameters.IsPmInstalled)
				Add(new PmPermissionJobStep(jobParameters));

			// WEB SERVICE TO ANALYZER
			//Add(new PerformanceManagerPermissionJobStep(jobParameters));
		}
	}

}