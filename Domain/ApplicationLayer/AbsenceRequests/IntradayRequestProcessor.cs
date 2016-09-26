using System;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
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
			IAbsenceRequest request = personRequest.Request as AbsenceRequest;
			if (request == null)
			{
				logger.WarnFormat("Person Request is not an Absence Request. (Id = {0})", personRequest.Id.GetValueOrDefault());
				return;
			}

			var workflowControlSet = request.Person.WorkflowControlSet;
			if (workflowControlSet == null)
			{
				handleNoWorkflowControlSet(request);
				return;
			}

			sendApproveCommand(request.Id.GetValueOrDefault());
		
		}

		private void handleNoWorkflowControlSet(IAbsenceRequest absenceRequest)
		{
			var denyReason = UserTexts.Resources.ResourceManager.GetString("RequestDenyReasonNoWorkflow",
																		   absenceRequest.Person.PermissionInformation.Culture());

			sendDenyCommand(absenceRequest.Id.GetValueOrDefault(), denyReason, false);

		}


		private void sendDenyCommand(Guid requestId, string denyReason, bool isAlreadyAbsent)
		{
			var command = new DenyRequestCommand()
			{
				PersonRequestId = requestId,
				DenyReason = denyReason,
				IsAlreadyAbsent = isAlreadyAbsent
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				logger.Warn(command.ErrorMessages);
			}
		}

		private void sendApproveCommand(Guid requestId)
		{
			var command = new ApproveRequestCommand()
			{
				PersonRequestId = requestId
			};
			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				logger.Warn(command.ErrorMessages);
			}
		}

	}
}
