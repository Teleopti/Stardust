using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.ScheduleThreading;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;
using RowsUpdatedEventArgs=Teleopti.Analytics.Etl.Transformer.ScheduleThreading.RowsUpdatedEventArgs;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageScheduleJobStep : JobStepBase
    {
		private readonly IScheduleTransformer _raptorTransformer;
		
        public StageScheduleJobStep(IJobParameters jobParameters)
            : this(jobParameters, new ScheduleTransformer())
        {}

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

                _raptorTransformer.Transform(rootList, DateTime.Now, _jobParameters, new ThreadPool());
            }

            return Result.RowsAffected.GetValueOrDefault(0);
        }

        void raptorTransformer_RowsUpdatedEvent(object sender, RowsUpdatedEventArgs e)
        {
            Result.RowsAffected += e.AffectedRows;

        }
    }
}