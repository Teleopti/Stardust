using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class FactScheduleDeviationJobStep : JobStepBase
	{
		public FactScheduleDeviationJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_schedule_deviation";
			JobCategory = JobCategoryType.AgentStatistics;
		}


		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Deviation data is never loaded into .net memory, just SQL Server stuff. Hardcode bigger chunks!
			const int chunkTimeSpan = 30;
		    var affectedRows = 0;
            var includedTodayUtc = JobParameters.DefaultTimeZone.SafeConvertTimeToUtc(DateTime.Today.AddDays(1).AddMilliseconds(-1));


			if (JobCategoryDatePeriod.StartDateUtc > includedTodayUtc)
				return affectedRows;

			var toDate = JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1) > includedTodayUtc
			                      ? includedTodayUtc.AddDays(chunkTimeSpan)
			                      : JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1).AddDays(chunkTimeSpan);

			for (DateTime startDateTime = JobCategoryDatePeriod.StartDateUtc;
				startDateTime.AddDays(chunkTimeSpan) < toDate;
					startDateTime = startDateTime.AddDays(chunkTimeSpan))
			{
				var endDateTime = JobCategoryDatePeriod.EndDateUtc.AddDays(1).AddMilliseconds(-1);

				if (startDateTime.AddDays(chunkTimeSpan) > includedTodayUtc)
					endDateTime = includedTodayUtc;
				else if (startDateTime.AddDays(chunkTimeSpan) < endDateTime)
					endDateTime = startDateTime.AddDays(chunkTimeSpan).AddMilliseconds(-1);

				var period = new DateTimePeriod(startDateTime, endDateTime);

				affectedRows += _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(period,
																							   RaptorTransformerHelper.CurrentBusinessUnit, 
																							   _jobParameters.DefaultTimeZone);
				Result.RowsAffected = affectedRows;
			}

			return affectedRows;
		}
	}
}
