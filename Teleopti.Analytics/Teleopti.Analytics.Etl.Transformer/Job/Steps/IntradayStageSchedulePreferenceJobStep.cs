using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class IntradayStageSchedulePreferenceJobStep : JobStepBase
	{
		public IntradayStageSchedulePreferenceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_schedule_preference, stg_day_off, dim_day_off";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new SchedulePreferenceTransformer(_jobParameters.IntervalsPerDay);
			ScheduleDayRestrictor = new ScheduleDayRestrictor();
			DayOffSubStep = new EtlDayOffSubStep();
			SchedulePreferenceInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public ISchedulePreferenceTransformer Transformer { get; set; }
		public IScheduleDayRestrictor ScheduleDayRestrictor { get; set; }
		public IEtlDayOffSubStep DayOffSubStep { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				if (!scenario.DefaultScenario) continue;
				var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
				//Get data from Cashe
				var scheduleDictionary = _jobParameters.StateHolder.GetScheduleCashe();

				// Extract one schedulepart per each person and date
				var scheduleParts = _jobParameters.StateHolder.GetSchedulePartPerPersonAndDate(scheduleDictionary);

				//IList<IScheduleDay> scheduleParts = _jobParameters.StateHolder.LoadSchedulePartsPerPersonAndDate(period, scenario);
				//Remove parts ending too late. This because the schedule repository fetches restrictions on larger period than schedule.
				scheduleParts = ScheduleDayRestrictor.RemoveScheduleDayEndingTooLate(scheduleParts, period.EndDateTime);
				Transformer.Transform(scheduleParts, BulkInsertDataTable1);
			}

			var rowsAffected = _jobParameters.Helper.Repository.PersistSchedulePreferences(BulkInsertDataTable1);
			
			rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.SchedulePreference,
			                                                       RaptorTransformerHelper.CurrentBusinessUnit,
			                                                       _jobParameters.Helper.Repository);

			return rowsAffected;
		}
	}
}
