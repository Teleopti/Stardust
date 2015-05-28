using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class IntradayStageSchedulePreferenceJobStep : JobStepBase
	{
		public IntradayStageSchedulePreferenceJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "stg_schedule_preference, stg_day_off, dim_day_off";
			JobCategory = JobCategoryType.Schedule;
			Transformer = new IntradaySchedulePreferenceTransformer();
			DayOffSubStep = new EtlDayOffSubStep();
			SchedulePreferenceInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		public IIntradaySchedulePreferenceTransformer Transformer { private get; set; }
		public IEtlDayOffSubStep DayOffSubStep { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var rowsAffected = 0;
			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
			{
				var rep = _jobParameters.Helper.Repository;
				var lastTime = rep.LastChangedDate(Result.CurrentBusinessUnit, "Preferences");
				_jobParameters.StateHolder.SetThisTime(lastTime, "Preferences");
				var changed = rep.ChangedPreferencesOnStep(lastTime.LastTime, Result.CurrentBusinessUnit);

				Transformer.Transform(changed, BulkInsertDataTable1,_jobParameters.StateHolder,scenario);

				rowsAffected = _jobParameters.Helper.Repository.PersistSchedulePreferences(BulkInsertDataTable1);

				rowsAffected += DayOffSubStep.StageAndPersistToMart(DayOffEtlLoadSource.SchedulePreference,
																	   RaptorTransformerHelper.CurrentBusinessUnit,
																	   _jobParameters.Helper.Repository);
			}

			return rowsAffected;
		}
	}
}
