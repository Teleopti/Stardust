using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

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

		public RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> ids)
		{
			var trackInfo = new TrackedCommandInfo
			{
				OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value
			};

			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();

			foreach (var personRequestId in ids)
			{

				var command = new ApproveRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId
				};

				_commandDispatcher.Execute(command);

				if (command.ErrorMessages != null)
				{
					errorMessages.AddRange(command.ErrorMessages);
				}

				if (command.AffectedRequestId.HasValue)  affectedRequestIds.Add(command.AffectedRequestId.Value);
			}

			return new RequestCommandHandlingResult(affectedRequestIds, errorMessages);
		}

		public RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> ids)
		{
			var trackInfo = new TrackedCommandInfo
			{
				OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value
			};

			var affectedRequestIds = new List<Guid>();

			foreach (var personRequestId in ids)
			{
				var command = new DenyRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId,
					IsManualDeny = true
				};

				_commandDispatcher.Execute(command);

				if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);
			}

			return new RequestCommandHandlingResult (affectedRequestIds, null);

		}

		public RequestCommandHandlingResult CancelRequests (IEnumerable<Guid> ids)
		{
			var trackInfo = new TrackedCommandInfo
			{
				OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value
			};

			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();

			foreach (var personRequestId in ids)
			{
				var command = new CancelAbsenceRequestCommand()
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId
				};

				_commandDispatcher.Execute (command);

				if (command.AffectedRequestId.HasValue)
				{
					affectedRequestIds.Add (command.AffectedRequestId.Value);
				}

				if (command.ErrorMessages != null)
				{
					errorMessages.AddRange (command.ErrorMessages);
				}
			}

			return new RequestCommandHandlingResult(affectedRequestIds, errorMessages);
		}



	}
}