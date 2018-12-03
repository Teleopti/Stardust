using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactRequestJobStep : JobStepBase
	{
		public FactRequestJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_request";
			JobCategory = JobCategoryType.Schedule;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			return _jobParameters.Helper.Repository.FillFactRequestMart(new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling), RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}