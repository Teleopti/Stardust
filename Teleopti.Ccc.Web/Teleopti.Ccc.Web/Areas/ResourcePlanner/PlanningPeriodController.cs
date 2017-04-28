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
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.ResourcePlanner.Validation;
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
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;
		private readonly INow _now;
		private readonly IBasicSchedulingValidator _basicSchedulingValidator;
		private readonly IAgentGroupRepository _agentGroupRepository;

		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider,
			IPlanningPeriodRepository planningPeriodRepository, IAgentGroupStaffLoader agentGroupStaffLoader, INow now,
			IBasicSchedulingValidator basicSchedulingValidator, IAgentGroupRepository agentGroupRepository)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_planningPeriodRepository = planningPeriodRepository;
			_agentGroupStaffLoader = agentGroupStaffLoader;
			_now = now;
			_basicSchedulingValidator = basicSchedulingValidator;
			_agentGroupRepository = agentGroupRepository;
		}

		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/result")]
		public virtual IHttpActionResult JobResult(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			var range = planningPeriod.Range;
			var lastJobResult = planningPeriod.GetLastSchedulingJob();
			if (lastJobResult != null)
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
				return BadRequest("Invalid planningPeriodId");
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
				return BadRequest("Invalid planningPeriodId");
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

		[UnitOfWork, HttpGet, Route("api/resourceplanner/agentgroup/{agentGroupId}/planningperiods")]
		public virtual IHttpActionResult GetAllPlanningPeriods(Guid agentGroupId)
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			if (agentGroup == null)
				return BadRequest("Invalid agentGroupId");
			var allPlanningPeriods = _planningPeriodRepository.LoadForAgentGroup(agentGroup).ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false, agentGroup);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods()
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var allPlanningPeriods = _planningPeriodRepository.LoadAll()
				.Where(planningPeriod => planningPeriod.AgentGroup == null)
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
				.Where(planningPeriod => planningPeriod.AgentGroup == null)
				.ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var validationResult = new ValidationResult();
			if (planningPeriod.AgentGroup != null)
			{
				validationResult = _basicSchedulingValidator.Validate(new ValidationParameters
				{
					Period = planningPeriod.Range,
					People = _agentGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.AgentGroup).AllPeople.ToList()
				});
			}
			return Ok(createPlanningPeriodModel(planningPeriod, validationResult));
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

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/suggestions/{agentGroupId}")]
		public virtual IHttpActionResult GetPlanningPeriodSuggestionsForAgentGroup(Guid agentGroupId)
		{
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			if (agentGroup == null)
				return BadRequest($"Invalid {nameof(agentGroupId)}");
			var suggestion = getSuggestion(agentGroup);
			var planningPeriod = new PlanningPeriod(suggestion, agentGroup);
			var result = suggestion.SuggestedPeriods(planningPeriod.Range);
			return Ok(result.Select(r => new SuggestedPlanningPeriodRangeModel
			{
				PeriodType = r.PeriodType.ToString(),
				StartDate = r.Range.StartDate.Date,
				EndDate = r.Range.EndDate.Date,
				Number = r.Number
			}));
		}

		private IPlanningPeriodSuggestions getSuggestion(IAgentGroup agentGroup)
		{
			var period = new DateOnly(_now.UtcDateTime()).ToDateOnlyPeriod();
			var suggestion = _planningPeriodRepository.Suggestions(_now,
				_agentGroupStaffLoader.Load(period, agentGroup)
					.FixedStaffPeople.Select(x => x.Id.GetValueOrDefault())
					.ToList());
			return suggestion;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}/countagents")]
		public virtual IHttpActionResult GetAgentCount(Guid planningPeriodId, DateTime startDate, DateTime endDate)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			var period = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			var numberOfAgents = _agentGroupStaffLoader.NumberOfAgents(period, planningPeriod.AgentGroup);
			return Ok(new
			{
				TotalAgents = numberOfAgents
			});
		}

		[UnitOfWork, HttpPut, Route("api/resourceplanner/planningperiod/{planningPeriodId}")]
		public virtual IHttpActionResult ChangeRange(Guid planningPeriodId, [FromBody] PlanningPeriodChangeRangeModel model)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			planningPeriod.ChangeRange(new SchedulePeriodForRangeCalculation
			{
				Number = model.Number,
				PeriodType = model.PeriodType,
				StartDate = new DateOnly(model.DateFrom)
			});
			var validationResults = _basicSchedulingValidator.Validate(new ValidationParameters
			{
				Period = planningPeriod.Range,
				People = _agentGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.AgentGroup).AllPeople.ToList()
			});
			return Ok(createPlanningPeriodModel(planningPeriod, validationResults));
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/nextplanningperiod")]
		public virtual IHttpActionResult GetNextPlanningPeriod()
		{
			return nextPlanningPeriod(null);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/agentgroup/{agentGroupId}/nextplanningperiod")]
		public virtual IHttpActionResult GetNextPlanningPeriod(Guid agentGroupId)
		{
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			if (agentGroup == null)
				return BadRequest("Invalid agentGroupId");
			return nextPlanningPeriod(agentGroup);
		}

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planningperiod/{planningPeriodId}/publish")]
		public virtual IHttpActionResult Publish(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			planningPeriod.Publish(_agentGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.AgentGroup).FixedStaffPeople.ToArray());
			return Ok();
		}

		[UnitOfWork, HttpDelete, Route("api/resourceplanner/agentgroup/{agentGroupId}/lastperiod")]
		public virtual IHttpActionResult DeleteLastPeriod(Guid agentGroupId)
		{
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			if (agentGroup == null)
				return BadRequest($"Invalid {nameof(agentGroupId)}");
			var planningPeriods = _planningPeriodRepository.LoadForAgentGroup(agentGroup).ToList();
			if (planningPeriods.Count == 1)
				return BadRequest("Cannot delete the last period.");
			var periodToDelete = planningPeriods.OrderBy(x => x.Range.StartDate).LastOrDefault();
			if (periodToDelete != null)
			{
				_planningPeriodRepository.Remove(periodToDelete);
				planningPeriods.Remove(periodToDelete);
			}
			return buildPlanningPeriodViewModels(planningPeriods, new List<PlanningPeriodModel>(), false, agentGroup);
		}

		[UnitOfWork, HttpPut, Route("api/resourceplanner/agentgroup/{agentGroupId}/lastperiod")]
		public virtual IHttpActionResult ChangeLastPeriod(Guid agentGroupId, DateTime endDate, DateTime? startDate=null)
		{
			var agentGroup = _agentGroupRepository.Get(agentGroupId);
			if (agentGroup == null)
				return BadRequest($"Invalid {nameof(agentGroupId)}");
			var planningPeriods = _planningPeriodRepository.LoadForAgentGroup(agentGroup).ToList();
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
				periodToChange.JobResults.Clear();
			}
			return buildPlanningPeriodViewModels(planningPeriods, new List<PlanningPeriodModel>(), false, agentGroup);
		}

		private IHttpActionResult nextPlanningPeriod(IAgentGroup agentGroup)
		{
			var allPlanningPeriods = _planningPeriodRepository.LoadAll().Where(p => p.AgentGroup == agentGroup).ToList();
			var last = allPlanningPeriods.OrderByDescending(p => p.Range.StartDate).FirstOrDefault();
			var nextPeriod = last == null
				? new PlanningPeriod(getSuggestion(agentGroup), agentGroup)
				: last.NextPlanningPeriod(agentGroup);
			_planningPeriodRepository.Add(nextPeriod);
			var validationResults = _basicSchedulingValidator.Validate(new ValidationParameters
			{
				Period = nextPeriod.Range,
				People = _agentGroupStaffLoader.Load(nextPeriod.Range, agentGroup).AllPeople.ToList()
			});

			return Created($"{Request.RequestUri}/{nextPeriod.Id.GetValueOrDefault()}", createPlanningPeriodModel(nextPeriod, validationResults));
		}

		private IHttpActionResult buildPlanningPeriodViewModels(
			IList<IPlanningPeriod> allPlanningPeriods,
			List<PlanningPeriodModel> availablePlanningPeriods,
			bool createDefaultPlanningPeriod,
			IAgentGroup agentGroup = null)
		{
			if (!allPlanningPeriods.Any() && createDefaultPlanningPeriod)
			{
				var planningPeriod = _nextPlanningPeriodProvider.Current(agentGroup);
				availablePlanningPeriods.Add(createPlanningPeriodModel(planningPeriod, null));
			}
			allPlanningPeriods.ForEach(pp => availablePlanningPeriods.Add(createPlanningPeriodModel(pp, null)));
			return Ok(availablePlanningPeriods);
		}

		private PlanningPeriodModel createPlanningPeriodModel(IPlanningPeriod planningPeriod, ValidationResult validationResult)
		{
			var lastJobResult = planningPeriod.GetLastSchedulingJob();
			return new PlanningPeriodModel
			{
				StartDate = planningPeriod.Range.StartDate.Date,
				EndDate = planningPeriod.Range.EndDate.Date,
				Id = planningPeriod.Id.GetValueOrDefault(),
				HasNextPlanningPeriod = hasNextPlanningPeriod(planningPeriod.Range.EndDate.AddDays(1)),
				State = (lastJobResult != null && lastJobResult.HasError() ? PlanningPeriodState.Failed : planningPeriod.State).ToString(),
				ValidationResult = validationResult,
				AgentGroupId = planningPeriod.AgentGroup?.Id
			};
		}

		private bool hasNextPlanningPeriod(DateOnly dateOnly)
		{
			var allPlanningPeriods = _planningPeriodRepository.LoadAll();
			return allPlanningPeriods.Any(p => p.Range.StartDate >= dateOnly);
		}
	}
}
