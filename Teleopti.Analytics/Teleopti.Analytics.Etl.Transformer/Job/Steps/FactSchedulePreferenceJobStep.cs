using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            //Load data from stage to datamart
            return
                _jobParameters.Helper.Repository.FillFactSchedulePreferenceMart(
                    new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor,
                                       JobCategoryDatePeriod.EndDateUtcCeiling),
                _jobParameters.DefaultTimeZone);
        }
    }
}
