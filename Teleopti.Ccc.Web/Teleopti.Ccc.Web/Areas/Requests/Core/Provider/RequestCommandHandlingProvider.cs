using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class RequestCommandHandlingProvider : IRequestCommandHandlingProvider
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public RequestCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
		}

		public RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> requestIds, string replyMessage)
		{
			var trackInfo = createTrackedCommandInfo();
			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();
			var replySuccessCount = 0;

			foreach (var personRequestId in requestIds)
			{
				var command = new ApproveRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId,
					ReplyMessage = replyMessage,
					IgnoreErrorMessageForApprovedRequest = false
				};

				_commandDispatcher.Execute(command);

				if (command.ErrorMessages != null)
				{
					errorMessages.AddRange(command.ErrorMessages);
				}

				if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);
				if (command.IsReplySuccess) replySuccessCount++;

			}

			return new RequestCommandHandlingResult(affectedRequestIds, errorMessages, replySuccessCount);
		}

		public RequestCommandHandlingResult ApproveWithValidators(IEnumerable<Guid> requestIds,
			RequestValidatorsFlag validators)
		{
			var trackInfo = createTrackedCommandInfo();
			var command = new ApproveBatchRequestsCommand
			{
				TrackedCommandInfo = trackInfo,
				Validator = validators,
				PersonRequestIdList = requestIds,
			};

			_commandDispatcher.Execute(command);

			var errorMessages = new List<string>();
			if (command.ErrorMessages != null)
			{
				errorMessages.AddRange(command.ErrorMessages);
			}

			return new RequestCommandHandlingResult(new Guid[] {}, errorMessages, trackInfo.TrackId)
			{
				Success = !errorMessages.Any()
			};
		}

		public RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> requestIds, string replyMessage)
		{
			var trackInfo = createTrackedCommandInfo();
			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();
			var replySuccessCount = 0;

			foreach (var personRequestId in requestIds)
			{
				var command = new DenyRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId,
					IsManualDeny = true,
					ReplyMessage = replyMessage,
					DenyReason = "RequestDenyReasonSupervisor"
				};

				_commandDispatcher.Execute(command);

				if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);

				if (command.ErrorMessages != null)
				{
					errorMessages.AddRange(command.ErrorMessages);
				}
				if (command.IsReplySuccess) replySuccessCount++;
			}

			return new RequestCommandHandlingResult(affectedRequestIds, errorMessages, replySuccessCount);
		}

		public RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> requestIds, string replyMessage)
		{
			var trackInfo = createTrackedCommandInfo();
			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();
			var replySuccessCount = 0;

			foreach (var personRequestId in requestIds)
			{
				var command = new CancelAbsenceRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId,
					ReplyMessage = replyMessage
				};

				_commandDispatcher.Execute(command);

				if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);

				if (command.ErrorMessages != null)
				{
					errorMessages.AddRange(command.ErrorMessages);
				}
				if (command.IsReplySuccess) replySuccessCount++;
			}

			return new RequestCommandHandlingResult(affectedRequestIds, errorMessages, replySuccessCount);
		}

		public RequestCommandHandlingResult ReplyRequests(IEnumerable<Guid> requestIds, string message)
		{
			var trackInfo = createTrackedCommandInfo();
			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();
			var replySuccessCount = 0;

			foreach (var personRequestId in requestIds)
			{
				var command = new ReplyRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId,
					ReplyMessage = message
				};

				_commandDispatcher.Execute(command);

				if (command.AffectedRequestId.HasValue)
				{
					affectedRequestIds.Add(command.AffectedRequestId.Value);
				}

				if (command.ErrorMessages != null)
				{
					errorMessages.AddRange(command.ErrorMessages);
				}
				if (command.IsReplySuccess) replySuccessCount++;
			}

			return new RequestCommandHandlingResult(affectedRequestIds, errorMessages, replySuccessCount);
		}

		public RequestCommandHandlingResult RunWaitlist(DateTimePeriod period)
		{
			var trackInfo = createTrackedCommandInfo();
			var errorMessages = new List<string>();
			var command = new RunWaitlistCommand
			{
				TrackedCommandInfo = trackInfo,
				Period = period,
				CommandId = trackInfo.TrackId
			};

			_commandDispatcher.Execute(command);

			if (command.ErrorMessages != null)
			{
				errorMessages.AddRange(command.ErrorMessages);
			}

			return new RequestCommandHandlingResult(new Guid[] {}, errorMessages, trackInfo.TrackId)
			{
				Success = !errorMessages.Any()
			};
		}

		private TrackedCommandInfo createTrackedCommandInfo()
		{
			return new TrackedCommandInfo
			{
				TrackId = Guid.NewGuid(),
				OperatedPersonId = _loggedOnUser.CurrentUser().Id.GetValueOrDefault()
			};
		}
	}
}