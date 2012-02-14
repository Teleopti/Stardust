using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactScheduleJobStep : JobStepBase
    {
        public FactScheduleJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "fact_schedule";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Load data from stage to datamart
            return
                _jobParameters.Helper.Repository.FillScheduleDataMart(
                    new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor,
                                       JobCategoryDatePeriod.EndDateUtcCeiling));
        }
    }
}