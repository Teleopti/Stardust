using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
    public class RunRequestWaitlistEventHandler : IHandleEvent<RunRequestWaitlistEvent>, IRunOnStardust
    {
	    private readonly IMultiAbsenceRequestsUpdater _absenceRequestsUpdater;
	    private readonly IPersonRequestRepository _personRequestRepository;
	    private readonly IMessageBrokerComposite _messageBroker;
	    private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

        public RunRequestWaitlistEventHandler(IMessageBrokerComposite messageBroker, 
			IMultiAbsenceRequestsUpdater absenceRequestsUpdater, IPersonRequestRepository personRequestRepository, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
        {
	        _messageBroker = messageBroker;
	        _absenceRequestsUpdater = absenceRequestsUpdater;
	        _personRequestRepository = personRequestRepository;
	        _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
        }

	    [AsSystem]
	    public virtual void Handle(RunRequestWaitlistEvent @event)
	    {
		    var period = new DateTimePeriod(@event.StartTime, @event.EndTime);
		    IList<Guid> requests;
		    using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
		    {
			     requests = _personRequestRepository.GetWaitlistRequests(period);
		    }
		    _absenceRequestsUpdater.UpdateAbsenceRequest(requests);

		    sendMessage(@event);
	    }

	    private void sendMessage(RunRequestWaitlistEvent @event)
        {
            _messageBroker.Send(
                @event.LogOnDatasource,
                @event.LogOnBusinessUnitId,
                @event.StartTime,
                @event.EndTime,
                Guid.Empty,
                @event.InitiatorId,
                typeof (Person),
                Guid.Empty,
                typeof (IRunRequestWaitlistEventMessage),
                DomainUpdateType.NotApplicable,
                null,
                @event.CommandId == Guid.Empty ? Guid.NewGuid() : @event.CommandId);
        }
    }
}