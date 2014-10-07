using System;
using System.Collections.Generic;
using System.Configuration;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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
			if (!int.TryParse(ConfigurationManager.AppSettings["chunkTimeSpan"], out chunkTimeSpan))
			{
				chunkTimeSpan = 2;
			}

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