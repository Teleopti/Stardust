using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class IntradayStageForecastWorkloadJobStep : JobStepBase
    {
		public IntradayStageForecastWorkloadJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_forecast_workload";
            JobCategory = JobCategoryType.Forecast;
            ForecastWorkloadInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			var rep = _jobParameters.Helper.Repository;

			var lastTime = rep.LastChangedDate(Result.CurrentBusinessUnit, "Forecast");
			_jobParameters.StateHolder.SetThisTime(lastTime, "Forecast");

	        if (lastTime.ThisTime > lastTime.LastTime)
	        {
				foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
				{
					//Get data from Raptor
					var rootList = _jobParameters.StateHolder.GetSkillDaysCollection(scenario,lastTime.LastTime);

					var raptorTransformer = new ForecastWorkloadTransformer(_jobParameters.IntervalsPerDay, DateTime.Now);
					raptorTransformer.Transform(rootList, BulkInsertDataTable1);
				}
	        }

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistForecastWorkload(BulkInsertDataTable1);
        }
    }
}