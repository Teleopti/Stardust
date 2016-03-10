using System;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangesPublisher : IHandleEvent<ProjectionChangedEvent>, IRunOnServiceBus
	{
		private readonly IHttpServer _server;
		private readonly ISettingDataRepository _settingsRepository;
		private readonly ILog logger = LogManager.GetLogger(typeof (ScheduleChangesPublisher));

		public ScheduleChangesPublisher(IHttpServer server, ISettingDataRepository settingsRepository)
		{
			_server = server;
			_settingsRepository = settingsRepository;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			var settings = _settingsRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions());
			foreach (var scheduleChangeSubscription in settings.Subscriptions())
			{
				try
				{
					_server.PostOrThrow(scheduleChangeSubscription.Uri.ToString(), @event);
				}
				catch (Exception exception)
				{
					logger.ErrorFormat(
						string.Format("Couldn't send schedule change notification for person ({0}) to subscriber {1} ({2}).",
							@event.PersonId, scheduleChangeSubscription.Name, scheduleChangeSubscription.Uri), exception);
				}
			}
		}
	}
}