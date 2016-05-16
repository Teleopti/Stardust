using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RunWaitlistCommandHandler : IHandleCommand<RunWaitlistCommand>
	{
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
			var @event = new RunRequestWaitlistEvent
			{
				InitiatorId = command.TrackedCommandInfo.TrackId,
				JobName = "",
				Period = command.Period,
				LogOnBusinessUnitId = _currentBusinessUnit.Current().Id.GetValueOrDefault(),
				LogOnDatasource = _currentDataSource.Current().DataSourceName,
				Timestamp = DateTime.UtcNow
			};
			_publisher.Publish(@event);
		}
	}
}