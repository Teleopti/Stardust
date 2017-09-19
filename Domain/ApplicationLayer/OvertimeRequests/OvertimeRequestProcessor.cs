using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestProcessor : IOvertimeRequestProcessor
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IEnumerable<IOvertimeRequestValidator> _overtimeRequestValidators;
		private readonly IOvertimeRequestAvailableSkillsValidator _overtimeRequestAvailableSkillsValidator;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillTypeRepository _skillTypeRepository;

		private static readonly ILog logger = LogManager.GetLogger(typeof(OvertimeRequestProcessor));

		public OvertimeRequestProcessor(ICommandDispatcher commandDispatcher,
			IEnumerable<IOvertimeRequestValidator> overtimeRequestValidators, IActivityRepository activityRepository, 
			ISkillRepository skillRepository, ISkillTypeRepository skillTypeRepository, IOvertimeRequestAvailableSkillsValidator overtimeRequestAvailableSkillsValidator)
		{
			_commandDispatcher = commandDispatcher;
			_overtimeRequestValidators = overtimeRequestValidators;
			_activityRepository = activityRepository;
			_skillRepository = skillRepository;
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestAvailableSkillsValidator = overtimeRequestAvailableSkillsValidator;
		}

		public void CheckAndProcessDeny(IPersonRequest personRequest)
		{
			if (isNotValid(personRequest)) return;

			validateSkills(personRequest);
		}

		public void Process(IPersonRequest personRequest, bool isAutoGrant)
		{
			if (isNotValid(personRequest)) return;

			var skills = validateSkills(personRequest);
			if(skills == null) return;

			personRequest.Pending();
			if (!isAutoGrant) return;

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

		private bool isNotValid(IPersonRequest personRequest)
		{
			// Preload to get rid of proxies later on #45827
			_activityRepository.LoadAll();
			_skillTypeRepository.LoadAll();
			_skillRepository.LoadAll();

			foreach (var overtimeRequestValidator in _overtimeRequestValidators)
			{
				var resultOfBasicValidator = overtimeRequestValidator.Validate(personRequest);
				if (resultOfBasicValidator.IsValid) continue;
				denyRequest(personRequest, resultOfBasicValidator.InvalidReason);
				return true;
			}
			return false;
		}

		private ISkill[] validateSkills(IPersonRequest personRequest)
		{
			var resultOfAvailableSkillsValidator = _overtimeRequestAvailableSkillsValidator.Validate(personRequest);
			if (!resultOfAvailableSkillsValidator.IsValid)
			{
				denyRequest(personRequest, resultOfAvailableSkillsValidator.InvalidReason);
				return null;
			}

			return resultOfAvailableSkillsValidator.Skills;
		}
	}
}