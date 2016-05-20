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
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new StagePersonJobStep(jobParameters));
				Add(new StageAgentSkillJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayActivity_38303))
			{
				Add(new StageActivityJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayAbsence_38301))
			{
				Add(new StageAbsenceJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpScenario_38300))
			{
				Add(new StageScenarioJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayShiftCategory_38718))
			{
				Add(new StageShiftCategoryJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791))
			{
				Add(new IntradayStageScheduleJobStep(jobParameters));
				Add(new IntradayStageScheduleDayOffCountJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayPreference_37124))
			{
				Add(new IntradayStageSchedulePreferenceJobStep(jobParameters));
			}
			Add(new IntradayStageAvailabilityJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradaySkill_37543))
			{
				Add(new StageSkillJobStep(jobParameters));
			}
			Add(new StageWorkloadJobStep(jobParameters));
			Add(new IntradayStageForecastWorkloadJobStep(jobParameters));
			Add(new StageKpiJobStep(jobParameters));
			Add(new StageScorecardJobStep(jobParameters));
			Add(new StageScorecardKpiJobStep(jobParameters));
			Add(new StageKpiTargetTeamJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623)
				|| !jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new StageGroupPagePersonJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayOvertime_38304))
			{
				Add(new StageOvertimeJobStep(jobParameters));
			}
			
			Add(new IntradayStageRequestJobStep(jobParameters));

			// DIM AND BRIDGE TABLES AND QUEUE/AGENT SYNC
			Add(new DimBusinessUnitJobStep(jobParameters));
			Add(new DimScorecardJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new DimSiteJobStep(jobParameters));
				Add(new DimTeamJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradaySkill_37543))
			{
				Add(new DimSkillJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new DimSkillSetJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new DimPersonJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayActivity_38303))
			{
				Add(new DimActivityJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayAbsence_38301))
			{
				Add(new DimAbsenceJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpScenario_38300))
			{
				Add(new DimScenarioJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayShiftCategory_38718))
			{
				Add(new DimShiftCategoryJobStep(jobParameters));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791))
			{
				Add(new DimShiftLengthJobStep(jobParameters));
			}
			Add(new DimWorkloadJobStep(jobParameters));
			Add(new DimKpiJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayOvertime_38304))
			{
				Add(new DimOvertimeJobStep(jobParameters));
			}
			Add(new ScorecardKpiJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new BridgeSkillSetSkillJobStep(jobParameters));
				Add(new BridgeAcdLogOnPersonJobStep(jobParameters));
			}

			Add(new BridgeQueueWorkloadJobStep(jobParameters));
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpGroupPagePersonIntraday_37623)
				|| !jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new DimGroupPageJobStep(jobParameters));
				Add(new BridgeGroupPagePersonJobStep(jobParameters));
			}

			// FACT TABLES
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791))
			{
				Add(new FactScheduleJobStep(jobParameters, true));
				Add(new FactScheduleDayCountJobStep(jobParameters, true));
			}
			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpIntradayPreference_37124))
			{
				Add(new FactSchedulePreferenceJobStep(jobParameters, true));
			}
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



			Add(new FactForecastWorkloadJobStep(jobParameters, true));
			Add(new FactScheduleDeviationJobStep(jobParameters, true));
			Add(new FactKpiTargetTeamJobStep(jobParameters));
			Add(new FactRequestJobStep(jobParameters, true));
			Add(new FactRequestedDaysJobStep(jobParameters, true));

			if (!jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439))
			{
				Add(new FactAgentSkillJobStep(jobParameters));
			}

		}
	}

}