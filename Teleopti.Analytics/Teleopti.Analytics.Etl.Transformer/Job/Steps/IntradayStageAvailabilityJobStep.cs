using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class IntradayStageAvailabilityJobStep : JobStepBase
	{
		public IntradayStageAvailabilityJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
            Name = "stg_hourly_availability";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new IntradayHourlyAvailabilityTransformer();
			
			HourlyAvailabilityInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IIntradayAvailabilityTransformer Transformer { get; set; }
		

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var rowsAffected = 0;
			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
			{
				var rep = _jobParameters.Helper.Repository;
				var lastTime = rep.LastChangedDate(Result.CurrentBusinessUnit, "Availability");
				_jobParameters.StateHolder.SetThisTime(lastTime, "Availability");
				var changed = rep.ChangedAvailabilityOnStep(lastTime.LastTime, Result.CurrentBusinessUnit);

				Transformer.Transform(changed, BulkInsertDataTable1,_jobParameters.StateHolder,scenario);

				rowsAffected = _jobParameters.Helper.Repository.PersistAvailability(BulkInsertDataTable1);

				
			}

			return rowsAffected;
		}
	}
}
