using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
		private readonly ISkillTypeRepository _skillTypeRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(OvertimeRequestProcessor));

		public OvertimeRequestProcessor(ICommandDispatcher commandDispatcher,
			IEnumerable<IOvertimeRequestValidator> overtimeRequestValidators, IActivityRepository activityRepository,
			ISkillRepository skillRepository, ISkillTypeRepository skillTypeRepository,
			IOvertimeRequestAvailableSkillsValidator overtimeRequestAvailableSkillsValidator,
			INow now)
		{
			_commandDispatcher = commandDispatcher;
			_overtimeRequestValidators = overtimeRequestValidators;
			_activityRepository = activityRepository;
			_skillRepository = skillRepository;
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestAvailableSkillsValidator = overtimeRequestAvailableSkillsValidator;
			_now = now;
		}

		public void CheckAndProcessDeny(IPersonRequest personRequest)
		{
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
			}
		}

		public void Process(IPersonRequest personRequest, bool isAutoGrant)
		{
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

			personRequest.Pending();

			if (!isAutoGrant) return;

			executeApproveCommand(personRequest, validateSkillsResult.Skills);
		}

		public void Process(IPersonRequest personRequest)
		{
			var overtimeOpenPeriod = getOvertimeRequestOpenPeriod(personRequest);

			if (overtimeOpenPeriod.AutoGrantType == WorkflowControl.OvertimeRequestAutoGrantType.Deny)
			{
				denyRequest(personRequest, overtimeOpenPeriod.DenyReason);
				return;
			}

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

			if (overtimeOpenPeriod.AutoGrantType == WorkflowControl.OvertimeRequestAutoGrantType.No)
				return;

			executeApproveCommand(personRequest, validateSkillsResult.Skills);
		}

		private void executeApproveCommand(IPersonRequest personRequest, ISkill[] skills)
		{
			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true,
				OvertimeValidatedSkills = skills
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
				var overtimeRequestValidationResult = overtimeRequestValidator.Validate(personRequest);
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
				denyRequest(personRequest, overtimeRequestValidationResult.InvalidReasons.First());
			}
			personRequest.TrySetMessage(string.Join(Environment.NewLine,overtimeRequestValidationResult.InvalidReasons));
		}

		private IOvertimeRequestOpenPeriod getOvertimeRequestOpenPeriod(IPersonRequest personRequest)
		{
			return personRequest.Person.WorkflowControlSet.GetMergedOvertimeRequestOpenPeriod(personRequest.Request as IOvertimeRequest,
				new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), personRequest.Person.PermissionInformation.DefaultTimeZone())));
		}
	}
}