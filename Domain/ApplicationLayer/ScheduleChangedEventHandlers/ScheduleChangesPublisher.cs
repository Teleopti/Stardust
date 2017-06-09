using System.Collections.Specialized;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[EnabledBy(Toggles.LastHandlers_ToHangfire_41203)]
	public class ScheduleChangesPublisherHangfire :
		ScheduleChangesPublisher,
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		public ScheduleChangesPublisherHangfire(IHttpServer server, INow now, IGlobalSettingDataRepository settingsRepository, SignatureCreator signatureCreator) : base(server, now, settingsRepository, signatureCreator)
		{
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			HandleBase(@event);
		}
	}

	[DisabledBy(Toggles.LastHandlers_ToHangfire_41203)]
	public class ScheduleChangesPublisherServiceBus :
		ScheduleChangesPublisher,
		IHandleEvent<ProjectionChangedEvent>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		public ScheduleChangesPublisherServiceBus(IHttpServer server, INow now, IGlobalSettingDataRepository settingsRepository, SignatureCreator signatureCreator) : base(server, now, settingsRepository, signatureCreator)
		{
		}

		public void Handle(ProjectionChangedEvent @event)
		{
			HandleBase(@event);
		}
	}

	public class ScheduleChangesPublisher
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

		public void HandleBase(ProjectionChangedEvent @event)
		{
			if (!@event.IsDefaultScenario) return;

			var settings = _settingsRepository.FindValueByKey(ScheduleChangeSubscriptions.Key, new ScheduleChangeSubscriptions());
			var scheduleChangeListeners = settings.Subscriptions();
			if (scheduleChangeListeners.Length == 0) return;

			var currentDate = _now.ServerDate_DontUse();
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
							logger.ErrorFormat($"Couldn't send schedule change notification for person ({@event.PersonId}) to subscriber {scheduleChangeSubscription.Name} ({scheduleChangeSubscription.Uri}).",
								t.Exception.Flatten());
						}
					});
			}
		}
	}
}