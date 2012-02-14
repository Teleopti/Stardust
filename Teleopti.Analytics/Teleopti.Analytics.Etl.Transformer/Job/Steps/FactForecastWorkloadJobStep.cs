using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactForecastWorkloadJobStep : JobStepBase
    {
        public FactForecastWorkloadJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "fact_forecast_workload";
            JobCategory = JobCategoryType.Forecast;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Load data from stage to datamart
            return
                _jobParameters.Helper.Repository.FillForecastWorkloadDataMart(
                    new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling));
        }
    }
}