using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactScheduleDayCountJobStep : JobStepBase
    {
	    private readonly INeedToRunChecker _needToRunChecker;

	    public FactScheduleDayCountJobStep(IJobParameters jobParameters)
			:this(jobParameters, new DefaultNeedToRunChecker())
		{}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ToRun")]
		public FactScheduleDayCountJobStep(IJobParameters jobParameters, INeedToRunChecker needToRunChecker)
            : base(jobParameters)
        {
	        _needToRunChecker = needToRunChecker;
	        Name = "fact_schedule_day_count";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
			if (!_needToRunChecker.NeedToRun(period, _jobParameters.Helper.Repository, RaptorTransformerHelper.CurrentBusinessUnit, Name))
				return 0;
            //Load datamart
            return
				_jobParameters.Helper.Repository.FillScheduleDayCountDataMart(period,
                    RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}