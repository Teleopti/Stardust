using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class IntradayRequestProcessor : IIntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessor));

		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public IntradayRequestProcessor(ICommandDispatcher commandDispatcher, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_commandDispatcher = commandDispatcher;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public void Process(IPersonRequest personRequest)
		{
			personRequest.Pending();
			var cascadingPersonSkills = personRequest.Person.Period(DateOnly.Today).CascadingSkills();

			var lowestIndex = cascadingPersonSkills.Min(x => x.Skill.CascadingIndex);
			var primarySkills = cascadingPersonSkills.Where(x => x.Skill.CascadingIndex == lowestIndex);
			
			foreach (var primarySkill in primarySkills)
			{
				var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkill(primarySkill.Skill.Id.GetValueOrDefault(),
																	 personRequest.Request.Period.StartDateTime, personRequest.Request.Period.EndDateTime);

				//already understaffed
				if (skillStaffingIntervals.Any(x => x.StaffingLevel < x.ForecastWithShrinkage + 1))
				{
					sendDenyCommand(personRequest.Id.GetValueOrDefault(), "A Skill is already understaffed");
					return;
				}

			}
			sendApproveCommand(personRequest.Id.GetValueOrDefault());

		}
		
		private void sendDenyCommand(Guid personRequestId, string denyReason)
		{
			var command = new DenyRequestCommand()
			{
				PersonRequestId = personRequestId,
				DenyReason = denyReason,
				IsAlreadyAbsent = false
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				logger.Warn(command.ErrorMessages);
			}
		}

		private void sendApproveCommand(Guid personRequestId)
		{
			var command = new ApproveRequestCommand()
			{
				PersonRequestId = personRequestId,
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				logger.Warn(command.ErrorMessages);
			}
		}

	}
}
