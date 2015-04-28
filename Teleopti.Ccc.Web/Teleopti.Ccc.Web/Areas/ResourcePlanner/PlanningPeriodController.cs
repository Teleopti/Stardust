using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Scheduling;
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
			var planningPeriodModel = new PlanningPeriodModel
			{
				StartDate = planningPeriod.Range.StartDate.Date,
				EndDate = planningPeriod.Range.EndDate.Date,
				Id = planningPeriod.Id.GetValueOrDefault(),
				Skills = getMissingForecast(planningPeriod.Range),
				SuggestedPeriods = _nextPlanningPeriodProvider.SuggestedPeriods().Select(x=>x.ToString())
			};
			return
				Ok(planningPeriodModel);
		}

		//[UnitOfWork, HttpPost, Route("api/resourceplanner/updateplanningperiod")]
		//public virtual IHttpActionResult UpdatePlanningPeriod([FromBody] PlanningPeriodModel model)
		//{
		//	var planningPeriod = _nextPlanningPeriodProvider.Find(model.Id);
		//	//planningPeriod.Range = new DateOnlyPeriod(new DateOnly(model.StartDate),new DateOnly(model.EndDate));
		//	return Created(Request.RequestUri , new { });
		//}

		private IEnumerable<MissingForecastModel> getMissingForecast(DateOnlyPeriod planningPeriodRange)
		{
			return _missingForecastProvider.GetMissingForecast(planningPeriodRange);
		}
	}
}