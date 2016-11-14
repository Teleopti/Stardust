﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RequestPersonAbsenceRemovedEventHandler : IHandleEvent<RequestPersonAbsenceRemovedEvent>, IRunOnHangfire
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IAbsenceRequestCancelService _absenceRequestCancelService;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;



		public RequestPersonAbsenceRemovedEventHandler(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IPersonRequestRepository personRequestRepository, IAbsenceRequestCancelService absenceRequestCancelService, IWorkflowControlSetRepository workflowControlSetRepository)
		{
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_personRequestRepository = personRequestRepository;
			_absenceRequestCancelService = absenceRequestCancelService;
			_workflowControlSetRepository = workflowControlSetRepository;
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

			if (_workflowControlSetRepository.LoadAll().Any(w => w.AbsenceRequestWaitlistEnabled))
			{
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
}