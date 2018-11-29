using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactAvailabilityJobStep : JobStepBase
	{

		public FactAvailabilityJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_hourly_availability";
			JobCategory = JobCategoryType.Schedule;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);

			//Load data from stage to datamart
			return
				 _jobParameters.Helper.Repository.FillFactAvailabilityMart(period,
				 _jobParameters.DefaultTimeZone, RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}
