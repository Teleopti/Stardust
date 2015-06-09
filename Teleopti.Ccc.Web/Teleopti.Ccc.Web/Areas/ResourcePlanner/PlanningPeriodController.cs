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
		public virtual IHttpActionResult GetPlanningPeriod()
		{
			var planningPeriod = _nextPlanningPeriodProvider.Current();
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range.StartDate.Date,
				planningPeriod.Range.EndDate.Date, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range));
			return Ok(planningPeriodModel);
		}

		private IEnumerable<MissingForecastModel> getMissingForecast(DateOnlyPeriod planningPeriodRange)
		{
			return _missingForecastProvider.GetMissingForecast(planningPeriodRange);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{id}/suggestions")]
		public virtual IHttpActionResult GetPlanningPeriodSuggestion(Guid id)
		{
			var suggestion = _planningPeriodRespository.Suggestions(_now);
			var planningPeriod = _planningPeriodRespository.Load(id);
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

		[UnitOfWork, HttpPut, Route("api/resourceplanner/planningperiod/{id}")]
		public virtual IHttpActionResult ChangeRange(Guid id, [FromBody] PlanningPeriodChangeRangeModel model)
		{
			var planningPeriod = _planningPeriodRespository.Load(id);
			planningPeriod.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = model.Number,
				PeriodType = model.PeriodType,
				StartDate = new DateOnly(model.DateFrom)
			});
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range.StartDate.Date,
				planningPeriod.Range.EndDate.Date, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range));
			return Ok(planningPeriodModel);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/nextplanningperiod")]
		public virtual IHttpActionResult GetNextPlanningPeriod([FromBody] PlanningPeriodChangeRangeModel model)
		{
			var planningPeriod =
				_nextPlanningPeriodProvider.Next(new SchedulePeriodForRangeCalculation
				{
					Number = model.Number,
					PeriodType = model.PeriodType,
					StartDate = new DateOnly(model.DateFrom)
				});
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range.StartDate.Date,
				planningPeriod.Range.EndDate.Date, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range));
			return Ok(planningPeriodModel);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/nextsuggestions")]
		public virtual IHttpActionResult GetNextPlanningPeriodSuggestions()
		{
			var planningPeriod = _nextPlanningPeriodProvider.Current();
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			var next =
				allPlanningPeriods.Where(p => p.Range.StartDate >= planningPeriod.Range.EndDate.AddDays(1));
			if (!next.Any())
			{
				var suggestion = _planningPeriodRespository.Suggestions(_now);
				var result = suggestion.NextSuggestedPeriods(planningPeriod.Range);
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
			return NotFound();
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/availableplanningperiod")]
		public virtual IHttpActionResult GetAvailablePlanningPeriods()
		{
			// may be only show the previous 10 ? 
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			if (!allPlanningPeriods.Any())
			{
				var planningPeriod = _nextPlanningPeriodProvider.Current();
				availablePlanningPeriods.Add(createPlanningPeriodModel(planningPeriod.Range.StartDate.Date,
					planningPeriod.Range.EndDate.Date, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range)));
			}
			else
			{
				allPlanningPeriods.ForEach(
					pp =>
						availablePlanningPeriods.Add(createPlanningPeriodModel(pp.Range.StartDate.Date, pp.Range.EndDate.Date,
							pp.Id.GetValueOrDefault(), getMissingForecast(pp.Range))));
			}
			return Ok(availablePlanningPeriods.AsEnumerable());
		}

		private PlanningPeriodModel createPlanningPeriodModel(DateTime startDate, DateTime endDate, Guid id,
			IEnumerable<MissingForecastModel> skills)
		{
			return new PlanningPeriodModel
			{
				StartDate = startDate,
				EndDate = endDate,
				Id = id,
				Skills = skills
			};
		}
	}
}
