using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class IntradayFactAgentQueueJobStep : JobStepBase
	{
		public IntradayFactAgentQueueJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_agent_queue";
			JobCategory = JobCategoryType.AgentStatistics;
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			int affectedRows = 0;

			int chunkTimeSpan;
			//Agg data is never needed in Memory. Hardcode bigger chunks!
			chunkTimeSpan = 10;

			//if (!int.TryParse(ConfigurationManager.AppSettings["chunkTimeSpan"], out chunkTimeSpan))
			//{
			//    chunkTimeSpan = 2;
			//}

			for (DateTime startDateTime = JobCategoryDatePeriod.StartDateUtc;
				 startDateTime.AddDays(chunkTimeSpan) < JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1).AddDays(chunkTimeSpan);
					  startDateTime = startDateTime.AddDays(chunkTimeSpan))
			{

				DateTime endDateTime;

				if (startDateTime.AddDays(chunkTimeSpan) < JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1))
				{
					endDateTime = startDateTime.AddDays(chunkTimeSpan).AddMilliseconds(-1);
				}
				else
				{
					endDateTime = JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1);
				}

				var period = new DateTimePeriod(startDateTime, endDateTime);

				affectedRows += _jobParameters.Helper.Repository.FillIntradayFactAgentQueueDataMart(period, _jobParameters.DataSource, _jobParameters.DefaultTimeZone, RaptorTransformerHelper.CurrentBusinessUnit);
				Result.RowsAffected = affectedRows;
			}

			return affectedRows;
		}
	}
}