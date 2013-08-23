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
			// Set start date one day earlier to be sure to get hold of all skill days
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor.AddDays(-1).AddHours(2),
											JobCategoryDatePeriod.EndDateUtcCeiling);

			var rep = _jobParameters.Helper.Repository;

			var lastTime = rep.LastChangedDate(Result.CurrentBusinessUnit, "Forecast", period);
			_jobParameters.StateHolder.SetThisTime(lastTime, "Forecast");

	        if (lastTime.ThisTime > lastTime.LastTime)
	        {
				foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
				{
					//Get data from Raptor
					var skills = _jobParameters.StateHolder.SkillCollection;
					var rootList = _jobParameters.StateHolder.GetSkillDaysCollection(period, skills, scenario);

					var raptorTransformer = new ForecastWorkloadTransformer(_jobParameters.IntervalsPerDay, DateTime.Now);
					raptorTransformer.Transform(rootList, BulkInsertDataTable1);
				}
	        }

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistForecastWorkload(BulkInsertDataTable1);
        }
    }
}