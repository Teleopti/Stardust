using System;
using System.Linq;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class RequestPersonAbsenceRemovedEventHandler : IHandleEvent<RequestPersonAbsenceRemovedEvent>, IRunOnHangfire
	{
		private readonly IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private readonly IWorkflowControlSetRepository _workflowControlSetRepository;
		private readonly INow _now;
		
		public RequestPersonAbsenceRemovedEventHandler(IQueuedAbsenceRequestRepository queuedAbsenceRequestRepository, IWorkflowControlSetRepository workflowControlSetRepository, INow now)
		{
			_queuedAbsenceRequestRepository = queuedAbsenceRequestRepository;
			_workflowControlSetRepository = workflowControlSetRepository;
			_now = now;
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
			var start = @event.StartDateTime;
			var end = @event.EndDateTime;
			while (start < end)
			{
				if (start >= _now.UtcDateTime())
				{
					var queuedAbsenceRequest = new QueuedAbsenceRequest
					{
						PersonRequest = Guid.Empty,
						Created = DateTime.UtcNow,
						StartDateTime = start,
						EndDateTime = start.AddDays(1)
					};
					_queuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
				}
				
				start = start.AddDays(1);
			}
		}
	}
}