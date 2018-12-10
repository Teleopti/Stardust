using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestProcessor : IOvertimeRequestProcessor
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IEnumerable<IOvertimeRequestValidator> _overtimeRequestValidators;
		private readonly IOvertimeRequestAvailableSkillsValidator _overtimeRequestAvailableSkillsValidator;
		private readonly IOvertimeRequestContractWorkRulesValidator _overtimeRequestContractWorkRulesValidator;
		private readonly INow _now;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly IUpdatedByScope _updatedByScope;
		private readonly IPersonRepository _personRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(OvertimeRequestProcessor));

		public OvertimeRequestProcessor(ICommandDispatcher commandDispatcher,
			IEnumerable<IOvertimeRequestValidator> overtimeRequestValidators,
			IOvertimeRequestContractWorkRulesValidator overtimeRequestContractWorkRulesValidator,
			IOvertimeRequestAvailableSkillsValidator overtimeRequestAvailableSkillsValidator,
			INow now,
			IActivityRepository activityRepository,
			ISkillRepository skillRepository,
			ISkillTypeRepository skillTypeRepository,
			IPersonRepository personRepository,
			IUpdatedByScope updatedByScope)
		{
			_commandDispatcher = commandDispatcher;
			_overtimeRequestValidators = overtimeRequestValidators;
			_overtimeRequestContractWorkRulesValidator = overtimeRequestContractWorkRulesValidator;
			_overtimeRequestAvailableSkillsValidator = overtimeRequestAvailableSkillsValidator;
			_now = now;

			_activityRepository = activityRepository;
			_skillRepository = skillRepository;
			_skillTypeRepository = skillTypeRepository;
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

			if (validateSkillsResult.SkillDictionary == null)
			{
				handleOvertimeRequestValidationResult(personRequest, validateSkillsResult);
				return;
			}

			var overTimeRequestOpenPeriod = getOvertimeRequestOpenPeriod(validateSkillsResult, personRequest);
			if (!validateSkillsResult.IsValid)
			{
				validateSkillsResult.ShouldDenyIfInValid =
					overTimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.Yes;
				handleOvertimeRequestValidationResult(personRequest, validateSkillsResult);
				return;
			}

			var workRuleValidationResult = validateWorkRules(personRequest, overTimeRequestOpenPeriod);
			if (!workRuleValidationResult.IsValid)
			{
				handleOvertimeRequestValidationResult(personRequest, workRuleValidationResult);
				return;
			}

			if (overTimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.Deny)
			{
				denyRequest(personRequest, overTimeRequestOpenPeriod.DenyReason);
				return;
			}

			if (overTimeRequestOpenPeriod.AutoGrantType == OvertimeRequestAutoGrantType.No)
				return;

			executeApproveCommand(personRequest, validateSkillsResult.SkillDictionary);
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

		private OvertimeRequestValidationResult validateWorkRules(IPersonRequest personRequest, OvertimeRequestSkillTypeFlatOpenPeriod overTimeRequestOpenPeriod)
		{
			return _overtimeRequestContractWorkRulesValidator.Validate(personRequest, overTimeRequestOpenPeriod);
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

		private OvertimeRequestSkillTypeFlatOpenPeriod getOvertimeRequestOpenPeriod(OvertimeRequestAvailableSkillsValidationResult validateSkillsResult, IPersonRequest personRequest)
		{
			var skills = validateSkillsResult.SkillDictionary.SelectMany(x => x.Value).Distinct().ToArray();
			if (personRequest.Person.WorkflowControlSet == null)
				return new OvertimeRequestSkillTypeFlatOpenPeriod
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Deny,
					DenyReason = Resources.MissingWorkflowControlSet
				};

			var defaultSkillType = getDefaultSkillType();
			var skillTypeFilteredOvertimeRequestOpenPeriods =
				new SkillTypeFlatOvertimeOpenPeriodMapper().Map(personRequest.Person.WorkflowControlSet.OvertimeRequestOpenPeriods, defaultSkillType).Where(x =>
				{
					var skillType = x.SkillType;
					var matchedSkillTypes = skills.Where(s => s.SkillType.Equals(skillType)).ToList();
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

			return projectedOvertimeRequestsOpenPeriods.OrderBy(o => o.OrderIndex).Last();
		}

		private ISkillType getDefaultSkillType()
		{
			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			return phoneSkillType;
		}

		private void executeApproveCommand(IPersonRequest personRequest, IDictionary<DateTimePeriod, IList<ISkill>> skillDictionary)
		{
			setUpdateScope();

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

		private void setUpdateScope()
		{
			var person = _personRepository.Get(SystemUser.Id);
			_updatedByScope.OnThisThreadUse(person);
		}
	}
}