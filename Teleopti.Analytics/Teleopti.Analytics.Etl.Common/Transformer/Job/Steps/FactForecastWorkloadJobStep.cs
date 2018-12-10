using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
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
			var rows = _jobParameters.Helper.Repository.FillForecastWorkloadDataMart(new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, 
				JobCategoryDatePeriod.EndDateUtcCeiling), 
				RaptorTransformerHelper.CurrentBusinessUnit);

			return rows;
		}
	}
}