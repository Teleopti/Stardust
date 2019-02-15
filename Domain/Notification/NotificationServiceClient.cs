using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Util;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationServiceClient : INotificationServiceClient
	{
		private HttpClient client;

		private readonly ILog _logger = LogManager.GetLogger(typeof(NotificationServiceClient));

		public NotificationServiceClient() : this(new HttpClient()) { }
		public NotificationServiceClient(HttpClient client)
		{
			this.client = client;
		}

		public NotificationSubscriptionMessage CreateSubscription(string tenantName, string apiUri, string apiKey, bool isCloudTenant)
		{
			var nsm = new NotificationSubscriptionMessage();
			try
			{
				var prefix = isCloudTenant ? "Cloud_" : "OnPrem_";
				tenantName = prefix + tenantName.Replace(" ", "");
				
				var req = new HttpRequestMessage(HttpMethod.Get, CreateUri($"{apiUri}?tenant={tenantName}"));

				var headers = new Dictionary<string, string>
				{
					{"Ocp-Apim-Subscription-Key", apiKey},
					{"Accept", "application/json"}
				};

				headers.ForEach(x => client.DefaultRequestHeaders.Add(x.Key, x.Value));

				var response = client.SendAsync(req).GetAwaiter().GetResult();
				if (!response.IsSuccessStatusCode)
				{
					// Log -> response.ReasonPhrase
				}

				var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				nsm = JsonConvert.DeserializeObject<NotificationSubscriptionMessage>(responseString);
				
				return nsm;
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}

			return nsm;
		}

		public bool SendEmail(SGMailMessage msg, string apiUri, string apiKey)
		{
			// POST  https://apiteleoptitest.azure-api.net/notify/asbq   // header   "Ocp-Apim-Subscription-Key"  : 2f0405da85a749999e1fc3fd71b2cd58
			try
			{
				//ApplicationConfig
				//var apiUrl = _serverConfiguration.Get("notification.apiEndpoint");
				//pi.Tenant.ApplicationConfig.TryGetValue("notification.apiKey", out var apiKey);

				//var configUri = "https://apiteleoptitest.azure-api.net/notify/asbq";
				//var configApiKey = "2f0405da85a749999e1fc3fd71b2cd58";

				var content = new StringContent(msg.ToJson(), Encoding.UTF8, msg.ContentType);


				var req = new HttpRequestMessage(HttpMethod.Post, CreateUri(apiUri))
				{
					Method = HttpMethod.Post,
					RequestUri = CreateUri(apiUri),
					Content = content
				};

				var headers = new Dictionary<string, string>
				{
					{"Ocp-Apim-Subscription-Key", apiKey},
					{"Accept", "application/json"}
				};

				headers.ForEach(x => client.DefaultRequestHeaders.Add(x.Key, x.Value));

				var response = client.SendAsync(req).GetAwaiter().GetResult();
				if (!response.IsSuccessStatusCode)
				{
					// Log -> response.ReasonPhrase
				}

				// use response.StatusCode, response.Content, response.Headers ?
				return response.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
			return false;
		}

		private Uri CreateUri(string uri)
		{
			return string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
		}
	}

	public class FakeNotificationServiceClient : INotificationServiceClient
	{
		public NotificationSubscriptionMessage CreateSubscription(string tenantName, string apiUri, string apiKey, bool isCloudTenant)
		{
			return new NotificationSubscriptionMessage { subscriptionKey = "thekey" };
		}

		public bool SendEmail(SGMailMessage msg, string apiUri, string apiKey)
		{
			return true;
		}
	}

	public class SGMailMessage
	{
		public string To { get; set; }
		public string ToFullName { get; set; }
		public string From { get; set; }
		public string FromFullName { get; set; }
		public string Subject { get; set; }
		public string ContentType { get; set; }
		public string ContentValue { get; set; }
	}
}