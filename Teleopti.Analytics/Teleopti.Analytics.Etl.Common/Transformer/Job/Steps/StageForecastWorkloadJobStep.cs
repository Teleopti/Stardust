using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageForecastWorkloadJobStep : JobStepBase
	{
		public StageForecastWorkloadJobStep(IJobParameters jobParameters)
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

			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				//Get data from Raptor
				IList<ISkill> skills = _jobParameters.StateHolder.SkillCollection;
				ICollection<ISkillDay> rootList = _jobParameters.StateHolder.GetSkillDaysCollection(period, skills, scenario);

				var raptorTransformer = new ForecastWorkloadTransformer(_jobParameters.IntervalsPerDay, DateTime.Now);
				raptorTransformer.Transform(rootList, BulkInsertDataTable1);
			}

			//Truncate staging table & Bulk insert data to staging database
			return _jobParameters.Helper.Repository.PersistForecastWorkload(BulkInsertDataTable1);
		}
	}
}