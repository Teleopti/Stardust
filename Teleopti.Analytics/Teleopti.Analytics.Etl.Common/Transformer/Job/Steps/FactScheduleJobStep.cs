using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactScheduleJobStep : JobStepBase
	{
		public FactScheduleJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_schedule";
			JobCategory = JobCategoryType.Schedule;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Load data from stage to datamart 
			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);
			return
			_jobParameters.Helper.Repository.FillScheduleDataMart(period, RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}