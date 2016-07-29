﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ApproveRequestsWithValidatorsEventHandler : IHandleEvent<ApproveRequestsWithValidatorsEvent>,
		IRunOnStardust
	{
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAbsenceRequestProcessor _absenceRequestProcessor;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IList<BeforeApproveAction> _beforeApproveActions;
		private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		private readonly IMessageBrokerComposite _messageBroker;

		public ApproveRequestsWithValidatorsEventHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IAbsenceRequestProcessor absenceRequestProcessor, IPersonRequestRepository personRequestRepository,
			IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator,
			IMessageBrokerComposite messageBroker)
		{
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_absenceRequestProcessor = absenceRequestProcessor;
			_personRequestRepository = personRequestRepository;
			_writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
			_messageBroker = messageBroker;
			_beforeApproveActions = new List<BeforeApproveAction>
			{
				checkPersonRequest,
				checkAbsenceRequest
			};
		}

		[AsSystem]
		public virtual void Handle(ApproveRequestsWithValidatorsEvent @event)
		{
			var validators = getAbsenceRequestValidators(@event.Validator).ToArray();
			foreach (var personRequestId in @event.PersonRequestIdList)
			{
				using (var unitOfWork = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					var personRequest = _personRequestRepository.Get(personRequestId);

					if (!requestIsOkForBasicValidation(personRequest))
					{
						continue;
					}

					if (_beforeApproveActions.Any(action => action.Invoke(personRequest)))
					{
						continue;
					}

					var absenceRequest = personRequest.Request as IAbsenceRequest;
					_absenceRequestProcessor.ApproveAbsenceRequestWithValidators(personRequest, absenceRequest,
						unitOfWork, validators);
				}
			}

			@event.EndTime = DateTime.Now;
			sendMessage(@event);
		}

		private bool requestIsOkForBasicValidation(IPersonRequest personRequest)
		{
			var result = _writeProtectedScheduleCommandValidator.ValidateCommand(personRequest.RequestedDate,
				personRequest.Person, new ApproveBatchRequestsCommand());
			return result;
		}

		private static bool checkPersonRequest(IPersonRequest personRequest)
		{
			return (personRequest == null || !(personRequest.IsPending || personRequest.IsWaitlisted));
		}

		private static bool checkAbsenceRequest(IPersonRequest personRequest)
		{
			return !(personRequest.Request is IAbsenceRequest);
		}

		private static IEnumerable<IAbsenceRequestValidator> getAbsenceRequestValidators(RequestValidatorsFlag validator)
		{
			if (validator.HasFlag(RequestValidatorsFlag.BudgetAllotmentValidator))
				yield return new BudgetGroupHeadCountValidator();
			if (validator.HasFlag(RequestValidatorsFlag.IntradayValidator))
				yield return new StaffingThresholdValidator();
		}

		private delegate bool BeforeApproveAction(IPersonRequest personRequest);

		private void sendMessage(ApproveRequestsWithValidatorsEvent @event)
		{
			_messageBroker.Send(
				@event.LogOnDatasource,
				@event.LogOnBusinessUnitId,
				@event.StartTime,
				@event.EndTime,
				Guid.Empty,
				@event.InitiatorId,
				typeof(Person),
				Guid.Empty,
				typeof(IApproveRequestsWithValidatorsEventMessage),
				DomainUpdateType.NotApplicable,
				null,
				@event.TrackedCommandInfo.TrackId);
		}
	}
}
