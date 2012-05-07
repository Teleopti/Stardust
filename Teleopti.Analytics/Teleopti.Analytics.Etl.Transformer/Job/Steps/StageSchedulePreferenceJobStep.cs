using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    /// <summary>
    /// Responsible for retriving the PersonRestrictions  and send it to the transformer
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-07-02
    /// </remarks>
    public class StageSchedulePreferenceJobStep : JobStepBase
    {

        public ISchedulePreferenceTransformer Transformer { get; protected set; }
        public IScheduleDayRestrictor ScheduleDayRestrictor { get; protected set; }
  
        public StageSchedulePreferenceJobStep(IJobParameters jobParameters):base(jobParameters)
        {
            Name = "stg_schedule_preference";
            JobCategory = JobCategoryType.Schedule;
            Transformer = new SchedulePreferenceTransformer(_jobParameters.IntervalsPerDay);
            ScheduleDayRestrictor = new ScheduleDayRestrictor();
            SchedulePreferenceInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
            //IScenario scenario = _jobParameters.StateHolder.DefaultScenario;

			foreach (IScenario scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded)
			{
				IList<IScheduleDay> scheduleParts = _jobParameters.StateHolder.LoadSchedulePartsPerPersonAndDate(period, scenario);
			    //Remove parts ending too late. This because the schedule repository fetches restrictions on larger period than schedule.
                scheduleParts = ScheduleDayRestrictor.RemoveScheduleDayEndingTooLate(scheduleParts, period.EndDateTime);
				Transformer.Transform(scheduleParts, BulkInsertDataTable1);
			}
            //Truncate staging table & Bulk insert data to staging database
            return _jobParameters.Helper.Repository.PersistSchedulePreferences(BulkInsertDataTable1);
        }
    }
}
