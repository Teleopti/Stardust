﻿using System;
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
using System.Globalization;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestProcessorOld : IRequestProcessor
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
		
		public IntradayRequestProcessorOld(ICommandDispatcher commandDispatcher,
												 ISkillCombinationResourceRepository skillCombinationResourceRepository,
												 IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
												 ISkillRepository skillRepository, SkillCombinationResourceReadModelValidator skillCombinationResourceReadModelValidator, 
												 IAbsenceRequestValidatorProvider absenceRequestValidatorProvider, SkillStaffingIntervalProvider skillStaffingIntervalProvider, 
												 IActivityRepository activityRepository)
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
		}

		public void Process(IPersonRequest personRequest)
		{
			try
			{
				if (!_skillCombinationResourceReadModelValidator.Validate())
				{
					logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonTechnicalIssues, CultureInfo.GetCultureInfo("en-US")) + $"Read model is not up to date Request {personRequest.Request.Id}");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
					return;
				}

				//what if the agent changes personPeriod in the middle of the request period?
				//what if the request is 8:00-8:05, only a third of a resource should be removed

				var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(personRequest.Request.Period).ToArray();
				if (!combinationResources.Any())
				{
					logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonTechnicalIssues, CultureInfo.GetCultureInfo("en-US")) + $" Can not find any skillcombinations for period {personRequest.Request.Period} and Request {personRequest.Request.Id}.");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
					return;
				}

				_activityRepository.LoadAll();
				var allSkills = _skillRepository.LoadAll();
				var loadSchedulesPeriodToCoverForMidnightShifts = personRequest.Request.Period.ChangeStartTime(TimeSpan.FromDays(-1));
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
					var validatedRequest = staffingThresholdValidators.FirstOrDefault().ValidateLight((IAbsenceRequest) personRequest.Request, skillStaffingIntervalsToValidate);
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
					logger.Error(Resources.ResourceManager.GetString(Resources.DenyReasonTechnicalIssues, CultureInfo.GetCultureInfo("en-US")) + " Can not find any staffingThresholdValidator.");
					sendDenyCommand(personRequest, Resources.DenyReasonTechnicalIssues);
				}
			}
			catch (Exception exp)
			{
				logger.Error(exp);
				throw;
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
	}
	
}