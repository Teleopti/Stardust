using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotifyAppSubscriptions
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NotifyAppSubscriptions));
		private readonly IHttpServer _httpServer;
		private readonly IPersonalSettingDataRepository _personalSettingDataRepository;
		private readonly IConfigReader _configReader;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		public NotifyAppSubscriptions(IHttpServer httpServer, IPersonalSettingDataRepository personalSettingDataRepository,
			IConfigReader configReader, ICurrentDataSource dataSource, ICurrentBusinessUnit currentBusinessUnit)
		{
			_httpServer = httpServer;
			_personalSettingDataRepository = personalSettingDataRepository;
			_configReader = configReader;
			_dataSource = dataSource;
			_currentBusinessUnit = currentBusinessUnit;
		}

		public async Task<bool> TrySend(INotificationMessage messages, IPerson[] persons)
		{
			var key = _configReader.AppConfig("FCM");
			if (string.IsNullOrEmpty(key)) return false;
			var hasFail = false;
			foreach (var person in persons)
			{
				var setting =
					_personalSettingDataRepository.FindValueByKeyAndOwnerPerson(UserDevices.Key, person, new UserDevices());
				if (!setting.TokenList.Any()) continue;

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Trying to send notification for Person {0} using token {1}", person.Id,
						string.Join(", ", setting.TokenList));
				}

				try
				{
					var logOnContext =
						new InputWithLogOnContext(_dataSource.CurrentName(), _currentBusinessUnit.Current().Id.GetValueOrDefault());
					var notification = new { title = messages.Subject, body = string.Join(" ", messages.Messages) };

					dynamic requestBody = new ExpandoObject();
					requestBody.registration_ids = setting.TokenList;
					requestBody.notification = notification;
					if (messages.Data == null)
					{
						messages.Data = new ExpandoObject();
					}
					messages.Data.title = notification.title;
					messages.Data.body = notification.body;
					requestBody.data = (object)messages.Data;
					var responseMessage = await _httpServer.Post("https://fcm.googleapis.com/fcm/send",
						(object)requestBody,
						s => new NameValueCollection { { "Authorization", key } });

					var invalidTokens = await getUserDevicesInvalidTokenAsync(setting.TokenList, responseMessage);
					if (invalidTokens.IsNullOrEmpty())
					{
						continue;
					}
					PersistUserDevicesSettingValue(logOnContext, person, invalidTokens);
				}
				catch (Exception e)
				{
					logger.Error($"Send notification faild. person: {person.Name}, token list: {string.Join(", ", setting.TokenList)}", e);
					hasFail = true;
				}
			}
			return !hasFail;
		}


		private async Task<List<string>> getUserDevicesInvalidTokenAsync(IEnumerable<string> tokenList, HttpResponseMessage responseMessage)
		{
			var invalidTokens = new List<string>();
			var tokens = tokenList.ToArray();
			var isJson = responseMessage?.Content?.Headers.ContentType.MediaType == "application/json";
			if (!isJson)
			{
				return invalidTokens;
			}
			var content = await responseMessage.Content.ReadAsStringAsync();
			if (!content.IsNullOrEmpty())
			{
				var dsResponse = JsonConvert.DeserializeObject<FCMSendMessageResponse>(content);
				if (dsResponse?.failure > 0)
				{

					var i = 0;
					foreach (var mr in dsResponse.results)
					{
						if (!mr.error.IsNullOrEmpty())
						{
							invalidTokens.Add(tokens[i]);
						}
						i++;
					}
				}
			}
			return invalidTokens;

		}

		[AsSystem, UnitOfWork]
		protected virtual void PersistUserDevicesSettingValue(InputWithLogOnContext logOnContext, IPerson person, List<string> invalidTokens)
		{

			var setting =
				_personalSettingDataRepository.FindValueByKeyAndOwnerPerson(UserDevices.Key, person, new UserDevices());

			setting.RemoveToken(invalidTokens.ToArray());

			_personalSettingDataRepository.PersistSettingValue(UserDevices.Key, setting);
		}

	}
}