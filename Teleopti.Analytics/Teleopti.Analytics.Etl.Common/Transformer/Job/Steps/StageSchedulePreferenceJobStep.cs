using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageSchedulePreferenceJobStep : JobStepBase
	{
		public StageSchedulePreferenceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_schedule_preference, stg_day_off, dim_day_off";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new SchedulePreferenceTransformer();
			ScheduleDayRestrictor = new ScheduleDayRestrictor();
			DayOffSubStep = new EtlDayOffSubStep();
			SchedulePreferenceInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public ISchedulePreferenceTransformer Transformer { get; set; }
		public IScheduleDayRestrictor ScheduleDayRestrictor { get; set; }
		public IEtlDayOffSubStep DayOffSubStep { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			// to fetch when agents in far away timezones bug #43288
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling.AddDays(1));
			//IScenario scenario = _jobParameters.StateHolder.DefaultScenario;

			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				var scheduleParts = _jobParameters.StateHolder.LoadSchedulePartsPerPersonAndDate(period, scenario);
				//Remove parts ending too late. This because the schedule repository fetches restrictions on larger period than schedule.
				scheduleParts = ScheduleDayRestrictor.RemoveScheduleDayEndingTooLate(scheduleParts, period.EndDateTime);
				Transformer.Transform(scheduleParts, BulkInsertDataTable1);
			}

			int rowsAffected = _jobParameters.Helper.Repository.PersistSchedulePreferences(BulkInsertDataTable1);
			
			rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.SchedulePreference,
			                                                       RaptorTransformerHelper.CurrentBusinessUnit,
			                                                       _jobParameters.Helper.Repository);

			return rowsAffected;
		}
	}
}
