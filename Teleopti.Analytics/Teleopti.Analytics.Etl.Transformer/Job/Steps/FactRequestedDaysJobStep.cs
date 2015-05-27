using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class FactRequestedDaysJobStep : JobStepBase
    {
		private readonly bool _isIntraday;

        public FactRequestedDaysJobStep(IJobParameters jobParameters) : this(jobParameters, false) { }
		public FactRequestedDaysJobStep(IJobParameters jobParameters, bool isIntraday) : base(jobParameters)

        {
			_isIntraday = isIntraday;
            Name = "fact_requested_days";
            JobCategory = JobCategoryType.Schedule;
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
			if (_isIntraday)
			{
				var rows = _jobParameters.Helper.Repository.FillIntradayFactRequestedDaysMart(RaptorTransformerHelper.CurrentBusinessUnit);
				_jobParameters.StateHolder.UpdateThisTime("Requests", RaptorTransformerHelper.CurrentBusinessUnit);
				return rows;
			}
			else
				return _jobParameters.Helper.Repository.FillFactRequestedDaysMart(new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling), RaptorTransformerHelper.CurrentBusinessUnit);
        }
    }
}