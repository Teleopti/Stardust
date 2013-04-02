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
		private readonly INeedToRunChecker _needToRunChecker;

        public StageScheduleJobStep(IJobParameters jobParameters)
            : this(jobParameters, new ScheduleTransformer())
        {}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		public StageScheduleJobStep(IJobParameters jobParameters, INeedToRunChecker needToRunChecker)
			: this(jobParameters, new ScheduleTransformer(), needToRunChecker)
		{}

        public StageScheduleJobStep(IJobParameters jobParameters, IScheduleTransformer raptorTransformer)
			: this(jobParameters, raptorTransformer, new DefaultNeedToRunChecker())
        {}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		public StageScheduleJobStep(IJobParameters jobParameters, IScheduleTransformer raptorTransformer, INeedToRunChecker needToRunChecker)
			: base(jobParameters)
		{
			Name = "stg_schedule, stg_schedule_day_absence_count";
			JobCategory = JobCategoryType.Schedule;
			_raptorTransformer = raptorTransformer;
			_needToRunChecker = needToRunChecker;
		}

	    protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
			
            //Transform data from Raptor to Matrix format
            _raptorTransformer.RowsUpdatedEvent += raptorTransformer_RowsUpdatedEvent;
            //Truncate stage table
            _jobParameters.Helper.Repository.TruncateSchedule();

			if (!_needToRunChecker.NeedToRun(period, _jobParameters.Helper.Repository, Result.CurrentBusinessUnit, Name))
			{
				//gets resetted to Done later :(
				Result.Status = "No need to run";
				return 0;
			}

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