using System.Collections.Specialized;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotifyAppSubscriptions
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NotifyAppSubscriptions));
		private readonly IHttpServer _httpServer;
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private readonly IConfigReader _configReader;

		public NotifyAppSubscriptions(IHttpServer httpServer, IPersonalSettingDataRepository personalSettingDataRepository, IConfigReader configReader)
		{
			_httpServer = httpServer;
			_personalSettingDataRepository = personalSettingDataRepository;
			_configReader = configReader;
		}

		public void TrySend(INotificationMessage messages, IPerson[] persons)
		{
			var key = _configReader.AppConfig("FCM");
			if (string.IsNullOrEmpty(key)) return;

			foreach (var person in persons)
			{
				var setting = _personalSettingDataRepository.FindValueByKeyAndOwnerPerson(UserDevices.Key, person, new UserDevices());
				if (!setting.TokenList.Any()) continue;

				foreach (var token in setting.TokenList)
				{
					if (logger.IsDebugEnabled)
					{
						logger.DebugFormat("Trying to send notification for Person {0} using token {1}",person.Id,token);
					}
					_httpServer.Post("https://fcm.googleapis.com/fcm/send", new {to=token,notification=new {title=messages.Subject, body=string.Join(" ",messages.Messages)} },
						s => new NameValueCollection {{"Authorization", key } });
				}
			}
		}
	}
}