using System;
using System.IO;
using System.Net;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace Teleopti.Analytics.Portal.Utils
{
	public class TenantLogin
	{
		public bool TryApplicationLogon(ApplicationLogonClientModel applicationLogonClientModel)
		{
			var json = JsonConvert.SerializeObject(applicationLogonClientModel);
			var server = WebConfigurationManager.AppSettings["TenantServer"];
			var request = (HttpWebRequest)WebRequest.Create(server + "Authenticate/ApplicationLogon");
			request.UserAgent = "PM";

			var result = request.PostRequest<AuthenticationQueryResult>(json);
			
			return result.Success;
		} 
	}

	public class ApplicationLogonClientModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}

	public class AuthenticationQueryResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		//public string DataSourceConfiguration { get; set; }
	}

	public static class JsonExtensions
	{
		public static T ExecuteJsonRequest<T>(this UriBuilder uriBuilder)
		{
			var request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
			request.AllowAutoRedirect = false;

			return responseToJson<T>(request);
		}

		public static T PostRequest<T>(this HttpWebRequest request, string json)
		{
			request.AllowAutoRedirect = false;
			request.Method = "POST";
			request.ContentType = "application/json";

			using (var requestWriter = new StreamWriter(request.GetRequestStream()))
			{
				requestWriter.Write(json);
			}

			return responseToJson<T>(request);
		}

		private static T responseToJson<T>(WebRequest request)
		{
			using (var response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					var jsonString = reader.ReadToEnd();
					return JsonConvert.DeserializeObject<T>(jsonString);
				}
			}
		}
	}
}