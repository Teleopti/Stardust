using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactSchedulePreferenceJobStep : JobStepBase
    {
		public FactSchedulePreferenceJobStep(IJobParameters jobParameters) : base(jobParameters)
        {
	        Name = "fact_schedule_preference";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);

            //Load data from stage to datamart
            return
                _jobParameters.Helper.Repository.FillFactSchedulePreferenceMart(period,
                _jobParameters.DefaultTimeZone, RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}
