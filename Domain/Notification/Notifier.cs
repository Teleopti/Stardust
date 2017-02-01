using System.Collections.Specialized;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Notification
{
	public class Notifier : INotifier
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Notifier));
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly INotificationChecker _notificationChecker;
		private readonly IHttpServer _httpServer;
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private static bool alreadyWarned;

		public Notifier(INotificationSenderFactory notificationSenderFactory, INotificationChecker notificationChecker, IHttpServer httpServer, IPersonalSettingDataRepository personalSettingDataRepository)
		{
			_notificationSenderFactory = notificationSenderFactory;
			_notificationChecker = notificationChecker;
			_httpServer = httpServer;
			_personalSettingDataRepository = personalSettingDataRepository;
		}
		
		public void Notify(INotificationMessage messages, params IPerson[] persons)
		{
			var sender = _notificationSenderFactory.GetSender();
			if (sender != null)
			{
				var lookup = _notificationChecker.Lookup();
				var emailSender = lookup.EmailSender;
				foreach (var person in persons)
				{
					sender.SendNotification(messages, new NotificationHeader
					{
						EmailSender = emailSender,
						MobileNumber = lookup.SmsMobileNumber(person),
						EmailReceiver = person.Email,
						PersonName = person.Name.ToString()
					});
				}
			}
			else
			{
				if (!alreadyWarned)
				{
					logger.Warn("No notification sender was found. Review the configuration.");
					alreadyWarned = true;
				}
			}

			tryToSendToAppSubscriptions(messages, persons);
		}

		private void tryToSendToAppSubscriptions(INotificationMessage messages, IPerson[] persons)
		{
			foreach (var person in persons)
			{
				var setting = _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(UserDevices.Key, person, new UserDevices());
				if (!setting.TokenList.Any()) continue;

				foreach (var token in setting.TokenList)
				{
					_httpServer.Post("https://fcm.googleapis.com/fcm/send", new {to=token,notification=new {title=messages.Subject, body=string.Join(" ",messages.Messages)} },
						s => new NameValueCollection {{"Authorization", "key=AAAANvMkWNA:APA91bG1pR8ZVsp-S98uWsFUE5lnQiC8UnsQL3DgN6Vyw5HyaKuqVt86kdeurfLfQkWt_7kZTgXcTuAaxvcVUkjtE8jFo72loTy6UYrLrVbYnqCXVI4mWCYhvLQnU3Sv0sIfW1k-eZCu" } });
				}
			}
		}
	}
}