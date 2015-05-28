using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using RowsUpdatedEventArgs = Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading.RowsUpdatedEventArgs;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class IntradayStageScheduleJobStep : JobStepBase
	{
		private readonly IScheduleTransformer _raptorTransformer;

		public IntradayStageScheduleJobStep(IJobParameters jobParameters)
			: this(jobParameters, new ScheduleTransformer())
		{ }

		public IntradayStageScheduleJobStep(IJobParameters jobParameters, IScheduleTransformer raptorTransformer)
			: base(jobParameters)
		{
			Name = "stg_schedule, stg_schedule_day_absence_count";
			JobCategory = JobCategoryType.Schedule;
			_raptorTransformer = raptorTransformer;
			ScheduleChangedInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Transform data from Raptor to Matrix format
			_raptorTransformer.RowsUpdatedEvent += raptorTransformerRowsUpdatedEvent;
			//Truncate stage table
			var rep = _jobParameters.Helper.Repository;
			rep.TruncateSchedule();

			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
			{
				// get it from stateholder and let it hold it and update in the end??????
				var lastTime = rep.LastChangedDate(Result.CurrentBusinessUnit, "Schedules");
				_jobParameters.StateHolder.SetThisTime(lastTime, "Schedules");
				var changed = rep.ChangedDataOnStep(lastTime.LastTime, Result.CurrentBusinessUnit, "Schedules");

				if (!changed.Any()) return 0;
				ScheduleChangedInfrastructure.AddRows(BulkInsertDataTable1, changed, scenario, Result.CurrentBusinessUnit);
				rep.PersistScheduleChanged(BulkInsertDataTable1);

				//Get data from Raptor
				var dictionary = _jobParameters.StateHolder.GetSchedules(changed, scenario);

				// Extract one schedulepart per each person and date
				var rootList = _jobParameters.StateHolder.GetSchedulePartPerPersonAndDate(dictionary);

				_raptorTransformer.Transform(rootList, DateTime.Now, _jobParameters, new ThreadPool());
			}

			return Result.RowsAffected.GetValueOrDefault(0);
		}

		void raptorTransformerRowsUpdatedEvent(object sender, RowsUpdatedEventArgs e)
		{
			Result.RowsAffected += e.AffectedRows;

		}
	}
}