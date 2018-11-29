using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class WaitlistRequestHandler : IHandleEvent<ProcessWaitlistedRequestsEvent>, IRunOnStardust
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly ArrangeRequestsByProcessOrder _arrangeRequestsByProcessOrder;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly INow _now;
		private readonly SchedulePartModifyAndRollbackServiceWithoutStateHolder _rollbackService;
		private readonly IStardustJobFeedback _stardustJobFeedback;
		private readonly IAbsenceRequestSynchronousValidator _absenceRequestSynchronousValidator;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceCollectionService;
		private readonly IAlreadyAbsentValidator _alreadyAbsentValidator;
		private readonly WaitlistPreloadService _waitlistPreloadService;

		public WaitlistRequestHandler(ICurrentScenario currentScenario,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider,
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder,
			IResourceCalculation resourceCalculation,
			INow now,
			SchedulePartModifyAndRollbackServiceWithoutStateHolder rollbackService,
			IStardustJobFeedback stardustJobFeedback, 
			IAbsenceRequestSynchronousValidator absenceRequestSynchronousValidator,
			IScheduleDifferenceSaver scheduleDifferenceSaver,
			IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService,
			IAlreadyAbsentValidator alreadyAbsentValidator,
			WaitlistPreloadService waitlistPreloadService)
		{
			_currentScenario = currentScenario;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_arrangeRequestsByProcessOrder = arrangeRequestsByProcessOrder;
			_resourceCalculation = resourceCalculation;
			_now = now;
			_rollbackService = rollbackService;
			_stardustJobFeedback = stardustJobFeedback;
			_absenceRequestSynchronousValidator = absenceRequestSynchronousValidator;
			_scheduleDifferenceSaver = scheduleDifferenceSaver;
			_differenceCollectionService = differenceCollectionService;
			_alreadyAbsentValidator = alreadyAbsentValidator;
			_waitlistPreloadService = waitlistPreloadService;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(ProcessWaitlistedRequestsEvent @event)
		{
			var dataHolder = _waitlistPreloadService.PreloadData();
			if (!dataHolder.InitSuccess || dataHolder.AllRequests.IsEmpty())
			{
				return;
			}

			_stardustJobFeedback.SendProgress($"Starting to process {dataHolder.AllRequests.Count} waitlisted requests.");
			using (getContext(dataHolder.CombinationResources, dataHolder.Skills, false))
			{
				var requestsNotHandled = new List<IPersonRequest>(dataHolder.AllRequests);

				var requestsBeforeFiltering = requestsNotHandled.Count;
				var requestsWithShrinkage = new List<IPersonRequest>();
				var requestsWithoutShrinkage = new List<IPersonRequest>();

				foreach (var pRequest in requestsNotHandled)
				{
					var absenceRequest = pRequest.Request as IAbsenceRequest;
					var personAbsenceAccount = dataHolder.PersonAbsenceAccounts[pRequest.Person].Find(absenceRequest.Absence);
					var result = _absenceRequestSynchronousValidator.Validate(pRequest, dataHolder.PersonsSchedules[pRequest.Person], personAbsenceAccount);
					if (!result.IsValid)
					{
						pRequest.Deny(result.ValidationErrors, new PersonRequestCheckAuthorization(), null,
							PersonRequestDenyOption.AutoDeny | result.DenyOption.GetValueOrDefault(PersonRequestDenyOption.None));
						_stardustJobFeedback.SendProgress(Resources.ResourceManager.GetString(result.ValidationErrors));
						continue;
					}
					if (isRequestUsingShrinkage(pRequest))
						requestsWithShrinkage.Add(pRequest);
					else
						requestsWithoutShrinkage.Add(pRequest);
				}
				requestsWithShrinkage = getRequestsToApprove(dataHolder, requestsWithShrinkage, true);
				requestsWithoutShrinkage = getRequestsToApprove(dataHolder, requestsWithoutShrinkage, false);

				var mergedRequests = new List<IPersonRequest>(requestsWithShrinkage);
				mergedRequests.AddRange(requestsWithoutShrinkage);

				requestsNotHandled = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(mergedRequests).ToList();
				requestsNotHandled.AddRange(_arrangeRequestsByProcessOrder.GetRequestsSortedByDate(mergedRequests));
				_stardustJobFeedback.SendProgress($"Requests before filtering:{requestsBeforeFiltering} and after filtering:{requestsNotHandled.Count}.");

				while (!requestsNotHandled.IsEmpty())
				{
					var requestsToHandle = new List<IPersonRequest>();
					foreach (var pRequest in requestsNotHandled)
					{
						var requestPeriod = pRequest.Request.Period;
						var isOverlapping = requestsToHandle.Any(x => x.Request.Period.ContainsPart(requestPeriod));

						if (isOverlapping)
							continue;

						requestsToHandle.Add(pRequest);
					}
					requestsToHandle.ForEach(r => requestsNotHandled.Remove(r));
					
					processRequests(dataHolder, requestsToHandle);

					_stardustJobFeedback.SendProgress($"Processed {dataHolder.AllRequests.Count - requestsNotHandled.Count} of {dataHolder.AllRequests.Count} waitlisted requests.");
				}

				foreach (var range in dataHolder.PersonsSchedules.Values)
				{
					var diff = range.DifferenceSinceSnapshot(_differenceCollectionService);
					_scheduleDifferenceSaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
				}
				
				_stardustJobFeedback.SendProgress($"Finished processing {dataHolder.AllRequests.Count} waitlisted requests.");
			}
		}

		private void processRequests(WaitlistDataHolder dataHolder, List<IPersonRequest> requests)
		{
			var requestToHandleWithShrinkage = new List<IPersonRequest>();
			var requestToHandleWithoutShrinkage = new List<IPersonRequest>();
			foreach (var pRequest in requests)
			{
				var schedules = dataHolder.PersonsSchedules[pRequest.Person];
				var workflowControlSet = pRequest.Request.Person.WorkflowControlSet;
				var absenceReqThresh = workflowControlSet.AbsenceRequestExpiredThreshold.GetValueOrDefault();

				if (_alreadyAbsentValidator.Validate(pRequest.Request as IAbsenceRequest, dataHolder.PersonsSchedules[pRequest.Person]))
				{
					pRequest.Deny(nameof(Resources.RequestDenyReasonAlreadyAbsent), new PersonRequestCheckAuthorization(), null,
						PersonRequestDenyOption.AlreadyAbsence);
					_stardustJobFeedback.SendProgress($"Already absent for request {pRequest.Id.GetValueOrDefault()}");
					continue;
				}

				if (pRequest.Request.Period.StartDateTime < _now.UtcDateTime().AddMinutes(absenceReqThresh))
				{
					continue;
				}

				var period = pRequest.Request.Period.ChangeEndTime(TimeSpan.FromDays(1));
				var dateOnlyPeriod = period.ToDateOnlyPeriod(pRequest.Person.PermissionInformation.DefaultTimeZone());
				var requestScheduledDays = schedules.ScheduledDayCollection(dateOnlyPeriod).ToList();
				var absenceRequest = (IAbsenceRequest) pRequest.Request;
				var layer = new AbsenceLayer(absenceRequest.Absence, pRequest.Request.Period);
				var personAbsence = new PersonAbsence(pRequest.Person, _currentScenario.Current(), layer);
				requestScheduledDays.ForEach(s => s.Add(personAbsence));

				if (!_rollbackService.ModifyStrictly(requestScheduledDays, new NoScheduleTagSetter(), dataHolder.BusinessRules))
				{
					pRequest.Deny(nameof(Resources.RequestDenyReasonPersonAccount), new PersonRequestCheckAuthorization());
					continue;
				}
				
				if(isRequestUsingShrinkage(pRequest))
					requestToHandleWithShrinkage.Add(pRequest);
				else
					requestToHandleWithoutShrinkage.Add(pRequest);
			}

			var requestsToApprove = getRequestsToApprove(dataHolder, requestToHandleWithShrinkage, true);
			requestsToApprove.AddRange(getRequestsToApprove(dataHolder, requestToHandleWithoutShrinkage, false));

			_rollbackService.RollbackMinimumChecks();

			_stardustJobFeedback.SendProgress($"Approving {requestsToApprove.Count} requests");
			
			foreach (var request in requestsToApprove)
			{
				var rules = request.Approve(dataHolder.RequestApprovalService, new PersonRequestCheckAuthorization(), true);
				
				_rollbackService.ClearModificationCollection();

				if (!rules.IsEmpty())
				{
					_stardustJobFeedback.SendProgress(
						$"Request {request.Id.GetValueOrDefault()} not approved because of these errors. {string.Join(";", rules.Select(x => x.Message))}");
				}
			}
		}

		private bool isRequestUsingShrinkage(IPersonRequest pRequest)
		{
			var mergedPeriod = pRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)pRequest.Request);
			var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
			var staffingThresholdValidators = validators.OfType<StaffingThresholdValidator>().ToList();
			var useShrinkage =
				staffingThresholdValidators.Any(x => x.GetType() == typeof(StaffingThresholdWithShrinkageValidator));

			return useShrinkage;
		}

		private List<IPersonRequest> getRequestsToApprove(WaitlistDataHolder dataHolder, List<IPersonRequest> requestsToHandle, bool useShrinkage)
		{
			var requestToApprove = new List<IPersonRequest>();
			if (requestsToHandle.IsEmpty()) return requestToApprove;

			foreach (var skillStaffingInterval in dataHolder.SkillStaffingIntervals)
			{
				skillStaffingInterval.SetUseShrinkage(useShrinkage);
			}

			var periodToResourceCalculate = new DateTimePeriod(requestsToHandle.Min(r => r.Request.Period.StartDateTime), requestsToHandle.Max(r => r.Request.Period.EndDateTime));
			_resourceCalculation.ResourceCalculate(
				periodToResourceCalculate.ToDateOnlyPeriod(TimeZoneInfo.Utc).Inflate(1),
				dataHolder.ResCalcData,
				() => getContext(dataHolder.CombinationResources, dataHolder.Skills, true));
		
			foreach (var pRequest in requestsToHandle)
			{
				var schedules = dataHolder.PersonsSchedules[pRequest.Person];
				
				var dateOnlyPeriod =
					dataHolder.LoadSchedulesPeriodToCoverForMidnightShifts.ToDateOnlyPeriod(
						pRequest.Person.PermissionInformation.DefaultTimeZone());
				var requestPeriod = pRequest.Request.Period;
				var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);

				var mergedPeriod =
					pRequest.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest) pRequest.Request);
				var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod).ToList();
				var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);

				var validatedRequestStaffing = ValidateStaffing(dataHolder, scheduleDays, requestPeriod, validators, pRequest);
				
				if (validatedRequestStaffing.IsValid)
				{
					if (!autoGrant) continue;
					requestToApprove.Add(pRequest);
				}
			}
			return requestToApprove;
		}

		private static IValidatedRequest ValidateStaffing(WaitlistDataHolder dataHolder,
			IEnumerable<IScheduleDay> scheduleDays, DateTimePeriod requestPeriod, List<IAbsenceRequestValidator> validators, IPersonRequest pRequest)
		{
			var staffingThresholdValidators = validators.OfType<StaffingThresholdValidator>().ToList();
			//should not happen now but hängslen och livrem
			if (staffingThresholdValidators.IsEmpty()) return new ValidatedRequest{IsValid = false};
			var shiftPeriodList = new List<DateTimePeriod>();

			foreach (var day in scheduleDays)
			{
				var projection = day.ProjectionService().CreateProjection().FilterLayers(requestPeriod);
				if (!projection.Any())
				{
					continue;
				}
				shiftPeriodList.Add(new DateTimePeriod(projection.OriginalProjectionPeriod.Value.StartDateTime,
					projection.OriginalProjectionPeriod.Value.EndDateTime));
			}

			var skillStaffingIntervalsToValidate = new List<SkillStaffingInterval>();
			foreach (var projectionPeriod in shiftPeriodList)
			{
				skillStaffingIntervalsToValidate.AddRange(
					dataHolder.SkillStaffingIntervals.Where(
						x => x.StartDateTime >= projectionPeriod.StartDateTime && x.StartDateTime < projectionPeriod.EndDateTime));
			}

			return staffingThresholdValidators.FirstOrDefault()
				.ValidateLight((IAbsenceRequest) pRequest.Request, skillStaffingIntervalsToValidate);
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}
}