using System;
using System.Configuration;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960, Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class QueuedAbsenceRequestFastIntradayHandler : QueuedAbsenceRequestHandlerBase, IHandleEvent<NewAbsenceRequestCreatedEvent>, IHandleEvent<RequestPersonAbsenceRemovedEvent>
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(QueuedAbsenceRequestHandler));

		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IConfigReader _configReader;
		private readonly IntradayRequestProcessor _intradayRequestProcessor;


		public QueuedAbsenceRequestFastIntradayHandler(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IAbsenceRequestCancelService absenceRequestCancelService, IConfigReader configReader, IntradayRequestProcessor intradayRequestProcessor)
			: base(personRequestRepository, queuedAbsenceRequestRepository, absenceRequestCancelService)
		{
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_configReader = configReader;
			_intradayRequestProcessor = intradayRequestProcessor;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public new virtual void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			IPersonRequest personRequest = CheckPersonRequest(@event.PersonRequestId);
			if (personRequest == null)
				return;

			var startDateTime = DateTime.UtcNow;

			var fakeIntradayStartUtcDateTime = _configReader.AppConfig("FakeIntradayUtcStartDateTime");
			if (fakeIntradayStartUtcDateTime != null)
			{
				try
				{
					startDateTime = DateTime.ParseExact(fakeIntradayStartUtcDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture).Utc();
				}
				catch
				{
					logger.Warn("The app setting 'FakeIntradayStartDateTime' is not specified correctly. Format your datetime as 'yyyy-MM-dd HH:mm' ");
				}
			}

			var intradayPeriod = new DateTimePeriod(startDateTime, startDateTime.AddHours(24));


			if (intradayPeriod.Contains(personRequest.Request.Period.StartDateTime) && intradayPeriod.Contains(personRequest.Request.Period.EndDateTime))
			{
				_intradayRequestProcessor.Process(personRequest);
			}
			else
			{
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
	}

	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960), DisabledBy(Toggles.AbsenceRequests_SpeedupIntradayRequests_40754)]
	public class QueuedAbsenceRequestHandler : QueuedAbsenceRequestHandlerBase, IHandleEvent<NewAbsenceRequestCreatedEvent>, IHandleEvent<RequestPersonAbsenceRemovedEvent>
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;


		public QueuedAbsenceRequestHandler(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IAbsenceRequestCancelService absenceRequestCancelService)
			: base(personRequestRepository, queuedAbsenceRequestRepository, absenceRequestCancelService)
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


	[EnabledBy(Toggles.AbsenceRequests_UseMultiRequestProcessing_39960)]
	public class QueuedAbsenceRequestHandlerBase : INewAbsenceRequestHandler, IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(QueuedAbsenceRequestHandler));

		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IAbsenceRequestCancelService _absenceRequestCancelService;

		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();

		public QueuedAbsenceRequestHandlerBase(IPersonRequestRepository personRequestRepository, IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IAbsenceRequestCancelService absenceRequestCancelService)
		{
			_personRequestRepository = personRequestRepository;
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_absenceRequestCancelService = absenceRequestCancelService;
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
			return personRequest;
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !obj.IsNew);
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

		public void Handle(NewAbsenceRequestCreatedEvent @event)
		{
			throw new NotImplementedException();
		}
	}

}
