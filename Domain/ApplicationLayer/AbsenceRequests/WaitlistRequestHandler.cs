using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		private readonly INow _now;
		private readonly SchedulePartModifyAndRollbackServiceWithoutStateHolder _rollbackService;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IContractRepository _contractRepository;
		private readonly IPartTimePercentageRepository _partTimePercentageRepository;
		private readonly IContractScheduleRepository _contractScheduleRepository;
		private readonly IHandleCommand<ApproveRequestCommand> _approveRequestCommandHandler;
		private readonly IAlreadyAbsentValidator _alreadyAbsentValidator;

		public WaitlistRequestHandler(ISkillCombinationResourceRepository skillCombinationResourceRepository,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			ISkillRepository skillRepository,
			SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider,
			IActivityRepository activityRepository, IPersonRequestRepository personRequestRepository,
			IAbsenceRequestSetting absenceRequestSetting,
			ArrangeRequestsByProcessOrder arrangeRequestsByProcessOrder,
			ExtractSkillForecastIntervals extractSkillForecastIntervals, IResourceCalculation resourceCalculation,
			INow now,
			SchedulePartModifyAndRollbackServiceWithoutStateHolder rollbackService,
			ISkillTypeRepository skillTypeRepository, IPersonRepository personRepository, IContractRepository contractRepository, IPartTimePercentageRepository partTimePercentageRepository,
			IContractScheduleRepository contractScheduleRepository, IHandleCommand<ApproveRequestCommand> approveRequestCommandHandler,
			IAlreadyAbsentValidator alreadyAbsentValidator)
		{
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
			_now = now;
			_rollbackService = rollbackService;
			_skillTypeRepository = skillTypeRepository;
			_personRepository = personRepository;
			_contractRepository = contractRepository;
			_partTimePercentageRepository = partTimePercentageRepository;
			_contractScheduleRepository = contractScheduleRepository;
			_approveRequestCommandHandler = approveRequestCommandHandler;
			_alreadyAbsentValidator = alreadyAbsentValidator;
		}

		public class WaitlistHelpers
		{
			public bool InitSuccess;
			public ResourceCalculationData ResCalcData;
			public IScheduleDictionary PersonsSchedules;
			public List<SkillCombinationResource> CombinationResources;
			public List<ISkill> Skills;
			public List<IPersonRequest> AllRequests;
			public DateOnlyPeriod DateOnlyPeriodOne;
			public DateTimePeriod LoadSchedulesPeriodToCoverForMidnightShifts;
			public int SkillInterval;
			public IEnumerable<SkillStaffingInterval> SkillStaffingIntervals;
		}

		private WaitlistHelpers initializeWaitlistHandling(IEnumerable<Guid> requestIdsToSkip)
		{
			var helpers = new WaitlistHelpers();
			if (!_skillCombinationResourceReadModelValidator.Validate())
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + "Read model is not up to date");
				helpers.InitSuccess = false;
				return helpers;
			}
			var validPeriod = new DateTimePeriod(_now.UtcDateTime(), _now.UtcDateTime().AddHours(_absenceRequestSetting.ImmediatePeriodInHours));
			helpers.LoadSchedulesPeriodToCoverForMidnightShifts = validPeriod.ChangeStartTime(TimeSpan.FromDays(-1));
			var waitlistedRequestsIds = _personRequestRepository.GetWaitlistRequests(helpers.LoadSchedulesPeriodToCoverForMidnightShifts).Except(requestIdsToSkip);
			var waitlistedRequests = _personRequestRepository.Find(waitlistedRequestsIds);

			waitlistedRequests =
				waitlistedRequests.Where(
					x =>
						x.Request.Period.StartDateTime >= validPeriod.StartDateTime &&
						x.Request.Period.EndDateTime <= validPeriod.EndDateTime).ToList();
			helpers.AllRequests = _arrangeRequestsByProcessOrder.GetRequestsSortedBySeniority(waitlistedRequests).ToList();
			helpers.AllRequests.AddRange(_arrangeRequestsByProcessOrder.GetRequestsSortedByDate(waitlistedRequests));

			_personRepository.FindPeople(helpers.AllRequests.Select(x => x.Person.Id.GetValueOrDefault()).ToList());

			var inflatedPeriod = new DateTimePeriod(helpers.AllRequests.Min(x => x.Request.Period.StartDateTime), helpers.AllRequests.Max(x => x.Request.Period.EndDateTime));

			helpers.CombinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(inflatedPeriod).ToList();
			if (!helpers.CombinationResources.Any())
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any skillcombinations.");
				helpers.InitSuccess = false;
				return helpers;
			}

			_activityRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_contractRepository.LoadAll();
			_partTimePercentageRepository.LoadAll();
			_contractScheduleRepository.LoadAllAggregate();

			helpers.Skills = _skillRepository.LoadAllSkills().ToList();

			var skillIds = new HashSet<Guid>();
			foreach (var skillCombinationResource in helpers.CombinationResources)
			{
				foreach (var skillId in skillCombinationResource.SkillCombination)
				{
					skillIds.Add(skillId);
				}
			}
			helpers.SkillInterval = helpers.Skills.Where(x => skillIds.Contains(x.Id.GetValueOrDefault())).Min(x => x.DefaultResolution);

			helpers.SkillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(helpers.Skills, inflatedPeriod, true).ToList();
			helpers.SkillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods =
				helpers.SkillStaffingIntervals.GroupBy(s => helpers.Skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
						v =>
							(IResourceCalculationPeriodDictionary)
							new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
								s => (IResourceCalculationPeriod)s)));
			helpers.ResCalcData = new ResourceCalculationData(helpers.Skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			var persons = waitlistedRequests.Select(p => p.Person).ToList();
			helpers.DateOnlyPeriodOne = ExtractSkillForecastIntervals.GetLongestPeriod(helpers.Skills, inflatedPeriod);
			helpers.PersonsSchedules =
				_scheduleStorage.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(helpers.LoadSchedulesPeriodToCoverForMidnightShifts, persons), _currentScenario.Current(),
					new PersonProvider(persons), new ScheduleDictionaryLoadOptions(false, false), persons);

			helpers.InitSuccess = true;
			return helpers;
		}

		[AsSystem]
		[UnitOfWork]
		public virtual void Handle(ProcessWaitlistedRequestsEvent @event)
		{
			try
			{
				var helpers = initializeWaitlistHandling(new List<Guid>());
				
				using (getContext(helpers.CombinationResources, helpers.Skills, false))
				{
					_resourceCalculation.ResourceCalculate(helpers.DateOnlyPeriodOne, helpers.ResCalcData,
						() => getContext(helpers.CombinationResources, helpers.Skills, true));
					//consider the exception and which requests to deny
					foreach (var pRequest in helpers.AllRequests)
					{
						var requestPeriod = pRequest.Request.Period;
						var schedules = helpers.PersonsSchedules[pRequest.Person];
						if (_alreadyAbsentValidator.Validate((IAbsenceRequest) pRequest.Request, schedules))
						{
							pRequest.Deny("RequestDenyReasonAlreadyAbsent", new PersonRequestCheckAuthorization(),null,PersonRequestDenyOption.AlreadyAbsence);
							continue;
						}
						var dateOnlyPeriod = helpers.LoadSchedulesPeriodToCoverForMidnightShifts.ToDateOnlyPeriod(pRequest.Person.PermissionInformation.DefaultTimeZone());
						var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);
						var startDateTimeInPersonTimeZone = TimeZoneHelper.ConvertFromUtc(pRequest.RequestedDate, pRequest.Person.PermissionInformation.DefaultTimeZone());
						var scheduleDay = schedules.ScheduledDay(new DateOnly(startDateTimeInPersonTimeZone));
						var personAssignment = scheduleDay.PersonAssignment();
						if (personAssignment == null) continue;

						var absenceRequest = (IAbsenceRequest)pRequest.Request;
						scheduleDay.CreateAndAddAbsence(new AbsenceLayer(absenceRequest.Absence, pRequest.Request.Period));

						_rollbackService.ModifyStrictly(scheduleDay, new NoScheduleTagSetter(), NewBusinessRuleCollection.Minimum());
						_resourceCalculation.ResourceCalculate(helpers.DateOnlyPeriodOne, helpers.ResCalcData, () => getContext(helpers.CombinationResources, helpers.Skills, true));

						var mergedPeriod = pRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)pRequest.Request);
						var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
						var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);
						var shiftPeriodList = new List<DateTimePeriod>();

						foreach (var day in scheduleDays)
						{
							var projection = day.ProjectionService().CreateProjection().FilterLayers(requestPeriod);
							var layers = projection.ToResourceLayers(helpers.SkillInterval).ToList();
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
								skillStaffingIntervalsToValidate.AddRange(helpers.SkillStaffingIntervals.Where(x => x.StartDateTime >= projectionPeriod.StartDateTime && x.StartDateTime < projectionPeriod.EndDateTime));
							}
							var validatedRequest = staffingThresholdValidators.FirstOrDefault().ValidateLight((IAbsenceRequest)pRequest.Request, skillStaffingIntervalsToValidate);
							_rollbackService.RollbackMinimumChecks();
							if (validatedRequest.IsValid)
							{
								if (!autoGrant) continue;
								var command = new ApproveRequestCommand
								{
									PersonRequestId = pRequest.Id.GetValueOrDefault(),
									IsAutoGrant = true
								};
								_approveRequestCommandHandler.Handle(command);
								if (!command.ErrorMessages.IsEmpty())
								{
									// TODO: log errors
								}
								_rollbackService.ClearModificationCollection();
							}
						}
						else
						{
							logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any staffingThresholdValidator.");
						}
					}
				}
			}
			catch (Exception exp)
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + exp);
			}
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}
	}
}