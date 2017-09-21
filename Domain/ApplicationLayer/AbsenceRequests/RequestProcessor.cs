using System;
using System.Collections.Generic;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RequestProcessor : IRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessorOld));
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICurrentScenario _currentScenario;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly SkillCombinationResourceReadModelValidator _skillCombinationResourceReadModelValidator;
		private readonly IAbsenceRequestValidatorProvider _absenceRequestValidatorProvider;
		private readonly SkillStaffingIntervalProvider _skillStaffingIntervalProvider;
		private readonly IActivityRepository _activityRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly INow _now;
		public RequestProcessor(ICommandDispatcher commandDispatcher,
			ISkillCombinationResourceRepository skillCombinationResourceRepository,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			ISkillRepository skillRepository, SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator,
			IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, SkillStaffingIntervalProvider skillStaffingIntervalProvider,
			IActivityRepository activityRepository, IPersonRequestRepository personRequestRepository, INow now)
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
		}

		public void Process(IPersonRequest personRequest, DateTime startTime)
		{
			try
			{
				var loadSchedulesPeriodToCoverForMidnightShifts = personRequest.Request.Period.ChangeStartTime(TimeSpan.FromDays(-1));
				if (personRequest.Person.WorkflowControlSet.AbsenceRequestWaitlistEnabled)
				{
					var waitlistedRequestsIds = _personRequestRepository.GetWaitlistRequests(personRequest.Request.Period);
					var waitlistedRequests = _personRequestRepository.Find(waitlistedRequestsIds);
					var validStart = _now.UtcDateTime() ; // add something?
					waitlistedRequests =
						waitlistedRequests.Where(
							x =>
						x.Request.Period.StartDateTime >= validStart).ToList();
					if (waitlistedRequests.Any())
					{
						sendDenyCommand(personRequest, Resources.AbsenceRequestAlreadyInWaitlist);
						return;
					}
				}
				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + "Read model is not up to date");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
					return;
				}

				//what if the agent changes personPeriod in the middle of the request period?
				//what if the request is 8:00-8:05, only a third of a resource should be removed

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(personRequest.Request.Period).ToArray();
				if (!combinationResources.Any())
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any skillcombinations.");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
					return;
				}

				_activityRepository.LoadAll();
				var allSkills = _skillRepository.LoadAll();
				
				var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(personRequest.Person, new ScheduleDictionaryLoadOptions(false, false), loadSchedulesPeriodToCoverForMidnightShifts, _currentScenario.Current())[personRequest.Person];

				var dateOnlyPeriod = loadSchedulesPeriodToCoverForMidnightShifts.ToDateOnlyPeriod(personRequest.Person.PermissionInformation.DefaultTimeZone());

				var scheduleDays = schedules.ScheduledDayCollection(dateOnlyPeriod);


				var skillIds = new HashSet<Guid>();
				foreach (var skillCombinationResource in combinationResources)
				{
					foreach (var skillId in skillCombinationResource.SkillCombination)
					{
						skillIds.Add(skillId);
					}
				}
				var skillInterval = allSkills.Where(x => skillIds.Contains(x.Id.GetValueOrDefault())).Min(x => x.DefaultResolution);

				var mergedPeriod = personRequest.Request.Person.WorkflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)personRequest.Request);
				var validators = _absenceRequestValidatorProvider.GetValidatorList(mergedPeriod);

				//this looks strange but is how it works. Pending = no autogrant, Grant = autogrant
				var autoGrant = mergedPeriod.AbsenceRequestProcess.GetType() != typeof(PendingAbsenceRequest);

				var shiftPeriodList = new List<DateTimePeriod>();
				foreach (var day in scheduleDays)
				{
					var projection = day.ProjectionService().CreateProjection().FilterLayers(personRequest.Request.Period);

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
					var useShrinkage = staffingThresholdValidators.Any(x => x.GetType() == typeof(StaffingThresholdWithShrinkageValidator));
					var skillStaffingIntervals = _skillStaffingIntervalProvider.GetSkillStaffIntervalsAllSkills(personRequest.Request.Period, combinationResources.ToList(), useShrinkage);
					var skillStaffingIntervalsToValidate = new List<SkillStaffingInterval>();
					foreach (var projectionPeriod in shiftPeriodList)
					{
						skillStaffingIntervalsToValidate.AddRange(skillStaffingIntervals.Where(x => x.StartDateTime >= projectionPeriod.StartDateTime && x.StartDateTime < projectionPeriod.EndDateTime));
					}
					var validatedRequest = staffingThresholdValidators.FirstOrDefault().ValidateLight((IAbsenceRequest)personRequest.Request, skillStaffingIntervalsToValidate);
					if (validatedRequest.IsValid)
					{
						if (!autoGrant) return;
						var result = sendApproveCommand(personRequest);
						if (!result)
						{
							sendDenyCommand(personRequest, validatedRequest.ValidationErrors);
						}
					}
					else
					{
						sendDenyCommand(personRequest, validatedRequest.ValidationErrors);
					}
				}
				else
				{
					logger.Error(Resources.DenyReasonTechnicalIssues + " Can not find any staffingThresholdValidator.");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
				}
			}
			catch (Exception exp)
			{
				logger.Error(Resources.DenyReasonTechnicalIssues + exp);
				sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
			}
		}

		private void sendDenyCommand(IPersonRequest personRequest, string denyReason)
		{
			var command = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = denyReason
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
			}
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

	public interface IRequestProcessor
	{
		void Process(IPersonRequest personRequest, DateTime startTime);
	}

	public interface IAbsenceRequestSetting
	{
		int ImmediatePeriodInHours { get; }
	}

	public class AbsenceRequestOneDaySetting : IAbsenceRequestSetting
	{
		public int ImmediatePeriodInHours => 25;
	}

	public class AbsenceRequestFourteenDaySetting : IAbsenceRequestSetting
	{
		public int ImmediatePeriodInHours => 24 * 14 + 1;
	}

}