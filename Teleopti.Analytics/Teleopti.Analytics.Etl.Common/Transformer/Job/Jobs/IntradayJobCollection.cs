using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class IntradayJobCollection : JobStepCollectionBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IntradayJobCollection(IJobParameters jobParameters)
		{
			// STAGE TABLES
			AddWhenAllDisabled(new StageDateJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayDate_38934);                    // BU independent
			AddWhenAllDisabled(new DimDateJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayDate_38934);                     // BU independent
			AddWhenAllDisabled(new StageBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayBusinessUnit_38932);            // BU independent
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));          // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));
			AddWhenAllDisabled(new StageActivityJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayActivity_38303);
			AddWhenAllDisabled(new StageAbsenceJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayAbsence_38301);
			AddWhenAllDisabled(new StageScenarioJobStep(jobParameters), Toggles.ETL_SpeedUpScenario_38300);
			AddWhenAllDisabled(new StageShiftCategoryJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayShiftCategory_38718);
			AddWhenAllDisabled(new IntradayStageScheduleJobStep(jobParameters), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new IntradayStageScheduleDayOffCountJobStep(jobParameters), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new IntradayStageAvailabilityJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayAvailability_38926);
			AddWhenAllDisabled(new StageSkillJobStep(jobParameters), Toggles.ETL_SpeedUpIntradaySkill_37543);
			AddWhenAllDisabled(new StageWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayWorkload_38928);

			AddWhenAllDisabled(new IntradayStageForecastWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayForecastWorkload_38929);
			AddWhenAllDisabled(new StageKpiJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new StageScorecardJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new StageScorecardKpiJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new StageKpiTargetTeamJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new StageOvertimeJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayOvertime_38304);
			AddWhenAllDisabled(new IntradayStageRequestJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayRequest_38914);

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			AddWhenAllDisabled(new DimBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayBusinessUnit_38932);
			AddWhenAllDisabled(new DimScorecardJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new DimKpiJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new ScorecardKpiJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new DimSkillJobStep(jobParameters), Toggles.ETL_SpeedUpIntradaySkill_37543);
			AddWhenAllDisabled(new DimActivityJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayActivity_38303);
			AddWhenAllDisabled(new DimAbsenceJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayAbsence_38301);
			AddWhenAllDisabled(new DimScenarioJobStep(jobParameters), Toggles.ETL_SpeedUpScenario_38300);
			AddWhenAllDisabled(new DimShiftCategoryJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayShiftCategory_38718);
			AddWhenAllDisabled(new DimShiftLengthJobStep(jobParameters), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new DimWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayWorkload_38928);

			AddWhenAllDisabled(new DimOvertimeJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayOvertime_38304);

			AddWhenAllDisabled(new BridgeQueueWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayWorkload_38928);

			// FACT TABLES
			AddWhenAllDisabled(new FactScheduleJobStep(jobParameters, true), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new FactScheduleDayCountJobStep(jobParameters, true), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new FactAvailabilityJobStep(jobParameters, true), Toggles.ETL_SpeedUpIntradayAvailability_38926);
			
			AddWhenAllEnabled(new IntradayFactQueueJobStep(jobParameters), Toggles.ETL_OnlyLatestQueueAgentStatistics_30787);
			AddWhenAllEnabled(new IntradayFactAgentJobStep(jobParameters), Toggles.ETL_OnlyLatestQueueAgentStatistics_30787); // BU independent
			AddWhenAllDisabled(new FactQueueJobStep(jobParameters), Toggles.ETL_OnlyLatestQueueAgentStatistics_30787); // BU independent
			AddWhenAllDisabled(new FactAgentJobStep(jobParameters), Toggles.ETL_OnlyLatestQueueAgentStatistics_30787); // BU independent

			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent

			AddWhenAllEnabled(new IntradayFactAgentQueueJobStep(jobParameters), Toggles.ETL_OnlyLatestQueueAgentStatistics_30787); // BU independent
			AddWhenAllDisabled(new FactAgentQueueJobStep(jobParameters), Toggles.ETL_OnlyLatestQueueAgentStatistics_30787); // BU independent

			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			AddWhenAllDisabled(new FactForecastWorkloadJobStep(jobParameters, true), Toggles.ETL_SpeedUpIntradayForecastWorkload_38929);
			Add(new FactScheduleDeviationJobStep(jobParameters, true));
			AddWhenAllDisabled(new FactKpiTargetTeamJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayScorecard_38933);
			AddWhenAllDisabled(new FactRequestJobStep(jobParameters, true), Toggles.ETL_SpeedUpIntradayRequest_38914);
			AddWhenAllDisabled(new FactRequestedDaysJobStep(jobParameters, true), Toggles.ETL_SpeedUpIntradayRequest_38914);
		}
	}
}