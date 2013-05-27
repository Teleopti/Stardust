﻿using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class StageAvailabilityJobStep : JobStepBase
	{
		public StageAvailabilityJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "Stage Hourly Availability";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new HourlyAvailabilityTransformer();
			ScheduleDayRestrictor = new ScheduleDayRestrictor();
			HourlyAvailabilityInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IEtlTransformer<IScheduleDay> Transformer { get; set; }
		public IScheduleDayRestrictor ScheduleDayRestrictor { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
			
			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				var scheduleParts = _jobParameters.StateHolder.LoadSchedulePartsPerPersonAndDate(period, scenario);
				//Remove parts ending too late. This because the schedule repository fetches restrictions on larger period than schedule.
				scheduleParts = ScheduleDayRestrictor.RemoveScheduleDayEndingTooLate(scheduleParts, period.EndDateTime);
				Transformer.Transform(scheduleParts, BulkInsertDataTable1);
			}

			var rowsAffected = _jobParameters.Helper.Repository.PersistAvailability(BulkInsertDataTable1);
			
			return rowsAffected;
		}
	}
}
