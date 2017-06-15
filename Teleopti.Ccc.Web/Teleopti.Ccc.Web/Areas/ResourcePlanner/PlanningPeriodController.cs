using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules,
		DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class PlanningPeriodController : ApiController
	{
		private readonly INextPlanningPeriodProvider _nextPlanningPeriodProvider;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly INow _now;
		private readonly SchedulingValidator _basicSchedulingValidator;
		private readonly IPlanningGroupRepository _planningGroupRepository;

		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider,
			IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader, INow now,
			SchedulingValidator basicSchedulingValidator, IPlanningGroupRepository planningGroupRepository)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
			_now = now;
			_basicSchedulingValidator = basicSchedulingValidator;
			_planningGroupRepository = planningGroupRepository;
		}

		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/result")]
		public virtual IHttpActionResult JobResult(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest($"Invalid {nameof(planningPeriodId)}");
			var range = planningPeriod.Range;
			var lastJobResult = planningPeriod.GetLastSchedulingJob();
			if (lastJobResult != null && lastJobResult.FinishedOk)
				return Ok(new
				{
					PlanningPeriod = new
					{
						StartDate = range.StartDate.Date,
						EndDate = range.EndDate.Date
					},
					ScheduleResult = JsonConvert.DeserializeObject<SchedulingResultModel>(lastJobResult.Details.First().Message),
					OptimizationResult = JsonConvert.DeserializeObject<OptimizationResultModel>(lastJobResult.Details.Last().Message),
				});
			return Ok(new { });
		}

		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/status")]
		public virtual IHttpActionResult JobStatus(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest($"Invalid {nameof(planningPeriodId)}");
			var lastJobResult = planningPeriod.GetLastSchedulingJob();
			if (lastJobResult != null)
				return Ok(new
				{
					HasJob = true,
					CurrentStep = lastJobResult.Details.Count(),
					TotalSteps = 2,
					Successful = lastJobResult.FinishedOk,
					Failed = lastJobResult.HasError()
				});
			return Ok(new { HasJob = false });
		}

		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/intradaystatus")]
		public virtual IHttpActionResult IntradayOptimizationJobStatus(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest($"Invalid {nameof(planningPeriodId)}");
			var lastJobResult = planningPeriod.GetLastIntradayOptimizationJob();
			if (lastJobResult != null)
				return Ok(new
				{
					HasJob = true,
					Successful = lastJobResult.FinishedOk,
					Failed = lastJobResult.HasError()
				});
			return Ok(new { HasJob = false });
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planninggroup/{planningGroupId}/planningperiods")]
		public virtual IHttpActionResult GetAllPlanningPeriods(Guid planningGroupId)
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");
			var allPlanningPeriods = _planningPeriodRepository.LoadForPlanningGroup(planningGroup).ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false, planningGroup);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods()
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var allPlanningPeriods = _planningPeriodRepository.LoadAll()
				.Where(planningPeriod => planningPeriod.PlanningGroup == null)
				.ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, true);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods(DateTime startDate, DateTime endDate)
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var period = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			var allPlanningPeriods = _planningPeriodRepository.LoadAll()
				.Where(planningPeriod => period.Intersection(planningPeriod.Range) != null)
				.Where(planningPeriod => planningPeriod.PlanningGroup == null)
				.ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}", Name = "GetPlanningPeriod")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			return Ok(createPlanningPeriodModel(planningPeriod));
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}/suggestions")]
		public virtual IHttpActionResult GetPlanningPeriodSuggestion(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var suggestion = getSuggestion(null);
			var result = suggestion.SuggestedPeriods(planningPeriod.Range);
			return Ok(result.Select(r => new SuggestedPlanningPeriodRangeModel
			{
				PeriodType = r.PeriodType.ToString(),
				StartDate = r.Range.StartDate.Date,
				EndDate = r.Range.EndDate.Date,
				Number = r.Number
			}));
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/suggestions/{planningGroupId}")]
		public virtual IHttpActionResult GetPlanningPeriodSuggestionsForPlanningGroup(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");
			var suggestion = getSuggestion(planningGroup);
			var result = suggestion.SuggestedPeriods(new DateOnly(_now.UtcDateTime()));
			return Ok(result.Select(r => new SuggestedPlanningPeriodRangeModel
			{
				PeriodType = r.PeriodType.ToString(),
				StartDate = r.Range.StartDate.Date,
				EndDate = r.Range.EndDate.Date,
				Number = r.Number
			}));
		}

		private IPlanningPeriodSuggestions getSuggestion(IPlanningGroup planningGroup)
		{
			var period = new DateOnly(_now.UtcDateTime()).ToDateOnlyPeriod();
			var personIds = planningGroup != null
				? _planningGroupStaffLoader.LoadPersonIds(period, planningGroup)
				: _planningGroupStaffLoader.Load(period, null)
					.FixedStaffPeople.Select(x => x.Id.GetValueOrDefault())
					.ToList();
			var suggestion = _planningPeriodRepository.Suggestions(_now, personIds);
			return suggestion;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}/validation")] 
		public virtual IHttpActionResult GetValidation(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var validationResult = new ValidationResult();
			if (planningPeriod.PlanningGroup != null)
			{

				validationResult = _basicSchedulingValidator.Validate(
					_planningGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.PlanningGroup).AllPeople.ToList(),
					planningPeriod.Range, true);
			}
			return Ok(validationResult);
		}

		[UnitOfWork, HttpPut, Route("api/resourceplanner/planningperiod/{planningPeriodId}")]
		public virtual IHttpActionResult ChangeRange(Guid planningPeriodId, [FromBody] PlanningPeriodChangeRangeModel model)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest($"Invalid {nameof(planningPeriodId)}");
			planningPeriod.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = model.Number,
				PeriodType = model.PeriodType,
				StartDate = new DateOnly(model.DateFrom),
			});
			return Ok(createPlanningPeriodModel(planningPeriod));
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/nextplanningperiod")]
		public virtual IHttpActionResult GetNextPlanningPeriod()
		{
			return nextPlanningPeriod(null);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planninggroup/{planningGroupId}/firstplanningperiod")]
		public virtual IHttpActionResult CreateFirstPlanningPeriod(Guid planningGroupId, DateTime endDate, DateTime startDate)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");

			var firstPeriod = new PlanningPeriod(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)), planningGroup);
			_planningPeriodRepository.Add(firstPeriod);
			
			return planningPeriodCreatedResponse(firstPeriod);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planninggroup/{planningGroupId}/nextplanningperiod")]
		public virtual IHttpActionResult GetNextPlanningPeriod(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");
			return nextPlanningPeriod(planningGroup);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/publish")]
		public virtual IHttpActionResult Publish(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest($"Invalid {nameof(planningPeriodId)}");
			planningPeriod.Publish(_planningGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.PlanningGroup).FixedStaffPeople.ToArray());
			return Ok();
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/planninggroup/{planningGroupId}/lastperiod")]
		public virtual IHttpActionResult DeleteLastPeriod(Guid planningGroupId)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");
			var planningPeriods = _planningPeriodRepository.LoadForPlanningGroup(planningGroup).ToList();
			if (planningPeriods.Count == 1)
				return BadRequest("Cannot delete the last period.");
			var periodToDelete = planningPeriods.OrderBy(x => x.Range.StartDate).LastOrDefault();
			if (periodToDelete != null)
			{
				_planningPeriodRepository.Remove(periodToDelete);
				planningPeriods.Remove(periodToDelete);
			}
			return buildPlanningPeriodViewModels(planningPeriods, new List<PlanningPeriodModel>(), false, planningGroup);
		}

		[UnitOfWork, HttpPut, Route("api/resourceplanner/planninggroup/{planningGroupId}/lastperiod")]
		public virtual IHttpActionResult ChangeLastPeriod(Guid planningGroupId, DateTime endDate, DateTime? startDate=null)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");
			var planningPeriods = _planningPeriodRepository.LoadForPlanningGroup(planningGroup).ToList();
			if (startDate.HasValue && planningPeriods.Count > 1)
				return BadRequest($"You are only allowed to change {nameof(startDate)} for first period.");
			var periodToChange = planningPeriods.OrderBy(x => x.Range.StartDate).LastOrDefault();
			if (periodToChange != null)
			{
				var newStartDate = startDate ?? periodToChange.Range.StartDate.Date;
				var periodDays = (int) (endDate - newStartDate).TotalDays + 1;
				if (periodDays <= 0)
					return BadRequest($"Invalid {nameof(endDate)}");
				periodToChange.ChangeRange(new SchedulePeriodForRangeCalculation
				{
					PeriodType = SchedulePeriodType.Day,
					StartDate = new DateOnly(newStartDate),
					Number = periodDays
				}, true);
				periodToChange.Reset();
			}
			return buildPlanningPeriodViewModels(planningPeriods, new List<PlanningPeriodModel>(), false, planningGroup);
		}

		private IHttpActionResult nextPlanningPeriod(IPlanningGroup planningGroup)
		{
			var allPlanningPeriods = _planningPeriodRepository.LoadAll().Where(p => p.PlanningGroup == planningGroup).ToList();
			var last = allPlanningPeriods.OrderByDescending(p => p.Range.StartDate).FirstOrDefault();
			var nextPeriod = last == null
				? new PlanningPeriod(getSuggestion(planningGroup), planningGroup)
				: last.NextPlanningPeriod(planningGroup);
			_planningPeriodRepository.Add(nextPeriod);

			return planningPeriodCreatedResponse(nextPeriod);
		}

		private IHttpActionResult buildPlanningPeriodViewModels(
			IList<IPlanningPeriod> allPlanningPeriods,
			List<PlanningPeriodModel> availablePlanningPeriods,
			bool createDefaultPlanningPeriod,
			IPlanningGroup planningGroup = null)
		{
			if (!allPlanningPeriods.Any() && createDefaultPlanningPeriod)
			{
				var planningPeriod = _nextPlanningPeriodProvider.Current(planningGroup);
				availablePlanningPeriods.Add(createPlanningPeriodModel(planningPeriod));
			}
			allPlanningPeriods.ForEach(pp => availablePlanningPeriods.Add(createPlanningPeriodModel(pp)));
			return Ok(availablePlanningPeriods);
		}

		private PlanningPeriodModel createPlanningPeriodModel(IPlanningPeriod planningPeriod)
		{
			var lastScheduleJobResult = planningPeriod.GetLastSchedulingJob();
			var lastIntradayOptimizationResult = planningPeriod.GetLastIntradayOptimizationJob();
			var state = planningPeriod.State;
			var numberOfAgents = _planningGroupStaffLoader.NumberOfAgents(planningPeriod.Range, planningPeriod.PlanningGroup);
			if (lastIntradayOptimizationResult != null && lastIntradayOptimizationResult.HasError())
				state = PlanningPeriodState.IntradayOptimizationFailed;
			if (lastScheduleJobResult != null && lastScheduleJobResult.HasError())
				state = PlanningPeriodState.ScheduleFailed;
			return new PlanningPeriodModel
			{
				StartDate = planningPeriod.Range.StartDate.Date,
				EndDate = planningPeriod.Range.EndDate.Date,
				Id = planningPeriod.Id.GetValueOrDefault(),
				HasNextPlanningPeriod = hasNextPlanningPeriod(planningPeriod.Range.EndDate.AddDays(1)),
				State = state.ToString(),
				TotalAgents = numberOfAgents,
				PlanningGroupId = planningPeriod.PlanningGroup?.Id,
				Number = planningPeriod.Number,
				Type = planningPeriod.PeriodType.ToString()
			};
		}

		private IHttpActionResult planningPeriodCreatedResponse(IPlanningPeriod planningPeriod)
		{
			return CreatedAtRoute("GetPlanningPeriod", new { planningPeriodId = planningPeriod.Id.GetValueOrDefault() }, createPlanningPeriodModel(planningPeriod));
		}

		private bool hasNextPlanningPeriod(DateOnly dateOnly)
		{
			var allPlanningPeriods = _planningPeriodRepository.LoadAll();
			return allPlanningPeriods.Any(p => p.Range.StartDate >= dateOnly);
		}
	}
}
