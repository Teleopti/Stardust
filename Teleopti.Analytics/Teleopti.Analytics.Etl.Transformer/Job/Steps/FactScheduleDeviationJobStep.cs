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
			const int chunkTimeSpan = 30;
		    var affectedRows = 0;

			//call once for intraday, dates does not matter
			if (_isIntraday)
			{
				affectedRows = _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(new DateTimePeriod(JobCategoryDatePeriod.StartDateUtc,JobCategoryDatePeriod.EndDateUtc), 
																								   RaptorTransformerHelper.CurrentBusinessUnit, 
																								   _jobParameters.DefaultTimeZone,
																								   true);
			}

			//then call in "nightly"  mode and chunk dates used in Agent period
			var toDate = JobCategoryDatePeriod.EndDateUtc;
		    for (DateTime startDateTime = JobCategoryDatePeriod.StartDateUtc;
                startDateTime <= toDate;
					startDateTime = startDateTime.AddDays(chunkTimeSpan))
				{
			    
					DateTime endDateTime = startDateTime.AddDays(chunkTimeSpan) > toDate ? toDate : startDateTime.AddDays(chunkTimeSpan).AddMilliseconds(-1);

					var period = new DateTimePeriod(startDateTime, endDateTime);
                
					affectedRows += _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(period,
																								   RaptorTransformerHelper.CurrentBusinessUnit, 
																								   _jobParameters.DefaultTimeZone,
																								   false);
					Result.RowsAffected = affectedRows;
				}

			return affectedRows;
		}
	}
}
