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
			AddWhenAllDisabled(new StageBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayBusinessUnit_38932);            // BU independent
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));          // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			AddWhenAllDisabled(new DimBusinessUnitJobStep(jobParameters), Toggles.ETL_SpeedUpIntradayBusinessUnit_38932);

			// FACT TABLES
			Add(new IntradayFactQueueJobStep(jobParameters));
			Add(new IntradayFactAgentJobStep(jobParameters)); // BU independent

			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent

			Add(new IntradayFactAgentQueueJobStep(jobParameters)); // BU independent

			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			Add(new FactScheduleDeviationJobStep(jobParameters, true));
		}
	}
}