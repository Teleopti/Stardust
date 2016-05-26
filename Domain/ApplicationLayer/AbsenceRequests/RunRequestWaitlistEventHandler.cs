using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
    public class RunRequestWaitlistEventHandler : IHandleEvent<RunRequestWaitlistEvent>, IRunOnStardust
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RunRequestWaitlistEventHandler));
        private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private readonly IAbsenceRequestWaitlistProcessor _processor;
        private readonly IWorkflowControlSetRepository _wcsRepository;
        private readonly IMessageBrokerComposite _messageBroker;

        public RunRequestWaitlistEventHandler(ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
            IAbsenceRequestWaitlistProcessor processor,
            IWorkflowControlSetRepository wcsRepository, IMessageBrokerComposite messageBroker)
        {
            _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
            _processor = processor;
            _wcsRepository = wcsRepository;
            _messageBroker = messageBroker;
        }

        [AsSystem]
        public virtual void Handle(RunRequestWaitlistEvent @event)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(
                    "Consuming event for running request waitlist with "
                    + $"StartTime=\"{@event.StartTime}\", EndTime=\"{@event.EndTime}\" "
                    + $"(Message timestamp=\"{@event.Timestamp}\")");
            }

            using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
            {
                var period = new DateTimePeriod(@event.StartTime, @event.EndTime);
                var workflowControlSets = _wcsRepository.LoadAll();



                foreach (var wcs in workflowControlSets)
                {
                    _processor.ProcessAbsenceRequestWaitlist(uow, period, wcs);
                }
            }

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