using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactSchedulePreferenceJobStep : JobStepBase
    {
	    private readonly INeedToRunChecker _needToRunChecker;

	    public FactSchedulePreferenceJobStep(IJobParameters jobParameters) :this(jobParameters, new DefaultNeedToRunChecker()){}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		public FactSchedulePreferenceJobStep(IJobParameters jobParameters, INeedToRunChecker needToRunChecker) : base(jobParameters)
        {
	        _needToRunChecker = needToRunChecker;
	        Name = "fact_schedule_preference";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
			if (!_needToRunChecker.NeedToRun(period, _jobParameters.Helper.Repository, RaptorTransformerHelper.CurrentBusinessUnit, Name))
				return 0;
            //Load data from stage to datamart
            return
                _jobParameters.Helper.Repository.FillFactSchedulePreferenceMart(period,
                _jobParameters.DefaultTimeZone, RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}
