using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
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
            Name = "stg_schedule_day_off_count";
            JobCategory = JobCategoryType.Schedule;
            ScheduleDayOffCountInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }


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
                var raptorTransformer = new ScheduleDayOffCountTransformer(_jobParameters.IntervalsPerDay);
                raptorTransformer.Transform(rootList, BulkInsertDataTable1);
            }

            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistScheduleDayOffCount(BulkInsertDataTable1);
        }
    }
}