using System;
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
	[EnabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class QueuedAbsenceRequestFastIntradayHandler : QueuedAbsenceRequestHandlerBase, IHandleEvent<NewAbsenceRequestCreatedEvent>
	{
		private readonly IAbsenceRequestIntradayFilter _absenceRequestIntradayFilter;


		public QueuedAbsenceRequestFastIntradayHandler(IPersonRequestRepository personRequestRepository,
			IAbsenceRequestIntradayFilter absenceRequestIntradayFilter) : base(personRequestRepository)
		{
			_absenceRequestIntradayFilter = absenceRequestIntradayFilter;
		}

		[AsSystem]
		[UnitOfWork]
		public new virtual void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			var personRequest = CheckPersonRequest(@event.PersonRequestId);
			if (personRequest == null)
				return;

			_absenceRequestIntradayFilter.Process(personRequest);
		}
	}

	[DisabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class QueuedAbsenceRequestHandler : QueuedAbsenceRequestHandlerBase, IHandleEvent<NewAbsenceRequestCreatedEvent>
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;


		public QueuedAbsenceRequestHandler(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository)
			: base(personRequestRepository)
		{
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public new virtual void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			var personRequest = CheckPersonRequest(@event.PersonRequestId);
			if (personRequest == null)
				return;

			var queuedAbsenceRequest = new QueuedAbsenceRequest()
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				Created = personRequest.CreatedOn.GetValueOrDefault(),
				StartDateTime = personRequest.Request.Period.StartDateTime,
				EndDateTime = personRequest.Request.Period.EndDateTime
			};
			_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);

		}
	}


	public class QueuedAbsenceRequestHandlerBase : INewAbsenceRequestHandler, IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(QueuedAbsenceRequestHandler));

		private readonly IPersonRequestRepository _personRequestRepository;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();

		public QueuedAbsenceRequestHandlerBase(IPersonRequestRepository personRequestRepository)
		{
			_personRequestRepository = personRequestRepository;
		}


		public IPersonRequest CheckPersonRequest(Guid requestId)
		{
			IPersonRequest personRequest = _personRequestRepository.Get(requestId);
			
			if (personRequestSpecification.IsSatisfiedBy(personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
						requestId);
				}
				return null;
			}
			if (!(personRequest.Request is IAbsenceRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"Request is not an absence request! (PersonRequestId = {0})",
						requestId);
				}
				return null;
			}
			personRequest.Pending(); 
			return personRequest;
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !obj.IsNew);
			}
		}

		public void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			throw new NotImplementedException();
		}
	}

}
