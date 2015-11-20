using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class GetHttpRequest : IGetHttpRequest
	{
		public T Get<T>(string url, NameValueCollection arguments, string userAgent = null)
		{
			url = appendQueryStringArguments(url, arguments);

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = userAgent;

			return request.GetRequest<T>();
		}

		private static string appendQueryStringArguments(string url, NameValueCollection arguments)
		{
			var builder = new StringBuilder(url);
			builder.Append(url.Contains("?") ? "&" : "?");
			foreach (var argument in arguments.AllKeys)
			{
				builder.Append(argument);
				builder.Append("=");
				builder.Append(Uri.EscapeDataString(arguments[argument]));
				builder.Append("&");
			}
			url = builder.ToString();
			return url;
		}

		public T GetSecured<T>(string url, NameValueCollection arguments, TenantCredentials tenantCredentials)
		{
			url = appendQueryStringArguments(url, arguments);

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Headers.Add("personid", tenantCredentials.PersonId.ToString());
			request.Headers.Add("tenantpassword", tenantCredentials.TenantPassword);
			return request.GetRequest<T>();
		}
	}
}