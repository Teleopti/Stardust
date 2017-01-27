using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules, DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class PlanningPeriodController : ApiController
	{
		private readonly INextPlanningPeriodProvider _nextPlanningPeriodProvider;
		private readonly IPlanningPeriodRepository _planningPeriodRespository;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly INow _now;
		private readonly IBasicSchedulingValidator _basicSchedulingValidator;

		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider, IPlanningPeriodRepository planningPeriodRespository, IFixedStaffLoader fixedStaffLoader, INow now, IBasicSchedulingValidator basicSchedulingValidator)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_planningPeriodRespository = planningPeriodRespository;
			_fixedStaffLoader = fixedStaffLoader;
			_now = now;
			_basicSchedulingValidator = basicSchedulingValidator;
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
			var allPlanningPeriods = _planningPeriodRespository.LoadAll().Where (planningPeriod => period.Intersection(planningPeriod.Range) != null);
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{id}")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid id)
		{
			var planningPeriod = _planningPeriodRespository.Load(id);
			var validationResults = _basicSchedulingValidator.Validate(new ValidationParameters
			{
				Period = planningPeriod.Range,
				People = _fixedStaffLoader.Load(planningPeriod.Range).AllPeople.ToList()
			});
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), planningPeriod.State, validationResults);
			return Ok(planningPeriodModel);
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
			var validationResults = _basicSchedulingValidator.Validate(new ValidationParameters
			{
				Period = planningPeriod.Range,
				People = _fixedStaffLoader.Load(planningPeriod.Range).AllPeople.ToList()
			});
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), planningPeriod.State, validationResults);
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
					var validationResults = _basicSchedulingValidator.Validate(new ValidationParameters
					{
						Period = planningPeriod.Range,
						People = _fixedStaffLoader.Load(planningPeriod.Range).AllPeople.ToList()
					});
					availablePlanningPeriods.Add (createPlanningPeriodModel (planningPeriod.Range,
						planningPeriod.Id.GetValueOrDefault(), planningPeriod.State, validationResults));
				}
			}
			else
			{
				allPlanningPeriods.ForEach(
					pp =>
						availablePlanningPeriods.Add(createPlanningPeriodModel(pp.Range, pp.Id.GetValueOrDefault(), pp.State, _basicSchedulingValidator.Validate(new ValidationParameters
							{
								Period = pp.Range,
								People = _fixedStaffLoader.Load(pp.Range).AllPeople.ToList()
							}))));
			}
			return Ok(availablePlanningPeriods);
		}

		private PlanningPeriodModel createPlanningPeriodModel(DateOnlyPeriod range, Guid id, PlanningPeriodState state, ValidationResult validationResult)
		{
			return new PlanningPeriodModel
			{
				StartDate = range.StartDate.Date,
				EndDate = range.EndDate.Date,
				Id = id,
				HasNextPlanningPeriod = hasNextPlanningPeriod(range.EndDate.AddDays(1)),
				State = state.ToString(),
				ValidationResult = validationResult
			};
		}

		private bool hasNextPlanningPeriod(DateOnly dateOnly)
		{
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			return allPlanningPeriods.Any(p => p.Range.StartDate >= dateOnly);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/publish")]
		public virtual IHttpActionResult Publish(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRespository.Load(planningPeriodId);
			planningPeriod.Publish(_fixedStaffLoader.Load(planningPeriod.Range).FixedStaffPeople.ToArray());
			return Ok();
		}
	}
}
