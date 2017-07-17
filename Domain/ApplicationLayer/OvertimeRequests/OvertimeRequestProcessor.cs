using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestProcessor : IOvertimeRequestProcessor
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly IEnumerable<IOvertimeRequestValidator> _overtimeRequestValidators;

		private static readonly ILog logger = LogManager.GetLogger(typeof(OvertimeRequestProcessor));

		public OvertimeRequestProcessor(ICommandDispatcher commandDispatcher,
			IEnumerable<IOvertimeRequestValidator> overtimeRequestValidators)
		{
			_commandDispatcher = commandDispatcher;
			_overtimeRequestValidators = overtimeRequestValidators;
		}

		public void Process(IPersonRequest personRequest)
		{
			foreach (var overtimeRequestValidator in _overtimeRequestValidators)
			{
				var result = overtimeRequestValidator.Validate(personRequest);
				if (result.IsValid) continue;
				var denyCommand = new DenyRequestCommand()
				{
					PersonRequestId = personRequest.Id.GetValueOrDefault(),
					DenyReason = result.InvalidReason,
					DenyOption = PersonRequestDenyOption.AutoDeny
				};
				_commandDispatcher.Execute(denyCommand);
				return;
			}

			personRequest.Pending();

			var command = new ApproveRequestCommand
			{
				PersonRequestId = personRequest.Id.GetValueOrDefault(),
				IsAutoGrant = true
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages.Any())
			{
				logger.Warn(command.ErrorMessages);
				var denyCommand = new DenyRequestCommand
				{
					PersonRequestId = personRequest.Id.GetValueOrDefault(),
					DenyReason = command.ErrorMessages.FirstOrDefault(),
					DenyOption = PersonRequestDenyOption.AutoDeny
				};
				_commandDispatcher.Execute(denyCommand);
			}
		}
	}
}