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
using Teleopti.Ccc.Domain.ResourcePlanner.Hints;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPlans,
		DefinedRaptorApplicationFunctionPaths.SeatPlanner)]
	public class PlanningPeriodController : ApiController
	{
		private readonly INextPlanningPeriodProvider _nextPlanningPeriodProvider;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly INow _now;
		private readonly IPlanningGroupRepository _planningGroupRepository;
		private readonly GetValidations _getValidatons;
		private readonly IUserTimeZone _userTimeZone;


		public PlanningPeriodController(INextPlanningPeriodProvider nextPlanningPeriodProvider,
			IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader, INow now,
			IPlanningGroupRepository planningGroupRepository, GetValidations getValidatons, IUserTimeZone userTimeZone)
		{
			_nextPlanningPeriodProvider = nextPlanningPeriodProvider;
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
			_now = now;
			_planningGroupRepository = planningGroupRepository;
			_getValidatons = getValidatons;
			_userTimeZone = userTimeZone;
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
			{
				var fullSchedulingResultModel = JsonConvert.DeserializeObject<FullSchedulingResultModel>(lastJobResult.Details.Last().Message);
				fullSchedulingResultModel.BusinessRulesValidationResults = HintsHelper.BuildBusinessRulesValidationResults(fullSchedulingResultModel.BusinessRulesValidationResults, _userTimeZone);
				return Ok(new
				{
					PlanningPeriod = new
					{
						StartDate = range.StartDate.Date,
						EndDate = range.EndDate.Date
					},
					FullSchedulingResult = fullSchedulingResultModel
				});
			}
			return Ok(new {});
		}


		[HttpGet, UnitOfWork, Route("api/resourceplanner/planningperiod/{planningPeriodId}/status")]
		public virtual IHttpActionResult JobStatus(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Get(planningPeriodId);
			if (planningPeriod == null)
				return BadRequest($"Invalid {nameof(planningPeriodId)}");
			var lastSchedulingJobResult = planningPeriod.GetLastSchedulingJob();
			dynamic schedulingStatus = new
			{
				HasJob = false
			};
			
			if (lastSchedulingJobResult != null)
			{
				schedulingStatus = new
				{
					HasJob = true,
					Successful = lastSchedulingJobResult.FinishedOk,
					Failed = lastSchedulingJobResult.HasError(),
					LastJobId = lastSchedulingJobResult.Id.Value
				};
			}
			
			var lastIntradayOptimizationJobResult = planningPeriod.GetLastIntradayOptimizationJob();
			dynamic intradayOptimizationStatus = new
			{
				HasJob = false
			};
			if (lastIntradayOptimizationJobResult != null)
			{
				intradayOptimizationStatus = new
				{
					HasJob = true,
					Successful = lastIntradayOptimizationJobResult.FinishedOk,
					Failed = lastIntradayOptimizationJobResult.HasError(),
					LastJobId = lastIntradayOptimizationJobResult.Id.Value
				};
			}

			var lastClearScheduleJobResult = planningPeriod.GetLastClearScheduleJob();
			dynamic clearScheduleStatus = new
			{
				HasJob = false
			};
			if (lastClearScheduleJobResult != null)
			{
				clearScheduleStatus = new
				{
					HasJob = true,
					Successful = lastClearScheduleJobResult.FinishedOk,
					Failed = lastClearScheduleJobResult.HasError(),
					LastJobId = lastClearScheduleJobResult.Id.Value
				};
			}

			var status = new
			{
				SchedulingStatus = schedulingStatus,
				IntradayOptimizationStatus = intradayOptimizationStatus,
				ClearScheduleStatus = clearScheduleStatus
			};
			
			return Ok(status);
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

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}", Name = "GetPlanningPeriod")]
		public virtual IHttpActionResult GetPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			return Ok(createPlanningPeriodModel(planningPeriod));
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

		private IPlanningPeriodSuggestions getSuggestion(PlanningGroup planningGroup)
		{
			var period = new DateOnly(_now.UtcDateTime()).ToDateOnlyPeriod();
			var personIds = _planningGroupStaffLoader.LoadPersonIds(period, planningGroup);
			var suggestion = _planningPeriodRepository.Suggestions(_now, personIds);
			return suggestion;
		}

		[UnitOfWork, HttpGet, Route("api/resourceplanner/planningperiod/{planningPeriodId}/validation")] 
		public virtual IHttpActionResult GetValidation(Guid planningPeriodId)
		{
			return Ok(_getValidatons.Execute(planningPeriodId));
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

		[UnitOfWork, HttpPost, Route("api/resourceplanner/planninggroup/{planningGroupId}/firstplanningperiod")]
		public virtual IHttpActionResult CreateFirstPlanningPeriod(Guid planningGroupId, DateTime startDate, SchedulePeriodType schedulePeriodType, int lengthOfThePeriodType)
		{
			var planningGroup = _planningGroupRepository.Get(planningGroupId);
			if (planningGroup == null)
				return BadRequest($"Invalid {nameof(planningGroupId)}");

			var firstPeriod = new PlanningPeriod(new DateOnly(startDate), schedulePeriodType, lengthOfThePeriodType, planningGroup);
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
		public virtual IHttpActionResult ChangeLastPeriod(Guid planningGroupId, DateTime endDate, SchedulePeriodType schedulePeriodType, int lengthOfThePeriodType, DateTime? startDate=null)
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
					PeriodType = schedulePeriodType,
					StartDate = new DateOnly(newStartDate),
					Number = lengthOfThePeriodType
				}, true);
				periodToChange.Reset();
			}
			return buildPlanningPeriodViewModels(planningPeriods, new List<PlanningPeriodModel>(), false, planningGroup);
		}

		private IHttpActionResult nextPlanningPeriod(PlanningGroup planningGroup)
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
			IList<PlanningPeriod> allPlanningPeriods,
			List<PlanningPeriodModel> availablePlanningPeriods,
			bool createDefaultPlanningPeriod,
			PlanningGroup planningGroup = null)
		{
			if (!allPlanningPeriods.Any() && createDefaultPlanningPeriod)
			{
				var planningPeriod = _nextPlanningPeriodProvider.Current(planningGroup);
				availablePlanningPeriods.Add(createPlanningPeriodModel(planningPeriod));
			}
			allPlanningPeriods.ForEach(pp => availablePlanningPeriods.Add(createPlanningPeriodModel(pp)));
			return Ok(availablePlanningPeriods);
		}

		private PlanningPeriodModel createPlanningPeriodModel(PlanningPeriod planningPeriod)
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

		private IHttpActionResult planningPeriodCreatedResponse(PlanningPeriod planningPeriod)
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

