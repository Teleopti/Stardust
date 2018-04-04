﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestProcessor : IOvertimeRequestProcessor
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IEnumerable<IOvertimeRequestValidator> _overtimeRequestValidators;
		private readonly IOvertimeRequestAvailableSkillsValidator _overtimeRequestAvailableSkillsValidator;
		private readonly INow _now;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IUpdatedByScope _updatedByScope;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(OvertimeRequestProcessor));

		public OvertimeRequestProcessor(ICommandDispatcher commandDispatcher,
			IEnumerable<IOvertimeRequestValidator> overtimeRequestValidators, IActivityRepository activityRepository,
			ISkillRepository skillRepository, ISkillTypeRepository skillTypeRepository,
			IOvertimeRequestAvailableSkillsValidator overtimeRequestAvailableSkillsValidator,
			INow now, IPersonRepository personRepository, IUpdatedByScope updatedByScope)
		{
			_commandDispatcher = commandDispatcher;
			_overtimeRequestValidators = overtimeRequestValidators;
			_activityRepository = activityRepository;
			_skillRepository = skillRepository;
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestAvailableSkillsValidator = overtimeRequestAvailableSkillsValidator;
			_now = now;
			_personRepository = personRepository;
			_updatedByScope = updatedByScope;
		}

		public int StaffingDataAvailableDays { get; set; }

		public void Process(IPersonRequest personRequest)
		{
			personRequest.Pending();

			var validateRulesResult = validateRules(personRequest);
			if (!validateRulesResult.IsValid)
			{
				handleOvertimeRequestValidationResult(personRequest, validateRulesResult);
				return;
			}

			var validateSkillsResult = validateSkills(personRequest);
			if (!validateSkillsResult.IsValid)
			{
				handleOvertimeRequestValidationResult(personRequest, validateSkillsResult);
				return;
			}

			var skills = validateSkillsResult.SkillDictionary.SelectMany(x => x.Value).Distinct().ToArray();
			var overTimeRequestOpenPeriod = getOvertimeRequestOpenPeriodBySkillType(personRequest, skills);

			if (overTimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.Deny)
			{
				denyRequest(personRequest, overTimeRequestOpenPeriod.DenyReason);
				return;
			}

			if (overTimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.No)
				return;

			var person = _personRepository.Get(SystemUser.Id);
			_updatedByScope.OnThisThreadUse(person);

			executeApproveCommand(personRequest, validateSkillsResult.SkillDictionary);
		}

		private ISkillType getDefaultSkillType()
		{
			ISkillType phoneSkillType;
			phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			return phoneSkillType;
		}

		private void executeApproveCommand(IPersonRequest personRequest, IDictionary<DateTimePeriod,IList<ISkill>> skillDictionary)
		{
			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true,
				OvertimeValidatedSkillDictionary = skillDictionary
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
				denyRequest(personRequest, command.ErrorMessages.FirstOrDefault());
			}
		}

		private void denyRequest(IPersonRequest personRequest, string denyReason)
		{
			var denyCommand = new DenyRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				DenyReason = denyReason,
				DenyOption = PersonRequestDenyOption.AutoDeny
			};
			_commandDispatcher.Execute(denyCommand);
		}

		private OvertimeRequestValidationResult validateRules(IPersonRequest personRequest)
		{
			// Preload to get rid of proxies later on #45827
			_activityRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_skillRepository.LoadAll();

			foreach (var overtimeRequestValidator in _overtimeRequestValidators)
			{
				var overtimeRequestValidationResult = overtimeRequestValidator.Validate(
					new OvertimeRequestValidationContext(personRequest) { StaffingDataAvailableDays = StaffingDataAvailableDays });
				if (overtimeRequestValidationResult.IsValid) continue;
				return overtimeRequestValidationResult;
			}
			return new OvertimeRequestValidationResult { IsValid = true };
		}

		private OvertimeRequestAvailableSkillsValidationResult validateSkills(IPersonRequest personRequest)
		{
			return _overtimeRequestAvailableSkillsValidator.Validate(personRequest);
		}

		private void handleOvertimeRequestValidationResult(IPersonRequest personRequest,
			OvertimeRequestValidationResult overtimeRequestValidationResult)
		{
			if (overtimeRequestValidationResult.IsValid) return;
			if (overtimeRequestValidationResult.ShouldDenyIfInValid)
			{
				denyRequest(personRequest, string.Join(Environment.NewLine, overtimeRequestValidationResult.InvalidReasons));
			}
			else
			{
				personRequest.TrySetMessage(string.Join(Environment.NewLine, overtimeRequestValidationResult.InvalidReasons));
			}
			personRequest.TrySetBrokenBusinessRule(overtimeRequestValidationResult.BrokenBusinessRules);
		}

		private IOvertimeRequestOpenPeriod getOvertimeRequestOpenPeriodBySkillType(IPersonRequest personRequest,
			ISkill[] skillTypes)
		{
			if (personRequest.Person.WorkflowControlSet == null)
				return new OvertimeRequestOpenRollingPeriod
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Deny,
					DenyReason = Resources.MissingWorkflowControlSet
				};

			var defaultSkillType = getDefaultSkillType();
			var skillTypeFilteredOvertimeRequestOpenPeriods =
				personRequest.Person.WorkflowControlSet.OvertimeRequestOpenPeriods.Where(x =>
				{
					var skillType = x.SkillType ?? defaultSkillType;
					var matchedSkillTypes = skillTypes.Where(s => s.SkillType == skillType).ToList();
					return matchedSkillTypes.Any();
				}).ToList();

			var permissionInformation = personRequest.Person.PermissionInformation;
			var viewpointDate =
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), permissionInformation.DefaultTimeZone()));
			var overtimePeriodProjection = new OvertimeRequestPeriodProjection(skillTypeFilteredOvertimeRequestOpenPeriods,
				permissionInformation.Culture(),
				permissionInformation.UICulture(),
				viewpointDate);

			var agentTimeZone = permissionInformation.DefaultTimeZone();
			var dateOnlyPeriod = personRequest.Request.Period.ToDateOnlyPeriod(agentTimeZone);
			var projectedOvertimeRequestsOpenPeriods =
				overtimePeriodProjection.GetProjectedOvertimeRequestsOpenPeriods(dateOnlyPeriod);

			return new OvertimeRequestOpenPeriodMerger().Merge(projectedOvertimeRequestsOpenPeriods);
		}
	}
}