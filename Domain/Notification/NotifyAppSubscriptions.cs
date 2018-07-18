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
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotifyAppSubscriptions
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(NotifyAppSubscriptions));
		private readonly IHttpServer _httpServer;
		private readonly UserDeviceService _userDeviceService;
		private readonly IConfigReader _configReader;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly string _key;

		public NotifyAppSubscriptions(
			IHttpServer httpServer,
			UserDeviceService userDeviceService,
			IConfigReader configReader,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit currentBusinessUnit)
		{
			_httpServer = httpServer;
			_userDeviceService = userDeviceService;
			_configReader = configReader;
			_dataSource = dataSource;
			_currentBusinessUnit = currentBusinessUnit;
			_key = _configReader.AppConfig("FCM");
		}

		public async Task<bool> TrySend(INotificationMessage message, IPerson[] persons)
		{
			if (string.IsNullOrEmpty(_key)) return false;

			var hasFail = false;
			var invalidTokens = new List<string>();

			var logOnContext =
					new InputWithLogOnContext(_dataSource.CurrentName(), _currentBusinessUnit.Current().Id.GetValueOrDefault());


			foreach (var person in persons)
			{
				var tokens = _userDeviceService.GetUserTokens(person).ToArray();
				if (!tokens.Any()) continue;

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Trying to send notification for Person {0} using token {1}", person.Id,
						string.Join(", ", tokens));
				}

				try
				{
					await sendNotificationAndCollectInvalidTokensAsync(message, tokens, invalidTokens);
				}
				catch (Exception e)
				{
					logger.Error($"Send notification faild. person: {person.Name}, token list: {string.Join(", ", tokens)}", e);
					hasFail = true;
				}
			}

			if (!invalidTokens.IsNullOrEmpty())
			{
				try { removeInvalidTokens(logOnContext, invalidTokens.ToArray()); }
				catch (DataSourceException) { }
			}

			return !hasFail;
		}

		private async Task sendNotificationAndCollectInvalidTokensAsync(INotificationMessage message, string[] tokens, List<string> invalidTokens)
		{
			var requestBody = getNotificationRequestBody(message, tokens);
			var responseMessage = await _httpServer.Post("https://fcm.googleapis.com/fcm/send",
				   (object)requestBody,
				   s => new NameValueCollection { { "Authorization", _key } });

			invalidTokens.AddRange(await getInvalidTokensAsync(tokens, responseMessage));
		}

		private dynamic getNotificationRequestBody(INotificationMessage message, string[] tokens)
		{
			var notification = new { title = message.Subject, body = string.Join(" ", message.Messages) };

			dynamic requestBody = new ExpandoObject();
			requestBody.registration_ids = tokens;
			requestBody.notification = notification;
			if (message.Data == null)
			{
				message.Data = new ExpandoObject();
			}

			message.Data.title = notification.title;
			message.Data.body = notification.body;
			requestBody.data = (object)message.Data;

			return requestBody;
		}


		private async Task<string[]> getInvalidTokensAsync(string[] tokens, HttpResponseMessage responseMessage)
		{
			var invalidTokens = new List<string>();
			var isJson = responseMessage?.Content?.Headers.ContentType.MediaType == "application/json";
			if (!isJson)
			{
				return invalidTokens.ToArray();
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
			return invalidTokens.ToArray();
		}

		[AsSystem, UnitOfWork]
		protected virtual void removeInvalidTokens(InputWithLogOnContext logOnContext, string[] invalidTokens)
		{
			_userDeviceService.Remove(invalidTokens);
		}
	}
}