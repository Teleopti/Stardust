﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RequestPersonAbsenceRemovedEventHandler : IHandleEvent<RequestPersonAbsenceRemovedEvent>, IRunOnHangfire
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly ICommandDispatcher _commandDispatcher;


		public RequestPersonAbsenceRemovedEventHandler(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IWorkflowControlSetRepository workflowControlSetRepository, ICommandDispatcher commandDispatcher)
		{
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_workflowControlSetRepository = workflowControlSetRepository;
			_commandDispatcher = commandDispatcher;
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(RequestPersonAbsenceRemovedEvent @event)
		{
			if (_workflowControlSetRepository.LoadAll().Any(w => w.AbsenceRequestWaitlistEnabled))
			{
				queueAbsenceRequest(@event);
			}
		}

		private void queueAbsenceRequest(RequestPersonAbsenceRemovedEvent @event)
		{
			var queuedAbsenceRequest = new QueuedAbsenceRequest()
			{
				PersonRequest = Guid.Empty,
				Created = DateTime.UtcNow,
				StartDateTime = @event.StartDateTime,
				EndDateTime = @event.EndDateTime
			};
			_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
		}

	}
}