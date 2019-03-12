using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RequestProcessor : IRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(nameof(RequestProcessor));
		private static readonly ILog requestsLogger = LogManager.GetLogger("Teleopti.Requests");
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly SkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly ISkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly INow _now;
		private readonly SmartDeltaDoer _smartDeltaDoer;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IExtensiveLogRepository _extensiveLogRepository;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public RequestProcessor(ICommandDispatcher commandDispatcher,
			ISkillCombinationResourceRepository skillCombinationResourceRepository,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			ISkillRepository skillRepository, SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, ISkillStaffingIntervalProvider skillStaffingIntervalProvider,
			IActivityRepository activityRepository, IPersonRequestRepository personRequestRepository, INow now, SmartDeltaDoer smartDeltaDoer,
			ISkillDayLoadHelper skillDayLoadHelper, IScheduledStaffingProvider scheduledStaffingProvider, IExtensiveLogRepository extensiveLogRepositor,
			IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_commandDispatcher = commandDispatcher;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_skillRepository = skillRepository;
			_skillCombinationResourceReadModelValidator = skillCombinationResourceReadModelValidator;
			_absenceRequestValidatorProvider = absenceRequestValidatorProvider;
			_skillStaffingIntervalProvider = skillStaffingIntervalProvider;
			_activityRepository = activityRepository;
			_personRequestRepository = personRequestRepository;
			_now = now;
			_smartDeltaDoer = smartDeltaDoer;
			_skillDayLoadHelper = skillDayLoadHelper;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_extensiveLogRepository = extensiveLogRepositor;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public void Process(IPersonRequest personRequest)
		{
			try
			{
				var workflowControlSet = personRequest.Person.WorkflowControlSet;
				var absenceRequest = (IAbsenceRequest)personRequest.Request;

				var mergedPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod(absenceRequest);
				//this looks strange but is how it works. Pending = no autogrant, Grant = autogrant
				var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);

				var requestPeriod = personRequest.Request.Period;
				var timeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
				var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(timeZone);
				
				if (workflowControlSet.AbsenceRequestWaitlistEnabled && autoGrant)
				{
					var periods = personRequest.Person.PersonPeriods(requestPeriod.ToDateOnlyPeriod(timeZone));
					var absenceReqThresh = workflowControlSet.AbsenceRequestExpiredThreshold.GetValueOrDefault();
					var skills = periods.SelectMany(x => x.PersonSkillCollection.Select(y => y.Skill.Id.GetValueOrDefault())).Distinct();

					var hasWaitlisted = _personRequestRepository.HasWaitlistedRequestsOnSkill(skills,
						requestPeriod.StartDateTime, requestPeriod.EndDateTime, _now.UtcDateTime().AddMinutes(absenceReqThresh));

					if (hasWaitlisted)
					{
						sendDenyCommand(personRequest, Resources.AbsenceRequestAlreadyInWaitlist, PersonRequestDenyOption.None);
						return;
					}
				}

				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonSystemBusy, CultureInfo.GetCultureInfo("en-US")) + $"Read model is not up to date Request {personRequest.Request.Id}");
					sendDenyCommand(personRequest, Resources.DenyReasonSystemBusy, PersonRequestDenyOption.TechnicalIssues);
					return;
				}

				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(personRequest.Person, new ScheduleDictionaryLoadOptions(false, false), requestPeriod, _currentScenario.Current())[personRequest.Person];
				var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod).ToList();
				var adjustedRequestPeriod = adjustFullDayRequestPeriodToCoverOvernightShift(scheduleDays, absenceRequest);

				//what if the agent changes personPeriod in the middle of the request period?
				//what if the request is 8:00-8:05, only a third of a resource should be removed
				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(adjustedRequestPeriod).ToArray();

				if (!combinationResources.Any())
				{
					logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonNoSkillCombinationsFound, CultureInfo.GetCultureInfo("en-US")) + $" Can not find any skillcombinations for period {requestPeriod} and Request {personRequest.Request.Id}.");
					sendDenyCommand(personRequest, Resources.DenyReasonNoSkillCombinationsFound, PersonRequestDenyOption.TechnicalIssues);
					return;
				}

				dynamic skillCombLogObject = new ExpandoObject();
				skillCombLogObject.SkillCombinationResourcesRaw = combinationResources;
				skillCombLogObject.SkillCombinationResourcesZeroresources = combinationResources.Where(x => x.Resource <= 0);
				
				_activityRepository.LoadAll();
				var allSkills = _skillRepository.LoadAll().ToHashSet();
				var skillIds = combinationResources.SelectMany(s => s.SkillCombination).Distinct().ToHashSet();
				var skillInterval = allSkills.Where(x => skillIds.Contains(x.Id.GetValueOrDefault())).Min(x => x.DefaultResolution);

				var shiftPeriodList = new List<DateTimePeriod>();
				foreach (var day in scheduleDays)
				{
					var projection = day.ProjectionService().CreateProjection().FilterLayers(requestPeriod);

					var layers = projection.ToResourceLayers(skillInterval, day.TimeZone).ToList();
					if (!layers.Any())
					{
						continue;
					}

					shiftPeriodList.Add(new DateTimePeriod(projection.OriginalProjectionPeriod.GetValueOrDefault().StartDateTime, projection.OriginalProjectionPeriod.GetValueOrDefault().EndDateTime));
					
					_smartDeltaDoer.Do(layers, personRequest.Person, dateOnlyPeriod.StartDate, combinationResources);
				}

				skillCombLogObject.SkillCombinationAfterDeltaDist = combinationResources;
				skillCombLogObject.SkillCombinationZeroResourcesAfterDeltaDist = combinationResources.Where(x => x.Resource <= 0);
				_extensiveLogRepository.Add(skillCombLogObject, personRequest.Id.GetValueOrDefault(), "PersonRequest.CombinationResources");

				var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);
				var staffingThresholdValidators = validators.OfType<StaffingThresholdValidator>().ToList();
				if (staffingThresholdValidators.Any())
				{
					calculateForecastedAgentsForEmailSkills(requestPeriod, false, allSkills);

					var useShrinkage = staffingThresholdValidators.Any(x => x.GetType() == typeof(StaffingThresholdWithShrinkageValidator));
					var validator = staffingThresholdValidators.First();
					var skillStaffingIntervalsToValidate = getFilteredSkillStaffingIntervals(adjustedRequestPeriod, combinationResources, useShrinkage, shiftPeriodList, personRequest.Request.Id);

					var validatedRequest = validator.ValidateLight(absenceRequest, skillStaffingIntervalsToValidate);

					dynamic logObject = new ExpandoObject();
					logObject.ValidationResult = validatedRequest;
					logObject.SkillStaffingIntervals = skillStaffingIntervalsToValidate;
					_extensiveLogRepository.Add(logObject, personRequest.Id.GetValueOrDefault(), "PersonRequest.SkillStaffIntervals");

					if (validatedRequest.IsValid)
					{
						if (!autoGrant) return;
						var result = sendApproveCommand(personRequest);
						if (!result)
						{
							sendDenyCommand(personRequest, validatedRequest.ValidationErrors, PersonRequestDenyOption.None);
						}
					}
					else
					{
						sendDenyCommand(personRequest, validatedRequest.ValidationErrors, validatedRequest.DenyOption.GetValueOrDefault(PersonRequestDenyOption.None));
					}
				}
				else
				{
					logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonTechnicalIssues, CultureInfo.GetCultureInfo("en-US")) + " Can not find any staffingThresholdValidator.");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues, PersonRequestDenyOption.TechnicalIssues);
				}
			}
			catch (Exception exp)
			{
				logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonSystemBusy, CultureInfo.GetCultureInfo("en-US")) + exp);
				sendDenyCommand(personRequest, Resources.DenyReasonSystemBusy, PersonRequestDenyOption.TechnicalIssues);
			}
		}

		private DateTimePeriod adjustFullDayRequestPeriodToCoverOvernightShift(List<IScheduleDay> scheduleDays,IAbsenceRequest absenceRequest)
		{
			var adjustedRequestPeriod = absenceRequest.Period;
			if (absenceRequest.FullDay && scheduleDays.Any())
			{
				adjustedRequestPeriod = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(
					absenceRequest.Period,
					absenceRequest.Person, scheduleDays.First(), scheduleDays.Last(), _globalSettingDataRepository);
			}

			return adjustedRequestPeriod;
		}

		private List<SkillStaffingInterval> getFilteredSkillStaffingIntervals(DateTimePeriod requestPeriod,
			SkillCombinationResource[] combinationResources, bool useShrinkage, List<DateTimePeriod> shiftPeriodList,
			Guid? requestId)
		{
			var skillStaffingIntervals =
				_skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(requestPeriod,
					combinationResources.ToList(), useShrinkage);
			var skillStaffingIntervalsToValidate = new List<SkillStaffingInterval>();
			foreach (var projectionPeriod in shiftPeriodList)
			{
				var staffingIntervalsWithinPeriod = skillStaffingIntervals.Where(x =>
						x.StartDateTime >= projectionPeriod.StartDateTime &&
						x.StartDateTime < projectionPeriod.EndDateTime)
					.ToList();

				requestsLogger.Debug(
					$"Adding {staffingIntervalsWithinPeriod.Count} intervals to validate for request: {requestId} from period: {projectionPeriod.StartDateTime} - {projectionPeriod.EndDateTime}");
				skillStaffingIntervalsToValidate.AddRange(staffingIntervalsWithinPeriod);
			}

			return skillStaffingIntervalsToValidate;
		}

		private void sendDenyCommand(IPersonRequest personRequest, string denyReason, PersonRequestDenyOption denyOption)
		{
			requestsLogger.Debug($"sendDenyCommand for request: {personRequest.Request.Id}");
			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = denyReason,
				DenyOption = denyOption
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}
		}

		private bool sendApproveCommand(IPersonRequest personRequest)
		{
			requestsLogger.Debug($"sendApproveCommand for request: {personRequest.Request.Id}");
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

		private void calculateForecastedAgentsForEmailSkills(DateTimePeriod period, bool useShrinkage,
			HashSet<ISkill> allSkills)
		{
			var originRequestDateOnlyPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.Date),
				new DateOnly(period.EndDateTime.Date));
			var skillDays = _skillDayLoadHelper.LoadSchedulerSkillDays(originRequestDateOnlyPeriod, allSkills, _currentScenario.Current());

			var skillGroupsByResuolution = skillDays.Keys
				.Where(SkillTypesWithBacklog.IsBacklogSkillType)
				.GroupBy(x => x.DefaultResolution);
			foreach (var group in skillGroupsByResuolution)
			{
				var emailSkillsForOneResolution = group.ToList();
				var scheduledStaffingPerSkill = _scheduledStaffingProvider
					.StaffingPerSkill(emailSkillsForOneResolution, period, useShrinkage)
					.ToLookup(s => s.SkillId);

				foreach (var skill in emailSkillsForOneResolution)
				{
					var skillDaysEmail = skillDays[skill];
					foreach (var skillDay in skillDaysEmail)
					{
						foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
						{
							skillStaffPeriod.SetCalculatedResource65(0);

							if (!scheduledStaffingPerSkill.Contains(skill.Id.GetValueOrDefault())) continue;
							var scheduledStaff = scheduledStaffingPerSkill[skill.Id.GetValueOrDefault()]
								.FirstOrDefault(x => x.StartDateTime == skillStaffPeriod.Period.StartDateTime);

							if (scheduledStaff == null)
								continue;
							if (scheduledStaff.StaffingLevel > 0)
								skillStaffPeriod.SetCalculatedResource65(scheduledStaff.StaffingLevel);
						}
					}
				}
			}
		}
	}

	public interface IRequestProcessor
	{
		void Process(IPersonRequest personRequest);
	}

	public interface IAbsenceRequestSetting
	{
		int ImmediatePeriodInHours { get; }
	}

	public class AbsenceRequestFourteenDaySetting : IAbsenceRequestSetting
	{
		public int ImmediatePeriodInHours => 24 * 14 + 1;
	}

	public class SmartDeltaDoer
	{
		private readonly IPersonSkillProvider _personSkillProvider;

		public SmartDeltaDoer(IPersonSkillProvider personSkillProvider)
		{
			_personSkillProvider = personSkillProvider;
		}

		public void Do(IEnumerable<ResourceLayer> layers, IPerson person, DateOnly startDate, IEnumerable<SkillCombinationResource> combinationResources)
		{
			foreach (var layer in layers)
			{
				var skillCombination = _personSkillProvider.SkillsOnPersonDate(person, startDate).ForActivity(layer.PayloadId);
				if (!skillCombination.Skills.Any()) continue;
				distributeSmartly(combinationResources, skillCombination, layer);
			}

		}

		private void distributeSmartly(IEnumerable<SkillCombinationResource> combinationResources,
													 SkillCombination skillCombination, ResourceLayer layer)
		{
			var skillCombinationResourceByAgentAndLayer =
combinationResources.SingleOrDefault(x => x.SkillCombination.NonSequenceEquals(skillCombination.Skills.Select(y => y.Id.GetValueOrDefault()))
						&& layer.Period.StartDateTime >= x.StartDateTime && layer.Period.EndDateTime <= x.EndDateTime);

			if (skillCombinationResourceByAgentAndLayer == null) return;

			var part = layer.Period.ElapsedTime().TotalMinutes / skillCombinationResourceByAgentAndLayer.GetTimeSpan().TotalMinutes;
			var resource = skillCombinationResourceByAgentAndLayer.Resource - part;
			if (resource < 0) resource = 0;

			skillCombinationResourceByAgentAndLayer.Resource = resource;
		}
	}
}