using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class PlanningPeriodController : ApiController
	{
		private readonly INextPlanningPeriodProvider _nextPlanningPeriodProvider;
		private readonly IMissingForecastProvider _missingForecastProvider;
		private readonly IPlanningPeriodRepository _planningPeriodRespository;
		private readonly INow _now;

		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider,
			IMissingForecastProvider missingForecastProvider, IPlanningPeriodRepository planningPeriodRespository, INow now)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_missingForecastProvider = missingForecastProvider;
			_planningPeriodRespository = planningPeriodRespository;
			_now = now;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods()
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			if (!allPlanningPeriods.Any())
			{
				var planningPeriod = _nextPlanningPeriodProvider.Current();
				availablePlanningPeriods.Add(createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range)));
			}
			else
			{
				allPlanningPeriods.ForEach(
					pp =>
						availablePlanningPeriods.Add(createPlanningPeriodModel(pp.Range, pp.Id.GetValueOrDefault(), getMissingForecast(pp.Range))));
			}
			return Ok(availablePlanningPeriods);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{id}")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid id)
		{
			var planningPeriod = _planningPeriodRespository.Load(id);
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range));
			return Ok(planningPeriodModel);
		}

		private IEnumerable<MissingForecastModel> getMissingForecast(DateOnlyPeriod planningPeriodRange)
		{
			return _missingForecastProvider.GetMissingForecast(planningPeriodRange);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{id}/suggestions")]
		public virtual IHttpActionResult GetPlanningPeriodSuggestion(Guid id)
		{
			var planningPeriod = _planningPeriodRespository.Load(id);
			var suggestion = _planningPeriodRespository.Suggestions(_now);
			var result = suggestion.SuggestedPeriods(planningPeriod.Range);
			return
				Ok(
					result.Select(
						r =>
							new SuggestedPlanningPeriodRangeModel
							{
								PeriodType = r.PeriodType.ToString(),
								StartDate = r.Range.StartDate.Date,
								EndDate = r.Range.EndDate.Date,
								Number = r.Number
							}));
		}

		[UnitOfWork, HttpPut, Route("api/resourceplanner/changeplanningperiod/{id}")]
		public virtual IHttpActionResult ChangeRange(Guid id, [FromBody] PlanningPeriodChangeRangeModel model)
		{
			var planningPeriod = _planningPeriodRespository.Load(id);
			planningPeriod.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = model.Number,
				PeriodType = model.PeriodType,
				StartDate = new DateOnly(model.DateFrom)
			});
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range));
			return Ok(planningPeriodModel);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/nextplanningperiod")]
		public virtual IHttpActionResult GetNextPlanningPeriod()
		{
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			var last = allPlanningPeriods.OrderByDescending(p => p.Range.StartDate).First().NextPlanningPeriod();
			
			_planningPeriodRespository.Add(last);

			var id = last.Id.GetValueOrDefault();
			return Created(Request.RequestUri + "/" + id, new {
				Id= id,
				EndDate = last.Range.EndDate.Date,
				StartDate = last.Range.StartDate.Date,
			});
		}

		private PlanningPeriodModel createPlanningPeriodModel(DateOnlyPeriod range, Guid id, IEnumerable<MissingForecastModel> skills)
		{
			return new PlanningPeriodModel
			{
				StartDate = range.StartDate.Date,
				EndDate = range.EndDate.Date,
				Id = id,
				HasNextPlanningPeriod = hasNextPlanningPeriod(range.EndDate.AddDays(1)),
				Skills = skills
			};
		}

		private bool hasNextPlanningPeriod(DateOnly dateOnly)
		{
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			return allPlanningPeriods.Any(p => p.Range.StartDate >= dateOnly);
		}
	}
}
