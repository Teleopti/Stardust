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
		private readonly bool _isIntraday;

		public FactScheduleDeviationJobStep(IJobParameters jobParameters, bool isIntraday = false)
			: base(jobParameters)
		{
			_isIntraday = isIntraday;
			Name = "fact_schedule_deviation";
			JobCategory = JobCategoryType.AgentStatistics;
		}


		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Deviation data is never loaded into .net memory, just SQL Server stuff. Hardcode bigger chunks!
			const int chunkTimeSpan = 30;
		    var affectedRows = 0;
            var includedTodayUtc = JobParameters.DefaultTimeZone.SafeConvertTimeToUtc(DateTime.Today.AddDays(1).AddMilliseconds(-1));
            var includedYesterdayUtc = JobParameters.DefaultTimeZone.SafeConvertTimeToUtc(DateTime.Today.AddDays(-1));

            // should we really do this check now when this is in the JobCategoryType.AgentStatistics
            // if we choose to update in future so what???
			if (JobCategoryDatePeriod.StartDateUtc > includedTodayUtc && !_isIntraday)
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
                // part of #24257 
                if(_isIntraday)
                    period = new DateTimePeriod(includedYesterdayUtc, includedTodayUtc);

				affectedRows += _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(period,
																							   RaptorTransformerHelper.CurrentBusinessUnit, 
																							   _jobParameters.DefaultTimeZone,
																							   _isIntraday);
				Result.RowsAffected = affectedRows;
			}

			return affectedRows;
		}
	}
}
