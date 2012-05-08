using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.JobLog;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common
{
	public class JobRunner : IJobRunner
	{
		public IList<IJobResult> Run(IJob job, IList<IBusinessUnit> businessUnitCollection, IList<IJobResult> jobResultCollection, IList<IJobStep> jobStepsNotToRun)
		{
			int counter = 0;

			foreach (var businessUnit in businessUnitCollection)
			{
				var startTime = DateTime.Now;
				var jobResult = job.Run(businessUnit, jobStepsNotToRun, jobResultCollection, counter == 0, counter == businessUnitCollection.Count - 1);
				jobResult.EndTime = DateTime.Now;
				jobResult.StartTime = startTime;
				
				counter++;
				jobResultCollection.Add(jobResult);
			}

			return jobResultCollection;
		}

		public void SaveResult(IList<IJobResult> jobResultCollection, ILogRepository logRepository, int jobScheduleId)
		{
			foreach (var jobResult in jobResultCollection)
			{
				IEtlJobLog etlJobLogItem = new EtlJobLog(logRepository);
				etlJobLogItem.Init(jobScheduleId, jobResult.StartTime, jobResult.EndTime);

				foreach (var jobStepResult in jobResult.JobStepResultCollection)
					etlJobLogItem.PersistJobStep(jobStepResult);

				etlJobLogItem.Persist(jobResult);
			}
		}
	}
}