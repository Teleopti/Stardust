using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Requests.Core.Provider
{
	public class ReqeustCommandHandlingProvider : IRequestCommandHandlingProvider
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public ReqeustCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<Guid> ApproveRequests(IEnumerable<Guid> ids)
		{
			var trackInfo = new TrackedCommandInfo
			{
				OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value
			};

			var affectedRequestIds = new List<Guid>();

			foreach (var personRequestId in ids)
			{
				var command = new ApproveRequestCommand
				{
					TrackedCommandInfo = trackInfo,
					PersonRequestId = personRequestId
				};

				_commandDispatcher.Execute(command);

				if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);				
			}

			return affectedRequestIds;
		}

		public IEnumerable<Guid> DenyRequests(IEnumerable<Guid> ids)
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
					PersonRequestId = personRequestId
				};

				_commandDispatcher.Execute(command);

				if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);
			}

			return affectedRequestIds;
		}
	}
}