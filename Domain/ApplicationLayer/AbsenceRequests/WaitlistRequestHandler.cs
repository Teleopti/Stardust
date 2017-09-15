using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class WaitlistRequestHandler : IHandleEvent<ProcessWaitlistedRequestsEvent>, IRunOnStardust
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(WaitlistRequestHandler));
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly SkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestSetting _absenceRequestSetting;
		private readonly ArrangeRequestsByProcessOrder _arrangeRequestsByProcessOrder;
		private readonly ExtractSkillForecastIntervals _extractSkillForecastIntervals;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private readonly SchedulePartModifyAndRollbackServiceWithoutStateHolder _rollbackService;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;

		public WaitlistRequestHandler(ICommandDispatcher commandDispatcher,
			ISkillCombinationResourceRepository skillCombinationResourceRepository,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			ISkillRepository skillRepository,
			SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider,
			IActivityRepository activityRepository, IPersonRequestRepository personRequestRepository,
			IAbsenceRequestSetting absenceRequestSetting,
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder,
			ExtractSkillForecastIntervals extractSkillForecastIntervals, IResourceCalculation resourceCalculation,
			ICurrentUnitOfWork currentUnitOfWork,
			INow now,
			SchedulePartModifyAndRollbackServiceWithoutStateHolder rollbackService,
			ISkillTypeRepository skillTypeRepository, IPersonRepository personRepository, IContractRepository contractRepository, IPartTimePercentageRepository partTimePercentageRepository,
			IContractScheduleRepository contractScheduleRepository)
		{
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_skillRepository = skillRepository;
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_activityRepository = activityRepository;
			_personRequestRepository = personRequestRepository;
			_absenceRequestSetting = absenceRequestSetting;
			_arrangeRequestsByProcessOrder = arrangeRequestsByProcessOrder;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_resourceCalculation = resourceCalculation;
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
			_rollbackService = rollbackService;
			_skillTypeRepository = skillTypeRepository;
			_personRepository = personRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractScheduleRepository = contractScheduleRepository;
		}

		[AsSystem]
		public virtual void Handle(ProcessWaitlistedRequestsEvent @event)
		{
			var requestIdsToSkip = new List<Guid>();
			var failedRequestIds = new List<Guid>();

			do
			{
				failedRequestIds = Internal(@event, requestIdsToSkip);
				requestIdsToSkip.AddRange(failedRequestIds);
			}
			while (!failedRequestIds.IsEmpty());
		}

		[UnitOfWork]
		public virtual List<Guid> Internal(ProcessWaitlistedRequestsEvent @event, List<Guid> requestIdsToSkip)
		{
			try
			{
				var failedRequestIds = new List<Guid>();
				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + "Read model is not up to date");
					return failedRequestIds;
				}
				var validPeriod = new DateTimePeriod(_now.UtcDateTime(), _now.UtcDateTime().AddHours(_absenceRequestSetting.ImmediatePeriodInHours));
				var loadSchedulesPeriodToCoverForMidnightShifts = validPeriod.ChangeStartTime(TimeSpan.FromDays(-1));
				var waitlistedRequestsIds = _personRequestRepository.GetWaitlistRequests(loadSchedulesPeriodToCoverForMidnightShifts).Except(requestIdsToSkip);
				var waitlistedRequests = _personRequestRepository.Find(waitlistedRequestsIds);
				
				waitlistedRequests =
					waitlistedRequests.Where(
						x =>
							x.Request.Period.StartDateTime >= validPeriod.StartDateTime &&
							x.Request.Period.EndDateTime <= validPeriod.EndDateTime).ToList();
				var allRequests = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(waitlistedRequests).ToList();
				allRequests.AddRange(_arrangeRequestsByProcessOrder.GetRequestsSortedByDate(waitlistedRequests));

				_personRepository.FindPeople(allRequests.Select(x => x.Person.Id.GetValueOrDefault()).ToList());

				var inflatedPeriod = new DateTimePeriod(allRequests.Min(x => x.Request.Period.StartDateTime), allRequests.Max(x => x.Request.Period.EndDateTime));

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(inflatedPeriod).ToList();
				if (!combinationResources.Any())
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any skillcombinations.");
					return failedRequestIds;
				}

				_activityRepository.LoadAll();
				_skillTypeRepository.LoadAll();
				_contractRepository.LoadAll();
				_partTimePercentageRepository.LoadAll();
				_contractScheduleRepository.LoadAllAggregate();

				var skills = _skillRepository.LoadAllSkills().ToList();
				
				var skillIds = new HashSet<Guid>();
				foreach (var skillCombinationResource in combinationResources)
				{
					foreach (var skillId in skillCombinationResource.SkillCombination)
					{
						skillIds.Add(skillId);
					}
				}
				var skillInterval = skills.Where(x => skillIds.Contains(x.Id.GetValueOrDefault())).Min(x => x.DefaultResolution);

				var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, inflatedPeriod, true).ToList();
				skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

				var relevantSkillStaffPeriods =
					skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
						.ToDictionary(k => k.Key,
							v =>
								(IResourceCalculationPeriodDictionary)
								new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
									s => (IResourceCalculationPeriod)s)));
				var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
				var persons = waitlistedRequests.Select(p => p.Person).ToList();
				var dateOnlyPeriodOne = ExtractSkillForecastIntervals.GetLongestPeriod(skills, inflatedPeriod);
				var personsSchedules =
					_scheduleStorage.FindSchedulesForPersons(
						new ScheduleDateTimePeriod(loadSchedulesPeriodToCoverForMidnightShifts, persons), _currentScenario.Current(),
						new PersonProvider(persons), new ScheduleDictionaryLoadOptions(false, false), persons);

				_currentUnitOfWork.Current().Clear();
				var approvedRequest = new List<IPersonRequest>();

				using (getContext(combinationResources, skills, false))
				{
					_resourceCalculation.ResourceCalculate(dateOnlyPeriodOne, resCalcData, () => getContext(combinationResources, skills, true));
					//consider the exception and which requests to deny
					foreach (var pRequest in allRequests)
					{
						var requestPeriod = pRequest.Request.Period;
						var schedules = personsSchedules[pRequest.Person];
						var dateOnlyPeriod = loadSchedulesPeriodToCoverForMidnightShifts.ToDateOnlyPeriod(pRequest.Person.PermissionInformation.DefaultTimeZone());
						var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);
						var startDateTimeInPersonTimeZone = TimeZoneHelper.ConvertFromUtc(pRequest.RequestedDate, pRequest.Person.PermissionInformation.DefaultTimeZone());
						var scheduleDay = schedules.ScheduledDay(new DateOnly(startDateTimeInPersonTimeZone));
						var personAssignment = scheduleDay.PersonAssignment();
						if (personAssignment == null) continue;

						var absenceRequest = (IAbsenceRequest)pRequest.Request;
						scheduleDay.CreateAndAddAbsence(new AbsenceLayer(absenceRequest.Absence, pRequest.Request.Period));
						//personsSchedules.Modify(scheduleDay, _scheduleDayChangeCallback);
						_rollbackService.ModifyStrictly(scheduleDay, new NoScheduleTagSetter(), NewBusinessRuleCollection.Minimum());
						_resourceCalculation.ResourceCalculate(dateOnlyPeriodOne, resCalcData, () => getContext(combinationResources, skills, true));

						var mergedPeriod = pRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)pRequest.Request);
						var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);

						var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);

						var shiftPeriodList = new List<DateTimePeriod>();

						foreach (var day in scheduleDays)
						{

							var projection = day.ProjectionService().CreateProjection().FilterLayers(requestPeriod);

							var layers = projection.ToResourceLayers(skillInterval).ToList();
							if (!layers.Any())
							{
								continue;
							}
							shiftPeriodList.Add(new DateTimePeriod(projection.OriginalProjectionPeriod.Value.StartDateTime, projection.OriginalProjectionPeriod.Value.EndDateTime));
						}

						var staffingThresholdValidators = validators.OfType<StaffingThresholdValidator>().ToList();
						if (staffingThresholdValidators.Any())
						{
							//TODO: how to handle shrinkage?
							//var useShrinkage = staffingThresholdValidators.Any(x => x.GetType() == typeof(StaffingThresholdWithShrinkageValidator));
							var skillStaffingIntervalsToValidate = new List<SkillStaffingInterval>();
							foreach (var projectionPeriod in shiftPeriodList)
							{
								skillStaffingIntervalsToValidate.AddRange(skillStaffingIntervals.Where(x => x.StartDateTime >= projectionPeriod.StartDateTime && x.StartDateTime < projectionPeriod.EndDateTime));
							}
							var validatedRequest = staffingThresholdValidators.FirstOrDefault().ValidateLight((IAbsenceRequest)pRequest.Request, skillStaffingIntervalsToValidate);
							if (validatedRequest.IsValid)
							{
								if (!autoGrant) continue;
								_rollbackService.ClearModificationCollection();
								approvedRequest.Add(pRequest);
							}
							else
							{
								_rollbackService.RollbackMinimumChecks();
							}
						}
						else
						{
							logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any staffingThresholdValidator.");
						}
					}

				}
				foreach (var personRequest in approvedRequest)
				{
					var result = sendApproveCommand(personRequest);
					if (!result)
					{
						failedRequestIds.Add(personRequest.Id.GetValueOrDefault());
					}
				}
				return failedRequestIds;
			}
			catch (Exception exp)
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + exp);
			}
			return new List<Guid>();
		}

		private bool sendApproveCommand(IPersonRequest personRequest)
		{
			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}

			return !command.ErrorMessages.Any();
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}
}