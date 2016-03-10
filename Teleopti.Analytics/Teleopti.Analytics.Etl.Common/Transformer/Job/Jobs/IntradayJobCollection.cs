using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class IntradayJobCollection : List<IJobStep>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IntradayJobCollection(IJobParameters jobParameters)
		{
			// STAGE TABLES
			Add(new StageDateJobStep(jobParameters));                    // BU independent
			Add(new DimDateJobStep(jobParameters));                     // BU independent
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
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791))
			{
				Add(new IntradayStageScheduleJobStep(jobParameters));
				Add(new IntradayStageScheduleDayOffCountJobStep(jobParameters));
			}
			Add(new IntradayStageSchedulePreferenceJobStep(jobParameters));
			Add(new IntradayStageAvailabilityJobStep(jobParameters));
			Add(new StageSkillJobStep(jobParameters));
			Add(new StageWorkloadJobStep(jobParameters));
			Add(new IntradayStageForecastWorkloadJobStep(jobParameters));
			Add(new StageKpiJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageScorecardKpiJobStep(jobParameters));
			Add(new StageKpiTargetTeamJobStep(jobParameters));
			Add(new StageGroupPagePersonJobStep(jobParameters));
			Add(new StageOvertimeJobStep(jobParameters));
			Add(new IntradayStageRequestJobStep(jobParameters));

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162))
			{
				Add(new DimSiteJobStep(jobParameters));
				Add(new DimTeamJobStep(jobParameters));
			}
			Add(new DimSkillJobStep(jobParameters));
		    if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162))
		    {
		        Add(new DimSkillSetJobStep(jobParameters));
		    }
		    Add(new DimStateGroupJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162))
			{
				Add(new DimPersonJobStep(jobParameters));
			}
			Add(new DimActivityJobStep(jobParameters));
			Add(new DimAbsenceJobStep(jobParameters));
			Add(new DimScenarioJobStep(jobParameters));
			Add(new DimShiftCategoryJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791))
			{
				Add(new DimShiftLengthJobStep(jobParameters));
			}
			Add(new DimWorkloadJobStep(jobParameters));
			Add(new DimKpiJobStep(jobParameters));
			Add(new DimOvertimeJobStep(jobParameters));
			Add(new ScorecardKpiJobStep(jobParameters));
		    if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162))
		    {
		        Add(new BridgeSkillSetSkillJobStep(jobParameters));
		    }
		    Add(new BridgeAcdLogOnPersonJobStep(jobParameters));
			Add(new BridgeQueueWorkloadJobStep(jobParameters));
			Add(new DimGroupPageJobStep(jobParameters));
			Add(new BridgeGroupPagePersonJobStep(jobParameters));

			// FACT TABLES
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791))
			{
				Add(new FactScheduleJobStep(jobParameters, true));
				Add(new FactScheduleDayCountJobStep(jobParameters, true));
			}
			Add(new FactSchedulePreferenceJobStep(jobParameters, true));
			Add(new FactAvailabilityJobStep(jobParameters, true));
			var agentQueueIntradayEnabled = jobParameters.ToggleManager.IsEnabled(Toggles.ETL_OnlyLatestQueueAgentStatistics_30787);
			if (agentQueueIntradayEnabled)
			{
				Add(new IntradayFactQueueJobStep(jobParameters));
				Add(new IntradayFactAgentJobStep(jobParameters));                   // BU independent
			}
			else
			{
				Add(new FactQueueJobStep(jobParameters));                   // BU independent
				Add(new FactAgentJobStep(jobParameters));                   // BU independent
			}

			Add(new StatisticsUpdateNotificationJobStep(jobParameters));                   // BU independent

			if (agentQueueIntradayEnabled)
				Add(new IntradayFactAgentQueueJobStep(jobParameters));              // BU independent
			else
				Add(new FactAgentQueueJobStep(jobParameters));              // BU independent

			Add(new FactQualityLoadJobStep(jobParameters));             // BU independent
			Add(new FactAgentStateJobStep(jobParameters));
			Add(new FactForecastWorkloadJobStep(jobParameters, true));
			Add(new FactScheduleDeviationJobStep(jobParameters, true));
			Add(new FactKpiTargetTeamJobStep(jobParameters));
			Add(new FactRequestJobStep(jobParameters, true));
			Add(new FactRequestedDaysJobStep(jobParameters, true));
			Add(new FactAgentSkillJobStep(jobParameters));

		}
	}

}