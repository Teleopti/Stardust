using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactScheduleJobStep : JobStepBase
    {
	    private readonly bool _isIntraday;

	    public FactScheduleJobStep(IJobParameters jobParameters):this(jobParameters, false){}
		public FactScheduleJobStep(IJobParameters jobParameters, bool isIntraday)
            : base(jobParameters)
        {
			_isIntraday = isIntraday;
			Name = "fact_schedule";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			//Load data from stage to datamart 
			if(_isIntraday)
				return _jobParameters.Helper.Repository.FillIntradayScheduleDataMart(RaptorTransformerHelper.CurrentBusinessUnit);

			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
            return
				_jobParameters.Helper.Repository.FillScheduleDataMart(period, RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}