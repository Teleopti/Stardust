using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;
using RowsUpdatedEventArgs = Teleopti.Analytics.Etl.Common.Transformer.ScheduleThreading.RowsUpdatedEventArgs;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class StageScheduleJobStep : JobStepBase
	{
		private readonly IScheduleTransformer _raptorTransformer;

		public StageScheduleJobStep(IJobParameters jobParameters)
			: this(jobParameters, new ScheduleTransformer())
		{ }

		public StageScheduleJobStep(IJobParameters jobParameters, IScheduleTransformer raptorTransformer)
			: base(jobParameters)
		{
			Name = "stg_schedule, stg_schedule_day_absence_count";
			JobCategory = JobCategoryType.Schedule;
			_raptorTransformer = raptorTransformer;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling.AddDays(1));

			//Transform data from Raptor to Matrix format
			_raptorTransformer.RowsUpdatedEvent += raptorTransformer_RowsUpdatedEvent;
			//Truncate stage table
			_jobParameters.Helper.Repository.TruncateSchedule();

			foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{

				//Get data from Raptor
				var scheduleDictionary = _jobParameters.StateHolder.GetSchedules(period, scenario);

				// Extract one schedulepart per each person and date
				var rootList = _jobParameters.StateHolder.GetSchedulePartPerPersonAndDate(scheduleDictionary);

				using (var threadPool = new ThreadPool())
					_raptorTransformer.Transform(rootList, DateTime.Now, _jobParameters, threadPool);
			}

			return Result.RowsAffected.GetValueOrDefault(0);
		}

		void raptorTransformer_RowsUpdatedEvent(object sender, RowsUpdatedEventArgs e)
		{
			Result.RowsAffected += e.AffectedRows;

		}
	}
}