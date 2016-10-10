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
			AddWhenAllDisabled(new IntradayStageScheduleJobStep(jobParameters), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new IntradayStageScheduleDayOffCountJobStep(jobParameters), Toggles.ETL_SpeedUpETL_30791);

			AddWhenAllDisabled(new IntradayStageForecastWorkloadJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayForecastWorkload_38929);


			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			AddWhenAllDisabled(new DimBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayBusinessUnit_38932);
			AddWhenAllDisabled(new DimShiftLengthJobStep(jobParameters), Toggles.ETL_SpeedUpETL_30791);

			// FACT TABLES
			AddWhenAllDisabled(new FactScheduleJobStep(jobParameters, true), Toggles.ETL_SpeedUpETL_30791);
			AddWhenAllDisabled(new FactScheduleDayCountJobStep(jobParameters, true), Toggles.ETL_SpeedUpETL_30791);
			
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
		}
	}
}