﻿using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactRequestedDaysJobStep : JobStepBase
    {
        public FactRequestedDaysJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "fact_requested_days";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            return _jobParameters.Helper.Repository.FillFactRequestedDaysMart(new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling));
        }
    }
}