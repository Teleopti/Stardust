using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactScheduleDayCountJobStep : JobStepBase
    {
		public FactScheduleDayCountJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
	        Name = "fact_schedule_day_count";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
            //Load datamart
            return
				_jobParameters.Helper.Repository.FillScheduleDayCountDataMart(period,
                    RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}