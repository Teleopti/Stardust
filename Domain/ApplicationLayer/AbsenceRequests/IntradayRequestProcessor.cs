using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestProcessor
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(IntradayRequestProcessor));

		private readonly ICommandDispatcher _commandDispatcher;

		public IntradayRequestProcessor(ICommandDispatcher commandDispatcher)
		{
			_commandDispatcher = commandDispatcher;
		}

		public void Process(IPersonRequest personRequest)
		{
			personRequest.Pending();

			var workflowControlSet = personRequest.Request.Person.WorkflowControlSet;
			if (workflowControlSet == null)
			{
				handleNoWorkflowControlSet(personRequest);
				return;
			}

			sendApproveCommand(personRequest.Id.GetValueOrDefault());
		
		}

		private void handleNoWorkflowControlSet(IPersonRequest personRequest)
		{
			var denyReason = UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoWorkflow",
																		   personRequest.Request.Person.PermissionInformation.Culture());

			sendDenyCommand(personRequest.Id.GetValueOrDefault(), denyReason, false);

		}


		private void sendDenyCommand(Guid personRequestId, string denyReason, bool isAlreadyAbsent)
		{
			var command = new DenyRequestCommand()
			{
				PersonRequestId = personRequestId,
				DenyReason = denyReason,
				IsAlreadyAbsent = isAlreadyAbsent
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
