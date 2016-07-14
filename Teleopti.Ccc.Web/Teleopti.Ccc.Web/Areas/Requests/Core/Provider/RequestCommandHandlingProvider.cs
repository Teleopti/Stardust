﻿using System;
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

        public RequestCommandHandlingResult ApproveRequests(IEnumerable<Guid> requestIds)
        {
            var trackInfo = createTrackedCommandInfo();
            var affectedRequestIds = new List<Guid>();
            var errorMessages = new List<string>();

            foreach (var personRequestId in requestIds)
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

                if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);
            }

            return new RequestCommandHandlingResult(affectedRequestIds, errorMessages);
        }

		public RequestCommandHandlingResult ApproveRequestsBasedOnBudgetAllotment(IEnumerable<Guid> requestIds)
		{
			var trackInfo = createTrackedCommandInfo();
			var command = new ApproveBatchRequestsCommand
			{
				TrackedCommandInfo = trackInfo,
				Validator = RequestValidatorsFlag.WriteProtectedScheduleValidator
							| RequestValidatorsFlag.BudgetAllotmentValidator,
				PersonRequestIdList = requestIds,
			};

			_commandDispatcher.Execute(command);

			var errorMessages = new List<string>();
			if (command.ErrorMessages != null)
			{
				errorMessages.AddRange(command.ErrorMessages);
			}

			return new RequestCommandHandlingResult(new Guid[] { }, errorMessages, trackInfo.TrackId);
		}

        public RequestCommandHandlingResult DenyRequests(IEnumerable<Guid> requestIds)
        {
            var trackInfo = createTrackedCommandInfo();
            var affectedRequestIds = new List<Guid>();
            var errorMessages = new List<string>();

            foreach (var personRequestId in requestIds)
            {
                var command = new DenyRequestCommand
                {
                    TrackedCommandInfo = trackInfo,
                    PersonRequestId = personRequestId,
                    IsManualDeny = true
                };

                _commandDispatcher.Execute(command);

                if (command.AffectedRequestId.HasValue) affectedRequestIds.Add(command.AffectedRequestId.Value);

                if (command.ErrorMessages != null)
                {
                    errorMessages.AddRange(command.ErrorMessages);
                }

            }

            return new RequestCommandHandlingResult(affectedRequestIds, errorMessages);
        }

        public RequestCommandHandlingResult CancelRequests(IEnumerable<Guid> requestIds)
        {
            var trackInfo = createTrackedCommandInfo();
            var affectedRequestIds = new List<Guid>();
            var errorMessages = new List<string>();

            foreach (var personRequestId in requestIds)
            {
                var command = new CancelAbsenceRequestCommand
                {
                    TrackedCommandInfo = trackInfo,
                    PersonRequestId = personRequestId
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
            }

            return new RequestCommandHandlingResult(affectedRequestIds, errorMessages);
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

			return new RequestCommandHandlingResult(new Guid[] { }, errorMessages, trackInfo.TrackId);
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