using System.Collections.Specialized;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangesPublisherHangfire :
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		private readonly IHttpServer _server;
		private readonly INow _now;
		private readonly IGlobalSettingDataRepository _settingsRepository;
		private readonly SignatureCreator _signatureCreator;
		private readonly ILog logger = LogManager.GetLogger(typeof(ScheduleChangesPublisherHangfire));

		public ScheduleChangesPublisherHangfire(IHttpServer server, INow now, IGlobalSettingDataRepository settingsRepository, SignatureCreator signatureCreator)
		{
			_server = server;
			_now = now;
			_settingsRepository = settingsRepository;
			_signatureCreator = signatureCreator;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		public virtual void Handle(ProjectionChangedEvent @event)
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
					c => new NameValueCollection { { "Signature", _signatureCreator.Create(c) } }).ContinueWith(t =>
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