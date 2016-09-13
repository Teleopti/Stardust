using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class QueuedAbsenceRequestHandler : INewAbsenceRequestHandler, IHandleEvent<NewAbsenceRequestCreatedEvent>, IHandleEvent<RequestPersonAbsenceRemovedEvent>, IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(QueuedAbsenceRequestHandler));

		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IAbsenceRequestCancelService _absenceRequestCancelService;

		private readonly IList<LoadDataAction> _loadDataActions;
		private IPersonRequest _personRequest;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();

		private delegate bool LoadDataAction(NewAbsenceRequestCreatedEvent @event);

		public QueuedAbsenceRequestHandler(IPersonRequestRepository personRequestRepository,
												IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IAbsenceRequestCancelService absenceRequestCancelService)
		{
			_personRequestRepository = personRequestRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_absenceRequestCancelService = absenceRequestCancelService;

			_loadDataActions = new List<LoadDataAction>
			{
				checkPersonRequest,
				checkAbsenceRequest
			};
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			if (_loadDataActions.Any(action => !action.Invoke(@event)))
			{
				return;
			}
			var queuedAbsenceRequest = new QueuedAbsenceRequest()
			{
				PersonRequest = _personRequest.Id.GetValueOrDefault(),
				Created = _personRequest.CreatedOn.GetValueOrDefault(),
				StartDateTime = _personRequest.Request.Period.StartDateTime,
				EndDateTime = _personRequest.Request.Period.EndDateTime
			};
			_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
		}

		private bool checkAbsenceRequest(NewAbsenceRequestCreatedEvent @event)
		{
			var req = _personRequest.Request as IAbsenceRequest;
			if (absenceRequestSpecification.IsSatisfiedBy(req))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
									  @event.PersonRequestId);
				}
				return false;
			}
			return true;
		}

		private bool checkPersonRequest(NewAbsenceRequestCreatedEvent @event)
		{
			_personRequest = _personRequestRepository.Get(@event.PersonRequestId);
			if (personRequestSpecification.IsSatisfiedBy(_personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
						@event.PersonRequestId);
				}
				return false;
			}
			return true;
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !obj.IsNew);
			}
		}

		private class isNullSpecification : Specification<IAbsenceRequest>
		{
			public override bool IsSatisfiedBy(IAbsenceRequest obj)
			{
				return (obj == null);
			}
		}

		[ImpersonateSystem, UnitOfWork]
		public virtual void Handle(RequestPersonAbsenceRemovedEvent @event)
		{
			var personRequest = _personRequestRepository.Find(@event.PersonRequestId);
			var absenceRequest = personRequest?.Request as IAbsenceRequest;

			var personRequestId = Guid.Empty;

			if (absenceRequest != null)
			{
				personRequestId = personRequest.Id.GetValueOrDefault();
				_absenceRequestCancelService.CancelAbsenceRequest(absenceRequest);
			}

			var queuedAbsenceRequest = new QueuedAbsenceRequest()
			{
				PersonRequest = personRequestId,
				Created = DateTime.UtcNow,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime
			};
			_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
		}
	}
}
