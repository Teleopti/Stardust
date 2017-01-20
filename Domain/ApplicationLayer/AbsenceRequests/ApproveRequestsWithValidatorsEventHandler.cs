using System;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
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
		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public ApproveRequestsWithValidatorsEventHandler(IPersonRequestRepository personRequestRepository,
			IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator,
			IMessageBrokerComposite messageBroker,
			IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_personRequestRepository = personRequestRepository;
			_writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
			_messageBroker = messageBroker;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		[AsSystem]
		public virtual void Handle(ApproveRequestsWithValidatorsEvent @event)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				foreach (var personRequestId in @event.PersonRequestIdList)
				{
					var personRequest = _personRequestRepository.Get(personRequestId);
					if (!isValidAbsenceRequest(personRequest)) continue;

					var queuedAbsenceRequest = new QueuedAbsenceRequest
					{
						PersonRequest = personRequest.Id.GetValueOrDefault(),
						Created = personRequest.CreatedOn.GetValueOrDefault(),
						StartDateTime = personRequest.Request.Period.StartDateTime,
						EndDateTime = personRequest.Request.Period.EndDateTime,
						MandatoryValidators = @event.Validator
					};
					_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
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
	}
}