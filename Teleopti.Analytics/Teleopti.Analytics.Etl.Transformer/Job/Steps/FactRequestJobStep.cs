using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class FactRequestJobStep : JobStepBase
	{
		private readonly bool _isIntraday;

	    public FactRequestJobStep(IJobParameters jobParameters, bool isIntraday = false)
            : base(jobParameters)
		{
			_isIntraday = isIntraday; 
			Name = "fact_request";
			JobCategory = JobCategoryType.Schedule;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			if (_isIntraday)
				return _jobParameters.Helper.Repository.FillIntradayFactRequestMart(RaptorTransformerHelper.CurrentBusinessUnit);
			else
				return _jobParameters.Helper.Repository.FillFactRequestMart(new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling), RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}