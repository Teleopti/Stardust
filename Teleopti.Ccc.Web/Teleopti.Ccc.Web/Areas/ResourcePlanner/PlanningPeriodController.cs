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
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider,
			IMissingForecastProvider missingForecastProvider, IPlanningPeriodRepository planningPeriodRespository, INow now, ICurrentBusinessUnit currentBusinessUnit)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_missingForecastProvider = missingForecastProvider;
			_planningPeriodRespository = planningPeriodRespository;
			_now = now;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods()
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, true);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiodsforrange")]
		public virtual IHttpActionResult GetAllPlanningPeriods(DateTime startDate, DateTime endDate)
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var period = new DateOnlyPeriod (new DateOnly (startDate), new DateOnly (endDate));
			var allPlanningPeriods = _planningPeriodRespository.LoadAll().Where (planningPeriod => (period.Intersection(planningPeriod.Range) != null));
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{id}")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid id)
		{
			var planningPeriod = _planningPeriodRespository.Load(id);
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range),planningPeriod.State);
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
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), getMissingForecast(planningPeriod.Range), planningPeriod.State);
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


		private IHttpActionResult buildPlanningPeriodViewModels(
					IEnumerable<IPlanningPeriod> allPlanningPeriods, 
					List<PlanningPeriodModel> availablePlanningPeriods, 
					bool createDefaultPlanningPeriod)
		{
			if (!allPlanningPeriods.Any())
			{
				if (createDefaultPlanningPeriod)
				{
					var planningPeriod = _nextPlanningPeriodProvider.Current();
					availablePlanningPeriods.Add (createPlanningPeriodModel (planningPeriod.Range,
						planningPeriod.Id.GetValueOrDefault(),
						getMissingForecast (planningPeriod.Range), planningPeriod.State));
				}
			}
			else
			{
				allPlanningPeriods.ForEach(
					pp =>
						availablePlanningPeriods.Add(createPlanningPeriodModel(pp.Range, pp.Id.GetValueOrDefault(),
							getMissingForecast(pp.Range), pp.State)));
			}
			return Ok(availablePlanningPeriods);
		}

		private PlanningPeriodModel createPlanningPeriodModel(DateOnlyPeriod range, Guid id, IEnumerable<MissingForecastModel> skills, PlanningPeriodState state)
		{
			return new PlanningPeriodModel
			{
				StartDate = range.StartDate.Date,
				EndDate = range.EndDate.Date,
				Id = id,
				HasNextPlanningPeriod = hasNextPlanningPeriod(range.EndDate.AddDays(1)),
				Skills = skills,
				State = state.ToString()
			};
		}

		private bool hasNextPlanningPeriod(DateOnly dateOnly)
		{
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			return allPlanningPeriods.Any(p => p.Range.StartDate >= dateOnly);
		}
	}
}
