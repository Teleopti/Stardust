using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSchedules, DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class PlanningPeriodController : ApiController
	{
		private readonly INextPlanningPeriodProvider _nextPlanningPeriodProvider;
		private readonly IPlanningPeriodRepository _planningPeriodRespository;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;
		private readonly INow _now;
		private readonly IBasicSchedulingValidator _basicSchedulingValidator;
		private readonly IAgentGroupRepository _agentGroupRepository;

		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider, IPlanningPeriodRepository planningPeriodRespository, IAgentGroupStaffLoader agentGroupStaffLoader, INow now, IBasicSchedulingValidator basicSchedulingValidator, IAgentGroupRepository agentGroupRepository)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_planningPeriodRespository = planningPeriodRespository;
			_agentGroupStaffLoader = agentGroupStaffLoader;
			_now = now;
			_basicSchedulingValidator = basicSchedulingValidator;
			_agentGroupRepository = agentGroupRepository;
		}

		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/result")]
		public virtual IHttpActionResult JobResult(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRespository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			var range = planningPeriod.Range;
			var lastJobResult = planningPeriod.JobResults
				.Where(x => x.JobCategory == JobCategory.WebSchedule && x.FinishedOk)
				.OrderByDescending(x => x.Timestamp)
				.FirstOrDefault();

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
			return Ok(new {});
		}


		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/status")]
		public virtual IHttpActionResult JobStatus(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRespository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			var lastJobResult = planningPeriod.JobResults
				.Where(x => x.JobCategory == JobCategory.WebSchedule)
				.OrderByDescending(x => x.Timestamp)
				.FirstOrDefault();
			if (lastJobResult != null)
				return Ok(new
				{
					HasJob = true,
					CurrentStep = lastJobResult.Details.Count(),
					TotalSteps = 2,
					Successful = lastJobResult.FinishedOk,
					Failed = lastJobResult.Details.Any(x => x.DetailLevel == DetailLevel.Error)
				});
			return Ok(new {HasJob = false});
		}

		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/intradaystatus")]
		public virtual IHttpActionResult IntradayOptimizationJobStatus(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRespository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			var lastJobResult = planningPeriod.JobResults.Where(x => x.JobCategory == JobCategory.WebIntradayOptimiztion).OrderByDescending(x => x.Timestamp).FirstOrDefault();
			if (lastJobResult != null)
				return Ok(new
				{
					HasJob = true,
					Successful = lastJobResult.FinishedOk,
					Failed = lastJobResult.Details.Any(x => x.DetailLevel == DetailLevel.Error)
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
			var allPlanningPeriods = _planningPeriodRespository.LoadForAgentGroup(agentGroup).ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, true, agentGroup);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods()
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var allPlanningPeriods = _planningPeriodRespository.LoadAll()
				.Where(planningPeriod => planningPeriod.AgentGroup == null)
				.ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, true);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod")]
		public virtual IHttpActionResult GetAllPlanningPeriods(DateTime startDate, DateTime endDate)
		{
			var availablePlanningPeriods = new List<PlanningPeriodModel>();
			var period = new DateOnlyPeriod (new DateOnly (startDate), new DateOnly (endDate));
			var allPlanningPeriods = _planningPeriodRespository.LoadAll()
				.Where(planningPeriod => period.Intersection(planningPeriod.Range) != null)
				.Where(planningPeriod => planningPeriod.AgentGroup == null)
				.ToList();
			return buildPlanningPeriodViewModels(allPlanningPeriods, availablePlanningPeriods, false);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRespository.Load(planningPeriodId);
			var validationResult = new ValidationResult();
			if (planningPeriod.AgentGroup != null)
			{
				validationResult = _basicSchedulingValidator.Validate(new ValidationParameters
				{
					Period = planningPeriod.Range,
					People = _agentGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.AgentGroup).AllPeople.ToList()
				});
			}
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), planningPeriod.State, validationResult, planningPeriod.AgentGroup);
			return Ok(planningPeriodModel);
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}/suggestions")]
		public virtual IHttpActionResult GetPlanningPeriodSuggestion(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRespository.Load(planningPeriodId);
			var suggestion = _planningPeriodRespository.Suggestions(_now);
			var result = suggestion.SuggestedPeriods(planningPeriod.Range);
			return Ok(result.Select(r => new SuggestedPlanningPeriodRangeModel
			{
				PeriodType = r.PeriodType.ToString(),
				StartDate = r.Range.StartDate.Date,
				EndDate = r.Range.EndDate.Date,
				Number = r.Number
			}));
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}/countagents")]
		public virtual IHttpActionResult GetAgentCount(Guid planningPeriodId, DateTime startDate, DateTime endDate)
		{
			var planningPeriod = _planningPeriodRespository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			var period = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));
			var numberOfAgents = _agentGroupStaffLoader.NumberOfAgents(period, planningPeriod.AgentGroup);
			return Ok(new
			{
				TotalAgents=numberOfAgents
			});
		}

		[UnitOfWork, HttpPut, Route("api/resourceplanner/planningperiod/{planningPeriodId}")]
		public virtual IHttpActionResult ChangeRange(Guid planningPeriodId, [FromBody] PlanningPeriodChangeRangeModel model)
		{
			var planningPeriod = _planningPeriodRespository.Get(planningPeriodId);
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
			var planningPeriodModel = createPlanningPeriodModel(planningPeriod.Range, planningPeriod.Id.GetValueOrDefault(), planningPeriod.State, validationResults, planningPeriod.AgentGroup);
			return Ok(planningPeriodModel);
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
			var planningPeriod = _planningPeriodRespository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest("Invalid planningPeriodId");
			planningPeriod.Publish(_agentGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.AgentGroup).FixedStaffPeople.ToArray());
			return Ok();
		}

		private IHttpActionResult nextPlanningPeriod(IAgentGroup agentGroup)
		{
			var allPlanningPeriods = _planningPeriodRespository.LoadAll().Where(p => p.AgentGroup == agentGroup).ToList();
			var last = allPlanningPeriods.OrderByDescending(p => p.Range.StartDate).FirstOrDefault();

			if (last == null)
			{
				var suggestion = _planningPeriodRespository.Suggestions(_now);
				last = new PlanningPeriod(suggestion, agentGroup);
				_planningPeriodRespository.Add(last);
			}
			var nextPeriod = last.NextPlanningPeriod(agentGroup);
			_planningPeriodRespository.Add(nextPeriod);

			var id = nextPeriod.Id.GetValueOrDefault();
			var validationResults = _basicSchedulingValidator.Validate(new ValidationParameters
			{
				Period = nextPeriod.Range,
				People = _agentGroupStaffLoader.Load(nextPeriod.Range, agentGroup).AllPeople.ToList()
			});
			
			return Created($"{Request.RequestUri}/{id}", createPlanningPeriodModel(nextPeriod.Range, id, PlanningPeriodState.New, validationResults, agentGroup));
		}


		private IHttpActionResult buildPlanningPeriodViewModels(
			IList<IPlanningPeriod> allPlanningPeriods, 
			List<PlanningPeriodModel> availablePlanningPeriods,
			bool createDefaultPlanningPeriod, 
			IAgentGroup agentGroup=null)
		{
			if (!allPlanningPeriods.Any())
			{
				if (!createDefaultPlanningPeriod) return Ok(availablePlanningPeriods);
				var planningPeriod = _nextPlanningPeriodProvider.Current(agentGroup);
				availablePlanningPeriods.Add(createPlanningPeriodModel(planningPeriod.Range,
					planningPeriod.Id.GetValueOrDefault(), planningPeriod.State, null, planningPeriod.AgentGroup));
			}
			else
			{
				allPlanningPeriods.ForEach(
					pp =>
						availablePlanningPeriods.Add(createPlanningPeriodModel(pp.Range, pp.Id.GetValueOrDefault(), pp.State, null, pp.AgentGroup)));
			}
			return Ok(availablePlanningPeriods);
		}

		private PlanningPeriodModel createPlanningPeriodModel(DateOnlyPeriod range, Guid id, PlanningPeriodState state, ValidationResult validationResult, IAgentGroup agentGroup)
		{
			return new PlanningPeriodModel
			{
				StartDate = range.StartDate.Date,
				EndDate = range.EndDate.Date,
				Id = id,
				HasNextPlanningPeriod = hasNextPlanningPeriod(range.EndDate.AddDays(1)),
				State = state.ToString(),
				ValidationResult = validationResult,
				AgentGroupId = agentGroup?.Id
			};
		}

		private bool hasNextPlanningPeriod(DateOnly dateOnly)
		{
			var allPlanningPeriods = _planningPeriodRespository.LoadAll();
			return allPlanningPeriods.Any(p => p.Range.StartDate >= dateOnly);
		}
	}
}
