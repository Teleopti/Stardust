﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactSchedulePreferenceJobStep : JobStepBase
    {
		private readonly bool _isIntraday;

		public FactSchedulePreferenceJobStep(IJobParameters jobParameters) : this(jobParameters, false) { }
		public FactSchedulePreferenceJobStep(IJobParameters jobParameters, bool isIntraday ) : base(jobParameters)
        {
	        Name = "fact_schedule_preference";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			if (_isIntraday)
				foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
				{
					var rows =_jobParameters.Helper.Repository.FillIntradayFactSchedulePreferenceMart(RaptorTransformerHelper.CurrentBusinessUnit, scenario);
					_jobParameters.StateHolder.UpdateThisTime("Preferences", RaptorTransformerHelper.CurrentBusinessUnit);
					return rows;
				}
				

			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);

            //Load data from stage to datamart
            return
                _jobParameters.Helper.Repository.FillFactSchedulePreferenceMart(period,
                _jobParameters.DefaultTimeZone, RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}
