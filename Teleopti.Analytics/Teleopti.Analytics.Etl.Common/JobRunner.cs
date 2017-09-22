using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common
{
	public class JobRunner : IJobRunner
	{
		public IList<IJobResult> Run(IJob job, IList<IJobResult> jobResultCollection, IList<IJobStep> jobStepsNotToRun)
		{
			int counter = 0;
			IList<IBusinessUnit> businessUnitCollection = job.JobParameters.Helper.BusinessUnitCollection;
			foreach (var businessUnit in businessUnitCollection)
			{
				var startTime = DateTime.Now;
				var jobResult = job.Run(businessUnit, jobStepsNotToRun, jobResultCollection, counter == 0, counter == businessUnitCollection.Count - 1);
				if (jobResult == null)
					return null;
				jobResult.EndTime = DateTime.Now;
				jobResult.StartTime = startTime;

				counter++;
				jobResultCollection.Add(jobResult);
			}

			publishIndexMaintenanceEvent(job, jobResultCollection);

			return jobResultCollection;
		}

		private static void publishIndexMaintenanceEvent(IJob job, IList<IJobResult> jobResultCollection)
		{
			if (job.JobParameters.ToggleManager.IsEnabled(Toggles.ETL_FasterIndexMaintenance_38847))
			{
				if (job.StepList.GetType() == typeof(NightlyJobCollection))
				{
					if (job.JobParameters.RunIndexMaintenance)
					{
						var eventPublisher = job.JobParameters.ContainerHolder.IocContainer.Resolve<IEventPublisher>();
						var dataSourceScope = job.JobParameters.ContainerHolder.IocContainer.Resolve<IDataSourceScope>();
						var jobHelper = job.JobParameters.Helper;
						var tenant = jobHelper.SelectedDataSource.DataSourceName;
						using (dataSourceScope.OnThisThreadUse(new DummyDataSource(tenant)))
						{
							eventPublisher.Publish(new IndexMaintenanceEvent
							{
								JobName = $"Index Maintenance for {tenant}",
								UserName = "Index Maintenance",
								AllStepsSuccess = jobResultCollection.All(x => x.Success)
							});
						}
					}
				}
			}
		}

		public void SaveResult(IList<IJobResult> jobResultCollection, IJobLogRepository jobLogRepository, int jobScheduleId)
		{
			foreach (var jobResult in jobResultCollection)
			{
				IEtlJobLog etlJobLogItem = new EtlJobLog(jobLogRepository);
				etlJobLogItem.Init(jobScheduleId, jobResult.StartTime, jobResult.EndTime);

				foreach (var jobStepResult in jobResult.JobStepResultCollection)
					etlJobLogItem.PersistJobStep(jobStepResult);

				etlJobLogItem.Persist(jobResult);
			}
		}
	}
}