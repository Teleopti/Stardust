using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PlanningPeriodController : ApiController
	{
		private readonly INextPlanningPeriodProvider _nextPlanningPeriodProvider;
		private readonly IMissingForecastProvider _missingForecastProvider;
		
		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider, IMissingForecastProvider missingForecastProvider)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_missingForecastProvider = missingForecastProvider;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetPlanningPeriod()
		{
			var planningPeriod = _nextPlanningPeriodProvider.Current();
			
			return
				Ok(new PlanningPeriodModel
				{
					StartDate = planningPeriod.Range.StartDate.Date,
					EndDate = planningPeriod.Range.EndDate.Date,
					Id = planningPeriod.Id.GetValueOrDefault(),
					Skills = getMissingForecast(planningPeriod.Range)
				});
		}

		private IEnumerable<MissingForecastModel> getMissingForecast(DateOnlyPeriod planningPeriodRange)
		{
			return _missingForecastProvider.GetMissingForecast(planningPeriodRange);
		}
	}
}