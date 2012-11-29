using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
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

			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				//Get data from Raptor
				IScheduleDictionary scheduleDictionary = _jobParameters.StateHolder.GetSchedules(period, scenario);

				// Extract one schedulepart per each person and date
				IList<IScheduleDay> rootList = _jobParameters.StateHolder.GetSchedulePartPerPersonAndDate(scheduleDictionary);

				//Transform data from Raptor to Matrix format
				Transformer.Transform(rootList, BulkInsertDataTable1, _jobParameters.IntervalsPerDay);
			}

			int rowsAffected = _jobParameters.Helper.Repository.PersistScheduleDayOffCount(BulkInsertDataTable1);
			rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.ScheduleDayOff,
			                                                       RaptorTransformerHelper.CurrentBusinessUnit,
			                                                       _jobParameters.Helper.Repository);
			

			return rowsAffected;
		}
	}
}