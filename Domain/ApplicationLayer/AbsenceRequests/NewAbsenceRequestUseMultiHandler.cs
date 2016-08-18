using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class NewAbsenceRequestUseMultiHandler : INewAbsenceRequestHandler, IHandleEvent<NewAbsenceRequestCreatedEvent>, IRunOnStardust
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NewAbsenceRequestUseMultiHandler));
		
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IEventPublisher _eventPublisher;

		private readonly IList<LoadDataAction> _loadDataActions;
		private IPersonRequest _personRequest;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();

		private delegate bool LoadDataAction(NewAbsenceRequestCreatedEvent @event);

		public NewAbsenceRequestUseMultiHandler(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, 
			IEventPublisher eventPublisher)
		{
			_personRequestRepository = personRequestRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_eventPublisher = eventPublisher;

			_loadDataActions = new List<LoadDataAction>
			{
				checkPersonRequest,
				checkAbsenceRequest
			};
		}

		[AsSystem]
		public void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			if (_loadDataActions.Any(action => !action.Invoke(@event)))
			{
				return;
			}

			var queuedAbsenceRequest = new QueuedAbsenceRequest()
			{
				PersonRequest = _personRequest,
				Created = _personRequest.CreatedOn.Value,
				StartDateTime = _personRequest.Request.Period.StartDateTime,
				EndDateTime = _personRequest.Request.Period.EndDateTime,
			};
			_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
			var requestWithOverlappingPeriod = _queuedAbsenceRequestRepository.Find(_personRequest.Request.Period);

			if (requestWithOverlappingPeriod.Count >= 2)
			{
				var Ids = new List<Guid>();
				foreach (var req in requestWithOverlappingPeriod)
				{
					if (req.PersonRequest.Id != null)
						Ids.Add(req.PersonRequest.Id.Value);

					_queuedAbsenceRequestRepository.Remove(req);
				}
				var multiRequestEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = Ids
				};
				_eventPublisher.Publish(multiRequestEvent);
				
			}
		}

		private bool checkAbsenceRequest(NewAbsenceRequestCreatedEvent @event)
		{
			var req  = _personRequest.Request as IAbsenceRequest;
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

	}
}
