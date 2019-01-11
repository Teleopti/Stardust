using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs
{
	public class JobCollection : List<IJob>
	{
		public JobCollection(IJobParameters jobParameters)
		{
			Add(new JobBase(jobParameters, new InitialJobCollection(jobParameters), "Initial", true, false));
			Add(new JobBase(jobParameters, new IntradayJobCollection(jobParameters), "Intraday", true, true));
			Add(new JobBase(jobParameters, new NightlyJobCollection(jobParameters), "Nightly", true, true));
			Add(new JobBase(jobParameters, new WorkloadQueuesJobCollection(jobParameters), "Workload Queues", false, true));
			Add(new JobBase(jobParameters, new QueueStatisticsJobCollection(jobParameters), "Queue Statistics", true, true));
			Add(new JobBase(jobParameters, new AgentStatisticsJobCollection(jobParameters), "Agent Statistics", true, true));
			Add(new JobBase(jobParameters, new ScheduleJobCollection(jobParameters), "Schedule", true, false));
			Add(new JobBase(jobParameters, new ForecastJobCollection(jobParameters), "Forecast", true, false));
			Add(new JobBase(jobParameters, new PermissionJobCollection(jobParameters), "Permission", false, false));
			Add(new JobBase(jobParameters, new AgentSkillCollection(jobParameters), "Person Skill", false, false));
			Add(new JobBase(jobParameters, new KpiJobCollection(jobParameters), "KPI", false, false));
			Add(new JobBase(jobParameters, new QueueAndAgentLogOnJobCollection(jobParameters), "Queue and Agent login synchronization", false, true));
			Add(new JobBase(jobParameters, new ReloadDatamartJobCollection(jobParameters), "Reload datamart (old nightly)", true, true));
			Add(new JobBase(jobParameters, new CleanupJobCollection(jobParameters), "Cleanup", false, false));

			// If PM is installed then show ETL job for processing cube
			if (jobParameters.IsPmInstalled)
			{
				Add(new JobBase(jobParameters, new PerformanceManagerJobCollection(jobParameters), "Process Cube", false, false));
			}

			Add(new JobBase(jobParameters, new UpgradeMaintenanceJobCollection(jobParameters), "Upgrade Maintenance", false, false));

			if (jobParameters.ToggleManager.IsEnabled(Toggles.WFM_Insights_78059) && jobParameters.InsightsEnabled)
			{
				Add(new JobBase(jobParameters, new InsightsDataRefreshJobCollection(jobParameters),
					"Insights data refresh", false, false));
			}
		}
	}
}
