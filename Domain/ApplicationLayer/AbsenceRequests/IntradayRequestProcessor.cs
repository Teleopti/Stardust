﻿using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class IntradayRequestProcessor : IIntradayRequestProcessor
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

			IntradayStaffingCheck();

			sendApproveCommand(personRequest.Id.GetValueOrDefault());
		
		}

		private void IntradayStaffingCheck()
		{
			//Do something and send Deny if understaffed
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
