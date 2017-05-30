using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
    public class RunWaitlistCommandHandler : IHandleCommand<RunWaitlistCommand>
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RunWaitlistCommandHandler));
        private readonly ICurrentBusinessUnit _currentBusinessUnit;
        private readonly ICurrentDataSource _currentDataSource;
        private readonly IEventPublisher _publisher;

        public RunWaitlistCommandHandler(ICurrentBusinessUnit currentCurrentBusinessUnit,
            ICurrentDataSource currentDataSource, IEventPublisher publisher)
        {
            _currentBusinessUnit = currentCurrentBusinessUnit;
            _currentDataSource = currentDataSource;
            _publisher = publisher;
        }

        public void Handle(RunWaitlistCommand command)
        {
            var trackInfo = command.TrackedCommandInfo;

            if (logger.IsDebugEnabled)
            {
                logger.Debug($"Handle RunWaitlistCommand with TrackId=\"{trackInfo.TrackId}\", "
                    + $"OperatedPersonId=\"{trackInfo.OperatedPersonId}\", Period=\"{command.Period}\"");
            }


			//Should not send event, should put in a placeholder for each day of period. Let's do it in utc
			//Maybe rename to triggerWaitlist instead of RunWaitlist.. 
            var @event = new RunRequestWaitlistEvent
            {
                InitiatorId = trackInfo.OperatedPersonId,
                JobName = "Run Request Waitlist",
                StartTime = command.Period.StartDateTime,
                EndTime = command.Period.EndDateTime,
                LogOnBusinessUnitId = _currentBusinessUnit.Current().Id.GetValueOrDefault(),
                LogOnDatasource = _currentDataSource.Current().DataSourceName,
                Timestamp = DateTime.UtcNow,
                CommandId = command.CommandId
            };

            try
            {
                _publisher.Publish(@event);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to publish event for RunWaitlistCommand with TrackId=\""
                    + $"{command.TrackedCommandInfo.TrackId}\", InitiatorId=\"{@event.InitiatorId}\", "
                    + $"Period=\"{command.Period}\"", ex);
                command.ErrorMessages = new[] { ex.Message };
            }
        }
    }
}