using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ApproveRequestsWithValidatorsEventHandler : IHandleEvent<ApproveRequestsWithValidatorsEvent>,
		IRunOnStardust
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public ApproveRequestsWithValidatorsEventHandler(IPersonRequestRepository personRequestRepository,
			IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator,
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_personRequestRepository = personRequestRepository;
			_writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		[AsSystem]
		public virtual void Handle(ApproveRequestsWithValidatorsEvent @event)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var alreadyQueuedRequests = _queuedAbsenceRequestRepository.FindByPersonRequestIds(@event.PersonRequestIdList);

				foreach (var personRequestId in @event.PersonRequestIdList)
				{
					var personRequest = _personRequestRepository.Get(personRequestId);
					if (!isValidAbsenceRequest(personRequest)) continue;

					var validators = @event.Validator;
					addToQueuedRequests(alreadyQueuedRequests, personRequest, validators);
				}

				uow.PersistAll();
			}

			@event.EndTime = DateTime.Now;
		}

		private bool isValidAbsenceRequest(IPersonRequest personRequest)
		{
			if (!(personRequest?.Request is IAbsenceRequest)) return false;
			if (!personRequest.IsPending && !personRequest.IsWaitlisted) return false;

			return _writeProtectedScheduleCommandValidator.ValidateCommand(personRequest.RequestedDate,
				personRequest.Person, new ApproveBatchRequestsCommand());
		}

		private void addToQueuedRequests(IEnumerable<IQueuedAbsenceRequest> alreadyQueuedRequests, IPersonRequest personRequest,
			RequestValidatorsFlag validators)
		{
			// If the request already queued, then update it in QueuedAbsenceRequest
			var queuedAbsenceRequest = alreadyQueuedRequests.FirstOrDefault(x => x.PersonRequest == personRequest.Id);
			if (queuedAbsenceRequest != null)
			{
				queuedAbsenceRequest.Created = personRequest.CreatedOn.GetValueOrDefault();
				queuedAbsenceRequest.StartDateTime = personRequest.Request.Period.StartDateTime;
				queuedAbsenceRequest.EndDateTime = personRequest.Request.Period.EndDateTime;
				queuedAbsenceRequest.MandatoryValidators = validators;
			}
			else
			{
				_queuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
				{
					PersonRequest = personRequest.Id.GetValueOrDefault(),
					Created = personRequest.CreatedOn.GetValueOrDefault(),
					StartDateTime = personRequest.Request.Period.StartDateTime,
					EndDateTime = personRequest.Request.Period.EndDateTime,
					MandatoryValidators = validators
				});
			}
		}
	}
}