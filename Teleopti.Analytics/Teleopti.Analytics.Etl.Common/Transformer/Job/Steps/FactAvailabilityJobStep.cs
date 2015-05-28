using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class FactAvailabilityJobStep : JobStepBase
	{
		private readonly bool _isIntraday;

		public FactAvailabilityJobStep(IJobParameters jobParameters) : this(jobParameters, false) { }
		public FactAvailabilityJobStep(IJobParameters jobParameters, bool isIntraday)
			: base(jobParameters)
		{
			Name = "fact_hourly_availability";
			JobCategory = JobCategoryType.Schedule;
			_isIntraday = isIntraday;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			if (_isIntraday)
				foreach (var scenario in _jobParameters.StateHolder.ScenarioCollectionDeletedExcluded.Where(scenario => scenario.DefaultScenario))
				{
					var rows = _jobParameters.Helper.Repository.FillIntradayFactAvailabilityMart(RaptorTransformerHelper.CurrentBusinessUnit, scenario);
					_jobParameters.StateHolder.UpdateThisTime("Availability", RaptorTransformerHelper.CurrentBusinessUnit);
					return rows;
				}


			var period = new DateTimePeriod(JobCategoryDatePeriod.StartDateUtcFloor, JobCategoryDatePeriod.EndDateUtcCeiling);

			//Load data from stage to datamart
			return
				 _jobParameters.Helper.Repository.FillFactAvailabilityMart(period,
				 _jobParameters.DefaultTimeZone, RaptorTransformerHelper.CurrentBusinessUnit);
		}
	}
}
