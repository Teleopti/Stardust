using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Util;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationServiceClient : INotificationServiceClient
	{
		public NotificationServiceClient()
		{
			
		}

		public bool SendEmail(SGMailMessage msg, string apiUri, string apiKey)
		{
			// POST  https://apiteleoptitest.azure-api.net/notify/asbq   // header   "Ocp-Apim-Subscription-Key"  : 2f0405da85a749999e1fc3fd71b2cd58
			try
			{
				//var configUri = "https://apiteleoptitest.azure-api.net/notify/asbq";
				//var configApiKey = "2f0405da85a749999e1fc3fd71b2cd58";
				var client = new HttpClient();


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

				var response = client.SendAsync(req).Result;
				if (!response.IsSuccessStatusCode)
				{
					// Log -> response.ReasonPhrase
				}

				// use response.StatusCode, response.Content, response.Headers ?
				return response.IsSuccessStatusCode;
			}
			catch (Exception)
			{
			}
			return false;
		}

		private Uri CreateUri(string uri)
		{
			return string.IsNullOrEmpty(uri) ? null : new Uri(uri, UriKind.RelativeOrAbsolute);
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