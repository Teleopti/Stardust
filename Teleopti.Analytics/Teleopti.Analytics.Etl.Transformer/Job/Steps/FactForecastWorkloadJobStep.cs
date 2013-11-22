using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactForecastWorkloadJobStep : JobStepBase
    {
	    private readonly bool _isIntraday;

	    public FactForecastWorkloadJobStep(IJobParameters jobParameters, bool isIntraday = false)
            : base(jobParameters)
        {
			_isIntraday = isIntraday;
			Name = "fact_forecast_workload";
            JobCategory = JobCategoryType.Forecast;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            //Load data from stage to datamart
            var rows =
                _jobParameters.Helper.Repository.FillForecastWorkloadDataMart(
					new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling), RaptorTransformerHelper.CurrentBusinessUnit, _isIntraday);

			if (_isIntraday)
				_jobParameters.StateHolder.UpdateThisTime("Forecast", RaptorTransformerHelper.CurrentBusinessUnit);

			return rows;
        }
    }
}