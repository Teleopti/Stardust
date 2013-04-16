using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactScheduleDayCountJobStep : JobStepBase
    {
	    private readonly bool _isIntraday;
	    public FactScheduleDayCountJobStep(IJobParameters jobParameters):this(jobParameters,false){}
		public FactScheduleDayCountJobStep(IJobParameters jobParameters, bool isIntraday)
            : base(jobParameters)
        {
			_isIntraday = isIntraday;
			Name = "fact_schedule_day_count";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			if(_isIntraday)
				return
				_jobParameters.Helper.Repository.FillIntradayScheduleDayCountDataMart(RaptorTransformerHelper.CurrentBusinessUnit);

			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
            //Load datamart
            return
				_jobParameters.Helper.Repository.FillScheduleDayCountDataMart(period,
                    RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}