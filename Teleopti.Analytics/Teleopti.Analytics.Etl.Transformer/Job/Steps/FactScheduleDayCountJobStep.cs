using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactScheduleDayCountJobStep : JobStepBase
    {
	    private readonly bool _isIntraday;

	    public FactScheduleDayCountJobStep(IJobParameters jobParameters, bool isIntraday = false)
            : base(jobParameters)
        {
			_isIntraday = isIntraday;
			Name = "fact_schedule_day_count";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			if(_isIntraday)
				foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
				{
					var rows = _jobParameters.Helper.Repository.FillIntradayScheduleDayCountDataMart(RaptorTransformerHelper.CurrentBusinessUnit, scenario);
					_jobParameters.StateHolder.UpdateThisTime("Schedules", RaptorTransformerHelper.CurrentBusinessUnit);
					return rows;
				}
	        

			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
            //Load datamart
            return
				_jobParameters.Helper.Repository.FillScheduleDayCountDataMart(period,
                    RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}