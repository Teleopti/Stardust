using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactScheduleForecastSkillJobStep : JobStepBase
	{
		public FactScheduleForecastSkillJobStep(IJobParameters jobParameters)
			: base(jobParameters)
		{
			Name = "fact_schedule_forecast_skill";
			JobCategory = JobCategoryType.Schedule;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			//Load data from stage to datamart
			return
				 _jobParameters.Helper.Repository.FillScheduleForecastSkillDataMart(
					  new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling), RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}