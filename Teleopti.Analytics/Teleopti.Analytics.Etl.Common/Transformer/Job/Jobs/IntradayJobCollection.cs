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
			Add(new DimQueueJobStep(jobParameters));                    // BU independent
			Add(new DimAcdLogOnJobStep(jobParameters));                 // BU independent
			Add(new DimQualityQuestLoadJobStep(jobParameters));          // BU independent
			Add(new RaptorQueueSynchronizationStep(jobParameters));
			Add(new RaptorAgentLogOnSynchronizationStep(jobParameters));

			// FACT TABLES
			Add(new IntradayFactQueueJobStep(jobParameters));
			Add(new IntradayFactAgentJobStep(jobParameters)); // BU independent

			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent

			Add(new IntradayFactAgentQueueJobStep(jobParameters)); // BU independent

			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			if(jobParameters.ToggleManager.IsEnabled(Toggles.ETL_Intraday_SpeedUp_Fact_Schedule_Deviation_Calculation_79646))
				Add(new FactScheduleDeviationJobStepStory79646(jobParameters, true));
			else
				Add(new FactScheduleDeviationJobStep(jobParameters, true));
		}
	}
}