using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;
using System.Configuration;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class FactScheduleDeviationJobStep : JobStepBase
	{
		private readonly bool _isIntraday;
	    private readonly bool _speedUpEtl;

		public FactScheduleDeviationJobStep(IJobParameters jobParameters, bool isIntraday = false)
			: base(jobParameters)
		{
			_isIntraday = isIntraday;
            _speedUpEtl = jobParameters.ToggleManager.IsEnabled(Toggles.ETL_SpeedUpETL_30791);

			Name = "fact_schedule_deviation";
			JobCategory = JobCategoryType.AgentStatistics;
            
			if (jobParameters.NowForTestPurpose.Equals(null))
			{
				DateTime nowForTestPurpose;
				if (DateTime.TryParse(ConfigurationManager.AppSettings["NowForTestPurpose"], out nowForTestPurpose))
					JobParameters.NowForTestPurpose = nowForTestPurpose;
			}
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			const int chunkTimeSpan = 30;
			var affectedRows = 0;
			
            //assume nightly
            var toDate = JobCategoryDatePeriod.EndDateUtc;
            var fromDate = JobCategoryDatePeriod.StartDateUtc;

			//call once for intraday, dates does not matter for 1st run
		    if (_isIntraday)
		    {
		        var paramIsIntraday = 3;

		        if (!_speedUpEtl)
		            paramIsIntraday = 1;

		        affectedRows =
		            _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(new DateTimePeriod(fromDate, toDate),
		                RaptorTransformerHelper.CurrentBusinessUnit,
		                _jobParameters.DefaultTimeZone,
		                paramIsIntraday,
		                _jobParameters.NowForTestPurpose);


		        affectedRows +=
		            _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(new DateTimePeriod(fromDate, toDate),
		                RaptorTransformerHelper.CurrentBusinessUnit,
		                _jobParameters.DefaultTimeZone,
		                2,
		                _jobParameters.NowForTestPurpose);

		    }
		    else
		    {
		        //call in "nightly"  mode and chunk dates used in Agent period

		        for (DateTime startDateTime = fromDate;
		            startDateTime <= toDate;
		            startDateTime = startDateTime.AddDays(chunkTimeSpan))
		        {

		            DateTime endDateTime = startDateTime.AddDays(chunkTimeSpan) > toDate
		                ? toDate
		                : startDateTime.AddDays(chunkTimeSpan).AddMilliseconds(-1);

		            var period = new DateTimePeriod(startDateTime, endDateTime);

		            affectedRows += _jobParameters.Helper.Repository.FillScheduleDeviationDataMart(period,
		                RaptorTransformerHelper.CurrentBusinessUnit,
		                _jobParameters.DefaultTimeZone,
		                0,
		                _jobParameters.NowForTestPurpose);
		            Result.RowsAffected = affectedRows;
		        }
		    }
		    return affectedRows;
		}
	}
}
