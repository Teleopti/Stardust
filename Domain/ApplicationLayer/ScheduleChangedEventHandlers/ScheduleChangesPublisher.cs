using System.Collections.Specialized;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangesPublisher : IHandleEvent<ProjectionChangedEvent>, IRunOnServiceBus
	{
		private readonly IHttpServer _server;
		private readonly INow _now;
		private readonly IGlobalSettingDataRepository _settingsRepository;
		private readonly SignatureCreator _signatureCreator;
		private readonly ILog logger = LogManager.GetLogger(typeof (ScheduleChangesPublisher));

		public ScheduleChangesPublisher(IHttpServer server, INow now, IGlobalSettingDataRepository settingsRepository, SignatureCreator signatureCreator)
		{
			_server = server;
			_now = now;
			_settingsRepository = settingsRepository;
			_signatureCreator = signatureCreator;
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			if (!@event.IsDefaultScenario) return;

			var settings = _settingsRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions());
			var scheduleChangeListeners = settings.Subscriptions();
			if (scheduleChangeListeners.Length == 0) return;

			var currentDate = _now.LocalDateOnly();
			foreach (var scheduleChangeSubscription in scheduleChangeListeners)
			{
				var validRange = new DateOnlyPeriod(currentDate.AddDays(scheduleChangeSubscription.RelativeDateRange.Minimum),
					currentDate.AddDays(scheduleChangeSubscription.RelativeDateRange.Maximum));
				if (!@event.ScheduleDays.Any(s => validRange.Contains(new DateOnly(s.Date)))) continue;

				_server.PostOrThrowAsync(scheduleChangeSubscription.Uri.ToString(), @event,
					c => new NameValueCollection {{"Signature", _signatureCreator.Create(c)}}).ContinueWith(t =>
					{
						if (t.IsFaulted)
						{
							logger.ErrorFormat(
								string.Format(
									"Couldn't send schedule change notification for person ({0}) to subscriber {1} ({2}).",
									@event.PersonId, scheduleChangeSubscription.Name, scheduleChangeSubscription.Uri),
								t.Exception.Flatten());
						}
					});
			}
		}
	}
}