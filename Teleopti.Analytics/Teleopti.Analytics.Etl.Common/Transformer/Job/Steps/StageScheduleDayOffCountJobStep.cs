using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageScheduleDayOffCountJobStep : JobStepBase
	{
		public StageScheduleDayOffCountJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_schedule_day_off_count, stg_day_off, dim_day_off";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new ScheduleDayOffCountTransformer();
			DayOffSubStep = new EtlDayOffSubStep();
			ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IScheduleDayOffCountTransformer Transformer { get; set; }
		public IEtlDayOffSubStep DayOffSubStep { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor,
														JobCategoryDatePeriod.EndDateUtcCeiling);

			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				//Get data from Raptor
				var scheduleDictionary = _jobParameters.StateHolder.GetSchedules(period, scenario);

				// Extract one schedulepart per each person and date
				var rootList = _jobParameters.StateHolder.GetSchedulePartPerPersonAndDate(scheduleDictionary);

				//Transform data from Raptor to Matrix format
				Transformer.Transform(rootList, BulkInsertDataTable1, _jobParameters.IntervalsPerDay);
			}

			var rowsAffected = _jobParameters.Helper.Repository.PersistScheduleDayOffCount(BulkInsertDataTable1);
			rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.ScheduleDayOff,
																					 RaptorTransformerHelper.CurrentBusinessUnit,
																					 _jobParameters.Helper.Repository);


			return rowsAffected;
		}
	}
}